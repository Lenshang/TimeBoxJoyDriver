using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;
using Newtonsoft.Json;
using TimeBoxJoy.Utils;

namespace TimeBoxJoy.Maps
{
    public class XInputConfig
    {
        public Dictionary<byte, Xbox360Buttons> Keymap { get; set; } = new Dictionary<byte, Xbox360Buttons>();

        public Xbox360Axes LTrigger { get; set; } = Xbox360Axes.LeftTrigger;
        public Xbox360Axes RTrigger { get; set; } = Xbox360Axes.RightTrigger;
        public Xbox360Axes LeftRemoteX { get; set; } = Xbox360Axes.LeftThumbX;
        public Xbox360Axes LeftRemoteY { get; set; } = Xbox360Axes.LeftThumbY;
        public Xbox360Axes RightRemoteX { get; set; } = Xbox360Axes.RightThumbX;
        public Xbox360Axes RightRemoteY { get; set; } = Xbox360Axes.RightThumbY;

        public XInputConfig()
        {
            Keymap.Add(4, Xbox360Buttons.A);//A
            Keymap.Add(22, Xbox360Buttons.B);//B
            Keymap.Add(27, Xbox360Buttons.X);//x
            Keymap.Add(29, Xbox360Buttons.Y);//y
            Keymap.Add(26, Xbox360Buttons.LeftThumb);//L1
            Keymap.Add(20, Xbox360Buttons.RightThumb);//R1

            Keymap.Add(82, Xbox360Buttons.Up);//up
            Keymap.Add(81, Xbox360Buttons.Down);//down
            Keymap.Add(80, Xbox360Buttons.Left);//left
            Keymap.Add(79, Xbox360Buttons.Right);//right
            Keymap.Add(24, Xbox360Buttons.LeftShoulder);//back L
            Keymap.Add(23, Xbox360Buttons.RightShoulder);//back R
            //Keymap.Add(89, Keys.T);//Help
            Keymap.Add(74, Xbox360Buttons.Guide);//Home

            Keymap.Add(88, Xbox360Buttons.Back);//back
            Keymap.Add(44, Xbox360Buttons.Start);//start
            Keymap.Add(96, Xbox360Buttons.LeftShoulder);//LC
            Keymap.Add(97, Xbox360Buttons.RightShoulder);//RC
        }
    }
    public class VitualXinputJoyMap : IJoyMap
    {
        ViGEmClient myViGEmClient;
        Xbox360Controller myController;

        XInputConfig config;
        Xbox360Buttons[] HoldKeyCache = new Xbox360Buttons[4];
        byte[] KeyCache = new byte[10];
        public VitualXinputJoyMap()
        {
            FileHelper fh = new FileHelper();
            if (File.Exists("xinput.config"))
            {
                var str = fh.readFile("xinput.config");
                this.config = JsonConvert.DeserializeObject<XInputConfig>(str);
            }
            else
            {
                this.config = new XInputConfig();
                var str = JsonConvert.SerializeObject(this.config);
                fh.SaveFile("xinput.config", str);
            }

            this.Name = "Defalt XInput Map";
        }
        public override bool Initialize(Action<Exception> FailedCallback)
        {
            try
            {
                myViGEmClient = new ViGEmClient();
                myController = new Xbox360Controller(myViGEmClient);
                myController.Connect();
                myController.FeedbackReceived += MyController_FeedbackReceived;
                return true;
            }
            catch(Exception ex)
            {
                if(ex is Nefarius.ViGEm.Client.Exceptions.VigemBusNotFoundException)
                {
                    FailedCallback.Invoke(new Exception("没有找到ViGEm虚拟手柄驱动！"));
                }
                else
                {
                    FailedCallback.Invoke(ex);
                }
                return false;
            }
        }
        private void MyController_FeedbackReceived(object sender, Xbox360FeedbackReceivedEventArgs e)
        {
            
        }

        public override void OnBuffer(byte[] keyBuffer, byte[] lTrigger, byte[] rTrigger, byte[] lRemoteX, byte[] lRemoteY, byte[] rRemoteX, byte[] rRemoteY)
        {
            Xbox360Report controllerReport = new Xbox360Report();
            OnKeyArray(keyBuffer, controllerReport);
            OnLeftRemote(lRemoteX, lRemoteY, controllerReport);
            OnRightRemote(rRemoteX, rRemoteY, controllerReport);
            OnLTrigger(lTrigger, controllerReport);
            OnRTrigger(rTrigger, controllerReport);
            myController.SendReport(controllerReport);
        }
        public void OnKeyArray(byte[] buffer, Xbox360Report controllerReport)
        {
            Xbox360Buttons[] _holdKeyCache = new Xbox360Buttons[4];
            for (int i = 0; i < buffer.Length; i++)
            {
                Xbox360Buttons target;
                if (this.config.Keymap.TryGetValue(buffer[i], out target))
                {
                    if (!HoldKeyCache.Contains(target))
                    {
                        ParseKey(target, controllerReport);
                    }
                    _holdKeyCache[i] = target;
                }

            }
            foreach (var _hk in HoldKeyCache)
            {
                if (!_holdKeyCache.Contains(_hk))
                {
                    ParseKey(_hk, controllerReport, 2);
                }
            }
            controllerReport.SetButtons(_holdKeyCache);
            HoldKeyCache = _holdKeyCache;
        }
        private void ParseKey(Xbox360Buttons target, Xbox360Report controllerReport, uint dwFlags = 0)
        {
            if (dwFlags == 0)
            {
                controllerReport.SetButtonState(target, true);
            }
            else
            {
                controllerReport.SetButtonState(target, false);
            }
        }
        public void OnLeftRemote(byte[] x, byte[] y, Xbox360Report controllerReport)
        {
            controllerReport.SetAxis(this.config.LeftRemoteX, getShort(x[0]));
            controllerReport.SetAxis(this.config.LeftRemoteY, getShort(y[0],true));
        }

        public void OnLTrigger(byte[] value, Xbox360Report controllerReport)
        {
            controllerReport.SetAxis(this.config.LTrigger, value[0]);
        }

        public void OnRightRemote(byte[] x, byte[] y, Xbox360Report controllerReport)
        {
            controllerReport.SetAxis(this.config.RightRemoteX, getShort(x[0]));
            controllerReport.SetAxis(this.config.RightRemoteY, getShort(y[0],true));
        }

        public void OnRTrigger(byte[] value, Xbox360Report controllerReport)
        {
            controllerReport.SetAxis(this.config.RTrigger, value[0]);
        }

        private short getShort(byte bt,bool convert=false)
        {
            if (convert)
            {
                if (bt < 0x80)
                {
                    bt = unchecked((byte)(bt - (bt - 0x80) * 2-1));
                }
                else if (bt > 0x80)
                {
                    bt = unchecked((byte)(bt - (bt - 0x80) * 2 + 1));
                }
                //if (bt < 0x80)
                //{
                //    bt = unchecked((byte)(bt + (-bt*2)));
                //}
                //else if (bt > 0x80)
                //{
                //    bt = unchecked((byte)(bt - (-bt * 2)));
                //}
            }

            var s = (short)((bt - 0x80) << 8);

            return s;
        }
    }
}
