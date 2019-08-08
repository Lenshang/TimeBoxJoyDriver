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
using TimeBoxJoy.Utils;

namespace TimeBoxJoy
{
    public partial class NewForm : Form
    {
        JoyManager manager;
        bool AutoHide = false;
        bool HideBalloon = false;
        object locker = new object();
        public NewForm(string[] args)
        {
            InitializeComponent();
            if (args.Contains("-hide"))
            {
                this.AutoHide = true;
                this.ShowInTaskbar = false;
                this.Opacity = 0;
            }
            if (args.Contains("-hideballoon"))
            {
                this.HideBalloon = true;
            }
            mainWebBrowser.ObjectForScripting = this;
            mainWebBrowser.ScriptErrorsSuppressed = false; //禁用错误脚本提示
            mainWebBrowser.IsWebBrowserContextMenuEnabled = false; // 禁用右键菜单
            mainWebBrowser.WebBrowserShortcutsEnabled = false; //禁用快捷键
            mainWebBrowser.AllowWebBrowserDrop = false; // 禁止文件拖动
            mainWebBrowser.Navigate(Application.StartupPath + @"\Web\Main.html");
        }

        private void NewForm_Load(object sender, EventArgs e)
        {

        }
        private void MainWebBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            try
            {
                manager = new JoyManager(
                    OnJoyMessageReceive: buffer => {
                        this.Invoke(new Action(()=> {
                            string text = HexHelper.byteToHexStr(buffer, buffer.Length);
                            mainWebBrowser.Document.InvokeScript("addCommit", new object[] { text });
                        }));
                    },
                    OnMessage: msg => {
                        this.Invoke(new Action(() => {
                            mainWebBrowser.Document.InvokeScript("addCommit", new object[] { msg });
                        }));
                    },
                    OnJoyStateChange: joys => {
                        this.Invoke(new Action(() => {
                            var lists = joys.Select(i => i.ToString());
                            var str = string.Join(";", lists);
                            mainWebBrowser.Document.InvokeScript("onlistChange", new object[] { str });
                        }));
                    },
                    OnStartScanDevice: () => {
                        this.Invoke(new Action(() => {
                            mainWebBrowser.Document.InvokeScript("changeScanState", new object[] { "扫描中....." });
                        }));
                    },
                    OnEndScanDevice: () => {
                        this.Invoke(new Action(() => {
                            mainWebBrowser.Document.InvokeScript("changeScanState", new object[] { "重新扫描" });
                        }));
                    }
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                //ShowMsg(ex.Message);
                System.Environment.Exit(0);
            }
        }
        public void StartScan()
        {
            this.manager.ScanBTDevice();
        }

        public void ConnectJoy(int index)
        {
            this.manager.ConnectJoy(index);
        }

        public void DisconnectJoy(int index)
        {
            this.manager.DisconnectJoy(index);
        }

        public void ShowEdit(int index)
        {
            var device = this.manager.GetDevice(index);
            if (device != null)
            {
                if (device.State == deviceState.CONNECT)
                {
                    MapEdit mForm = new MapEdit(this.manager, this.manager.mapConfigs, device);
                    mForm.ShowDialog();
                    mForm.Dispose();
                }
                else
                {
                    ShowMsg("设备未连接！");
                }
            }
        }
        public void ChangeLed(int index)
        {
            var device = this.manager.GetDevice(index);
            if (device != null)
            {
                if (device.State == deviceState.CONNECT)
                {
                    manager.ChangeLed(index);
                }
                else
                {
                    ShowMsg("设备未连接！");
                }
            }
        }
        public void ShowMsg(string msg)
        {
            this.Invoke(new Action(()=> {
                mainWebBrowser.Document.InvokeScript("showMsg", new object[] { msg });
            }));
        }

        public void CloseApp()
        {
            notifyIcon1.Visible = false;
            System.Environment.Exit(0);
        }
        private void NewForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
            this.Visible = false;
            if (!HideBalloon)
            {
                this.notifyIcon1.BalloonTipText = "程序依然在运行哟~";
                this.notifyIcon1.ShowBalloonTip(5);
            }
        }

        private void 关于作者ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Show();
            this.Visible = true;
            mainWebBrowser.Document.InvokeScript("showThanks");
            //MessageBox.Show("一个神秘的人");
        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            notifyIcon1.Visible = false;
            System.Environment.Exit(0);
        }

        private void NotifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            this.Visible = true;
        }

        private void 显示ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Show();
            this.Visible = true;
        }
    }
}
