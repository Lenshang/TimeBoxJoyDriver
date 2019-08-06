using InTheHand.Net;
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
        public TimeSpan ScanBlueTimeOut = TimeSpan.FromSeconds(30);
        public int State = 0;//0=STOP,1=SCANNING

        public List<string> rememberMac { get; set; }
        public List<DefaultMapConfig> mapConfigs { get; set; }
        public Action<List<BTDeviceInfo>> OnJoyStateChange { get; set; } = null;
        public Action<Byte[]> OnJoyMessageReceive { get; set; } = null;
        public Action<string> OnMessage { get; set; } = null;
        public Action OnStartScanDevice { get; set; } = null;
        public Action OnEndScanDevice { get; set; } = null;
        public JoyManager(
                    Action<List<BTDeviceInfo>> OnJoyStateChange=null, 
                    Action<Byte[]> OnJoyMessageReceive=null, 
                    Action<string> OnMessage=null,
                    Action OnStartScanDevice=null,
                    Action OnEndScanDevice=null)
        {
            timeBoxJoyList = new List<BTDeviceInfo>();
            this.OnJoyStateChange = OnJoyStateChange;
            this.OnJoyMessageReceive = OnJoyMessageReceive;
            this.OnMessage = OnMessage;
            this.OnStartScanDevice = OnStartScanDevice;
            this.OnEndScanDevice = OnEndScanDevice;

            //读取所有配置文件
            mapConfigs = new List<DefaultMapConfig>();
            if (!Directory.Exists("maps"))
            {
                Directory.CreateDirectory("maps");
            }
            mapConfigs.Add(new KeyBoardConfig());
            mapConfigs.Add(new XInputConfig());
            mapConfigs.Add(new MixModeConfig());
            mapConfigs.Add(new ScriptModeConfig());
            foreach (var file in Directory.GetFiles("maps"))
            {
                var _config = DefaultMapConfig.GetConfig(file);
                //FileInfo finfo = new FileInfo(file);
                _config.Name = System.IO.Path.GetFileNameWithoutExtension(file);
                if (!mapConfigs.Any(p => p.Name == _config.Name))
                {
                    mapConfigs.Add(_config);
                }
            }

            //读取已记忆的MAC设备
            rememberMac = new List<string>();
            if (File.Exists("remember.txt"))
            {
                FileHelper fh = new FileHelper();
                foreach(var addr in fh.readFileLine("remember.txt"))
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(addr))
                        {
                            continue;
                        }
                        if (!addr.Contains(":"))
                        {
                            continue;
                        }
                        string macAddr = addr.Split(':')[0];
                        string configName = addr.Split(':')[1];
                        if (rememberMac.Contains(macAddr))
                        {
                            continue;
                        }
                        rememberMac.Add(macAddr);
                        BluetoothAddress address = BluetoothAddress.Parse(macAddr);
                        BluetoothDeviceInfo info = new BluetoothDeviceInfo(address);
                        var btDevice = new BTDeviceInfo(info);
                        btDevice.State = deviceState.LOST;
                        btDevice.mapConfig = mapConfigs.Where(i => i.Name == configName).FirstOrDefault();
                        this.timeBoxJoyList.Add(btDevice);
                    }
                    catch
                    {

                    }
                }
                this.OnJoyStateChange?.Invoke(this.timeBoxJoyList);
            }

            //初始化蓝牙设备
            blueClient = new BluetoothClient();
            blueComponent = new BluetoothComponent(blueClient);
            string name = "timebox";
            blueComponent.DiscoverDevicesProgress += (sender, e) => {
                foreach (var device in e.Devices)
                {
                    if (device.DeviceName == name && !this.timeBoxJoyList.Any(p => p.bluetoothDeviceInfo.DeviceAddress.ToString() == device.DeviceAddress.ToString()))
                    {
                        this.timeBoxJoyList.Add(new BTDeviceInfo(device));
                    }
                }
                this.OnJoyStateChange?.Invoke(this.timeBoxJoyList);
            };
            blueComponent.DiscoverDevicesComplete += (sender, e) => {
                if ((DateTime.Now - startScan) < ScanBlueTimeOut)
                {
                    blueComponent.DiscoverDevicesAsync(30, true, true, true, true, null);
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
            string addr = joy.bluetoothDeviceInfo.DeviceAddress.ToString();
            JoyStick joyst = new JoyStick(addr);
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

                joy.SetJoyMap(this,joy.mapConfig);
                joy.joyStick.startFeed();

                this.rememberMac.Remove(addr);
                this.rememberMac.Add(addr+":"+ joy.mapConfig.Name);
                FileHelper fh = new FileHelper();
                fh.SaveFile("remember.txt", string.Join("\r\n", this.rememberMac));
            }
        }
        /// <summary>
        /// 循环检测断开的连接自动重连，以及检测手柄的连接状态
        /// </summary>
        private void ScanJoyConnection()
        {
            while (true)
            {
                //BTDeviceInfo joy = timeBoxJoyList.Where(i => i.State == deviceState.CONNECT).FirstOrDefault();
                //foreach(var joy in timeBoxJoyList.Where(i => i.State == deviceState.CONNECT))
                //{
                //    if (!joy.joyStick.CheckConnect())
                //    {
                //        joy.State = deviceState.LOST;
                //        joy.joyMap.Dispose();
                //        OnJoyStateChange?.Invoke(this.timeBoxJoyList);
                //    }
                //}

                //foreach(var joy in timeBoxJoyList.Where(i => i.State == deviceState.CONNECTING))
                //{
                //    ConnectJoy(joy);
                //}

                //foreach(var joy in timeBoxJoyList.Where(i => i.State == deviceState.LOST))
                //{
                //    ConnectJoy(joy);
                //}

                foreach(var joy in timeBoxJoyList)
                {
                    switch (joy.State)
                    {
                        case deviceState.CONNECT:
                            if (!joy.joyStick.CheckConnect())
                            {
                                joy.State = deviceState.LOST;
                                joy.joyMap.Dispose();
                                OnJoyStateChange?.Invoke(this.timeBoxJoyList);
                            }
                            break;
                        case deviceState.CONNECTING:
                            ConnectJoy(joy);
                            break;
                        case deviceState.LOST:
                            ConnectJoy(joy);
                            break;
                    }
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
                //if (joy.State == deviceState.CONNECT || joy.State == deviceState.CONNECTING)
                //{
                //    joy.State = deviceState.DISCONNECT;
                //    joy.joyStick.Disconnect();
                //    OnJoyStateChange?.Invoke(this.timeBoxJoyList);
                //}
                joy.State = deviceState.DISCONNECT;
                joy.joyStick.Disconnect();
                OnJoyStateChange?.Invoke(this.timeBoxJoyList);

                var addr = joy.bluetoothDeviceInfo.DeviceAddress.ToString();
                this.rememberMac.Remove(addr);
                FileHelper fh = new FileHelper();
                fh.SaveFile("remember.txt", string.Join("\r\n", this.rememberMac));
            }
        }

        public BTDeviceInfo GetDevice(int index)
        {
            var joy = this.timeBoxJoyList.Skip(index).FirstOrDefault();
            return joy;
        }
        int _led = 1;
        public void ChangeLed(int index)
        {
            var joy = GetDevice(index);
            if (joy != null)
            {
                int led = _led++ > 3 ? _led = 1: _led;
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
