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
using TimeBoxJoy.MapConfig;
using TimeBoxJoy.Utils;

namespace TimeBoxJoy.Maps
{
    public class VitualXinputJoyMap : IJoyMap
    {
        ViGEmClient myViGEmClient;
        Xbox360Controller myController;

        Xbox360Buttons[] HoldKeyCache = new Xbox360Buttons[4];
        byte[] KeyCache = new byte[10];
        public VitualXinputJoyMap(MapConfig.DefaultMapConfig config=null):base(config)
        {
            if (config != null)
            {
                this.config = config;
            }
            else
            {
                this.config = new XInputConfig();
            }
            //FileHelper fh = new FileHelper();
            //if (File.Exists("xinput.config"))
            //{
            //    var str = fh.readFile("xinput.config");
            //    this.config = JsonConvert.DeserializeObject<XInputConfig>(str);
            //}
            //else
            //{
            //    this.config = new XInputConfig();
            //    var str = JsonConvert.SerializeObject(this.config);
            //    fh.SaveFile("xinput.config", str);
            //}

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
                int target;
                if (this.config.Keymap.TryGetValue(buffer[i], out target))
                {
                    if (!HoldKeyCache.Contains((Xbox360Buttons)target))
                    {
                        ParseKey((Xbox360Buttons)target, controllerReport);
                    }
                    _holdKeyCache[i] = (Xbox360Buttons)target;
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
            if ((int)target < 0) return;
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
            if (this.config.LeftRemoteX >= 0)
                controllerReport.SetAxis((Xbox360Axes)this.config.LeftRemoteX, getShort(x[0]));
            if (this.config.LeftRemoteY >= 0)
                controllerReport.SetAxis((Xbox360Axes)this.config.LeftRemoteY, getShort(y[0],true));
        }

        public void OnLTrigger(byte[] value, Xbox360Report controllerReport)
        {
            if(this.config.LTrigger>=0)
                controllerReport.SetAxis((Xbox360Axes)this.config.LTrigger, value[0]);
        }

        public void OnRightRemote(byte[] x, byte[] y, Xbox360Report controllerReport)
        {
            if(this.config.RightRemoteX>=0)
                controllerReport.SetAxis((Xbox360Axes)this.config.RightRemoteX, getShort(x[0]));
            if(this.config.RightRemoteY>=0)
                controllerReport.SetAxis((Xbox360Axes)this.config.RightRemoteY, getShort(y[0],true));
        }

        public void OnRTrigger(byte[] value, Xbox360Report controllerReport)
        {
            if (this.config.RTrigger >= 0)
                controllerReport.SetAxis((Xbox360Axes)this.config.RTrigger, value[0]);
        }

        private short getShort(byte bt,bool convert=false)
        {
            if (convert)
            {
                if (bt < 0x80)
                {
                    bt = unchecked((byte)(bt - (bt - 0x80) * 2 - 1));
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

        public override void Dispose()
        {
            myController.Disconnect();
            myController.Dispose();
            myViGEmClient.Dispose();
        }
    }
}
