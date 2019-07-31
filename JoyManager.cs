using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TimeBoxJoy.MapConfig;
using TimeBoxJoy.Utils;

namespace TimeBoxJoy
{
    public class JoyManager
    {
        private List<BTDeviceInfo> timeBoxJoyList;
        private BluetoothComponent blueComponent;
        private BluetoothClient blueClient;
        private DateTime startScan = DateTime.Now;
        List<string> Messages = new List<string>();
        private object locker = new object();
        public TimeSpan ScanBlueTimeOut = TimeSpan.FromSeconds(30);
        public int State = 0;//0=STOP,1=SCANNING

        public List<DefaultMapConfig> mapConfigs { get; set; }
        public Action<List<BTDeviceInfo>> OnJoyStateChange { get; set; } = null;
        public Action<Byte[]> OnJoyMessageReceive { get; set; } = null;
        public Action<string> OnMessage { get; set; } = null;
        public Action OnStartScanDevice { get; set; } = null;
        public Action OnEndScanDevice { get; set; } = null;
        public JoyManager()
        {
            //读取所有配置文件
            mapConfigs = new List<DefaultMapConfig>();
            if (!Directory.Exists("maps"))
            {
                Directory.CreateDirectory("maps");
            }
            mapConfigs.Add(new KeyBoardConfig());
            mapConfigs.Add(new XInputConfig());
            foreach(var file in Directory.GetFiles("maps"))
            {
                var _config=DefaultMapConfig.GetConfig(file);
                //FileInfo finfo = new FileInfo(file);
                _config.Name = System.IO.Path.GetFileNameWithoutExtension(file);
                if (!mapConfigs.Any(p=> p.Name== _config.Name))
                {
                    mapConfigs.Add(_config);
                }
            }

            //初始化蓝牙设备
            timeBoxJoyList = new List<BTDeviceInfo>();
            blueClient = new BluetoothClient();
            blueComponent = new BluetoothComponent(blueClient);
            blueComponent.DiscoverDevicesProgress += (sender, e) => {
                foreach (var device in e.Devices)
                {
                    if (device.DeviceName == "timebox" && !this.timeBoxJoyList.Any(p => p.bluetoothDeviceInfo.DeviceAddress.ToString() == device.DeviceAddress.ToString()))
                    {
                        this.timeBoxJoyList.Add(new BTDeviceInfo(device));
                    }
                }
                this.OnJoyStateChange?.Invoke(this.timeBoxJoyList);
            };
            blueComponent.DiscoverDevicesComplete += (sender, e) => {
                if ((DateTime.Now - startScan) < ScanBlueTimeOut)
                {
                    blueComponent.DiscoverDevicesAsync(255, true, true, false, true, null);
                }
                else
                {
                    this.State = 0;
                    this.OnEndScanDevice?.Invoke();
                }
            };
            //启动自动扫描手柄状态线程
            Thread tr = new Thread(new ThreadStart(() => {
                ScanJoyConnection();
            }));
            tr.Start();


        }

        public void ScanBTDevice()
        {
            if (this.State == 0)
            {
                OnStartScanDevice?.Invoke();
                this.timeBoxJoyList.RemoveAll(i => i.State == deviceState.DISCONNECT);
                this.State = 1;
                startScan = DateTime.Now;
                blueComponent.DiscoverDevicesAsync(30, true, true, true, true, null);
            }
        }
        private void ConnectJoy(BTDeviceInfo joy)
        {
            JoyStick joyst = new JoyStick(joy.bluetoothDeviceInfo.DeviceAddress.ToString());
            joy.joyStick = joyst;
            if (joy.joyStick.StartConnect(0))
            {
                joy.State = deviceState.CONNECT;
                OnJoyStateChange?.Invoke(this.timeBoxJoyList);
                joy.joyStick.OnReceive = buffer => {
                    //string text = HexHelper.byteToHexStr(buffer, 18);
                    //this.ShowMsg(text);
                    this.OnJoyMessageReceive?.Invoke(buffer);
                };
                //joy.joyStick.SetJoyMap(new KeyBoardJoyMap());

                joy.SetJoyMap(this);
                joy.joyStick.startFeed();
            }
        }
        /// <summary>
        /// 循环检测断开的连接自动重连，以及检测手柄的连接状态
        /// </summary>
        private void ScanJoyConnection()
        {
            while (true)
            {
                BTDeviceInfo joy = timeBoxJoyList.Where(i => i.State == deviceState.CONNECT).FirstOrDefault();
                if (joy != null)
                {
                    if (!joy.joyStick.CheckConnect())
                    {
                        joy.State = deviceState.LOST;
                        OnJoyStateChange?.Invoke(this.timeBoxJoyList);
                    }
                }


                joy = timeBoxJoyList.Where(i => i.State == deviceState.CONNECTING).FirstOrDefault();
                if (joy != null)
                {
                    ConnectJoy(joy);
                }

                joy = timeBoxJoyList.Where(i => i.State == deviceState.LOST).FirstOrDefault();
                if (joy != null)
                {
                    ConnectJoy(joy);
                }

                Thread.Sleep(500);
            }
        }
        /// <summary>
        /// 连接一个手柄
        /// </summary>
        /// <param name="index"></param>
        public void ConnectJoy(int index)
        {
            var joy = GetDevice(index);
            if (joy != null)
            {
                if (joy.State == deviceState.DISCONNECT)
                {
                    joy.State = deviceState.CONNECTING;
                    OnJoyStateChange?.Invoke(this.timeBoxJoyList);
                }
            }
        }
        /// <summary>
        /// 断连一个手柄
        /// </summary>
        /// <param name="index"></param>
        public void DisconnectJoy(int index)
        {
            var joy = GetDevice(index);
            if (joy != null)
            {
                if (joy.State == deviceState.CONNECT || joy.State == deviceState.CONNECTING)
                {
                    joy.State = deviceState.DISCONNECT;
                    joy.joyStick.Disconnect();
                    OnJoyStateChange?.Invoke(this.timeBoxJoyList);
                }
            }
        }

        public BTDeviceInfo GetDevice(int index)
        {
            var joy = this.timeBoxJoyList.Skip(index).FirstOrDefault();
            return joy;
        }
        int _led = 0;
        public void ChangeLed(int index)
        {
            var joy = GetDevice(index);
            if (joy != null)
            {
                int led = _led++ > 4 ? _led = 1: _led;
                //int led = _led++;
                byte[] secret = new byte[]{
                    3,
                    85,
                    170,
                    1,
                    11,
                    1,
                    1,
                    0
                };
                secret[6] = (byte)led;
                joy.SendData(HexHelper.getSignStr(secret));
                WriteLog("切换LED灯");
            }
        }
        public void WriteLog(string value)
        {
            this.OnMessage?.Invoke(value);
        }
    }
}
