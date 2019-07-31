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
        JoyManager manager;
        List<string> Messages;
        private object locker = new object();
        private int SelectIndex = 0;
        public Form1()
        {
            InitializeComponent();
            //blueDeviceList = new List<BluetoothDeviceInfo>();
            try
            {
                Messages = new List<string>();
                manager = new JoyManager();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                System.Environment.Exit(0);
            }

        }
        private void Form1_Load(object sender, EventArgs e)
        {
            manager.OnJoyMessageReceive = buffer => {
                string text = HexHelper.byteToHexStr(buffer, buffer.Length);
                ShowMsg(text);
            };

            manager.OnMessage = msg => {
                ShowMsg(msg);
            };
            manager.OnJoyStateChange = joys => {
                this.Invoke(new Action(()=> {
                    this.listBox1.Items.Clear();
                    foreach(var joy in joys)
                    {
                        this.listBox1.Items.Add(joy);
                    }
                    if (SelectIndex >= 0 && SelectIndex < this.listBox1.Items.Count)
                    {
                        this.listBox1.SetSelected(SelectIndex, true);
                    }
                }));
            };
            manager.OnStartScanDevice = () =>{
                this.button2.Text = "扫描中......";
            };

            manager.OnEndScanDevice = () => {
                this.button2.Text = "重新扫描设备";
            };
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
            this.manager.ConnectJoy(this.SelectIndex);
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            this.manager.ScanBTDevice();
        }



        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            System.Environment.Exit(0);
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            this.manager.DisconnectJoy(this.SelectIndex);
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            var device = this.manager.GetDevice(this.SelectIndex);
            if (device != null)
            {
                if (device.State == deviceState.CONNECT)
                {
                    MapEdit mForm = new MapEdit(this.manager, this.manager.mapConfigs, device);
                    mForm.ShowDialog();
                }
                else
                {
                    MessageBox.Show("设备未连接！");
                }
            }
        }

        private void Button6_Click(object sender, EventArgs e)
        {
            manager.ChangeLed(this.SelectIndex);
        }

        private void ListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.SelectIndex = listBox1.SelectedIndex;
        }
    }
}
