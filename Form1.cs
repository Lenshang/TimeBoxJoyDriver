using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TimeBoxJoy.Maps;
using TimeBoxJoy.Utils;

namespace TimeBoxJoy
{
    public partial class Form1 : Form
    {
        //private List<BluetoothDeviceInfo> blueDeviceList;
        private List<BTDeviceInfo> timeBoxJoyList;
        private BluetoothComponent blueComponent;
        private BluetoothClient blueClient;
        private DateTime startScan=DateTime.Now;
        List<string> Messages = new List<string>();
        private object locker = new object();
        public Form1()
        {
            InitializeComponent();
            //blueDeviceList = new List<BluetoothDeviceInfo>();
            try
            {
                timeBoxJoyList = new List<BTDeviceInfo>();
                blueClient = new BluetoothClient();
                blueComponent = new BluetoothComponent(blueClient);
                blueComponent.DiscoverDevicesProgress += (sender, e) => {
                    //foreach (var device in e.Devices)
                    //{
                    //    if (!this.blueDeviceList.Any(p => p.DeviceAddress.ToString() == device.DeviceAddress.ToString()))
                    //    {
                    //        this.blueDeviceList.Add(device);
                    //    }
                    //}
                    //blueDeviceList.AddRange(e.Devices);
                    foreach (var device in e.Devices)
                    {
                        if (device.DeviceName == "timebox" && !this.timeBoxJoyList.Any(p => p.bluetoothDeviceInfo.DeviceAddress.ToString() == device.DeviceAddress.ToString()))
                        {
                            this.timeBoxJoyList.Add(new BTDeviceInfo(device));
                        }
                    }
                    UpdateListbox();
                };
                blueComponent.DiscoverDevicesComplete += (sender, e) => {
                    if ((DateTime.Now - startScan) < TimeSpan.FromSeconds(20))
                    {
                        this.Invoke(new Action(() => {
                            if (button2.Text.Length > 6)
                            {
                                button2.Text = "扫描中";
                            }
                            button2.Text += ".";
                        }));
                        blueComponent.DiscoverDevicesAsync(255, true, true, false, true, null);
                    }
                    else
                    {
                        this.Invoke(new Action(() => {
                            button2.Text = "重新扫描";
                        }));
                    }
                };
                ScanBTDevice();
                //启动自动扫描手柄状态线程
                Thread tr = new Thread(new ThreadStart(() => {
                    ScanJoyConnection();
                }));
                tr.Start();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                System.Environment.Exit(0);
            }

        }
        private void ScanBTDevice()
        {
            button2.Text = "扫描中";
            startScan = DateTime.Now;
            blueComponent.DiscoverDevicesAsync(30, true, true, true, true, null);
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            blueComponent.DiscoverDevicesAsync(255, true, true, false, true, null);
        }
        private void UpdateListbox()
        {
            this.Invoke(new Action(() => {
                this.listBox1.Items.Clear();
                foreach (var item in timeBoxJoyList)
                {
                    this.listBox1.Items.Add(item);
                }
            }));
        }
        private void ConnectJoy(BTDeviceInfo joy)
        {
            JoyStick joyst = new JoyStick(joy.bluetoothDeviceInfo.DeviceAddress.ToString());
            joy.joyStick = joyst;
            if (joy.joyStick.StartConnect(1))
            {
                joy.State = deviceState.CONNECT;
                UpdateListbox();
                joy.joyStick.OnReceive = buffer => {
                    string text = HexHelper.byteToHexStr(buffer, 18);
                    this.ShowMsg(text);
                };
                //joy.joyStick.SetJoyMap(new KeyBoardJoyMap());
                IJoyMap map = new VitualXinputJoyMap();
                if (!map.Initialize(ex=> {
                    MessageBox.Show(ex.Message+"\r\n已切换回键盘映射模式");
                }))
                {
                    map = new KeyBoardJoyMap();
                }
                joy.joyStick.SetJoyMap(map);
                joy.joyStick.startFeed();
            }
        }
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
                        this.UpdateListbox();
                    }
                }


                joy = timeBoxJoyList.Where(i => i.State==deviceState.CONNECTING).FirstOrDefault();
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
        private void ShowMsg(string msg)
        {
            this.Invoke(new Action(() => {
                if (Messages.Count > 10)
                {
                    Messages = new List<string>();
                }
                Messages.Add(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss>") + msg);
                this.textBox1.Lines = Messages.ToArray();
            }));

        }
        private void Button1_Click(object sender, EventArgs e)
        {
            var joy = (BTDeviceInfo)listBox1.SelectedItem;
            if (joy != null)
            {
                if (joy.State == deviceState.DISCONNECT)
                {
                    joy.State = deviceState.CONNECTING;
                    UpdateListbox();
                }
            }
            //ConnectJoy();
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            this.timeBoxJoyList.RemoveAll(i => i.State == deviceState.DISCONNECT);
            ScanBTDevice();
        }



        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            System.Environment.Exit(0);
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            var joy = (BTDeviceInfo)listBox1.SelectedItem;
            if (joy != null)
            {
                if (joy.State == deviceState.CONNECT||joy.State==deviceState.CONNECTING)
                {
                    joy.State = deviceState.DISCONNECT;
                    joy.joyStick.Disconnect();
                    UpdateListbox();
                }
            }
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            var joy = (BTDeviceInfo)listBox1.SelectedItem;
            if (joy != null)
            {
                IJoyMap map = new VitualXinputJoyMap();
                if (!map.Initialize(ex => {
                    MessageBox.Show(ex.Message + "\r\n已切换回键盘映射模式");
                }))
                {
                    map = new KeyBoardJoyMap();
                }
                joy.joyStick.SetJoyMap(map);
            }
        }

        private void Button5_Click(object sender, EventArgs e)
        {
            var joy = (BTDeviceInfo)listBox1.SelectedItem;
            if (joy != null)
            {
                IJoyMap map = new KeyBoardJoyMap();
                joy.joyStick.SetJoyMap(map);
            }
        }
    }
}
