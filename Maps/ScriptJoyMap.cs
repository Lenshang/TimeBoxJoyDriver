using Microsoft.ClearScript.V8;
using Nefarius.ViGEm.Client.Targets.Xbox360;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TimeBoxJoy.MapConfig;
using TimeBoxJoy.Utils;

namespace TimeBoxJoy.Maps
{
    enum TimeBoxScriptButton
    {
        A = 4,
        B = 22,
        X = 27,
        Y = 29,
        LB = 26,
        RB = 20,

        UP = 82,
        DOWN = 81,
        LEFT = 80,
        RIGHT = 79,
        LBACK = 24,
        RBACK = 23,
        HELP = 89,
        HOME = 74,
        BACK = 88,
        START = 44,
        LSBUTTON = 96,
        RSBUTTON = 97,

        LT=100,
        RT=101,
        LSUP=102,
        LSDOWN=103,
        LSLEFT=104,
        LSRIGHT=105,
        RSUP=106,
        RSDOWN=107,
        RSLEFT=108,
        RSRIGHT=109
    }
    public class ScriptJoyMap : IJoyMap
    {
        DefaultMapConfig KeyBoardConfig { get; set; }
        DefaultMapConfig XinputConfig { get; set; }
        IJoyMap KeyBoardJoyMap { get; set; }
        VitualXinputJoyMap XinputJoyMap { get; set; }
        V8ScriptEngine Engine { get; set; }
        Dictionary<int,string> Scripts { get; set; }

        byte[] HoldKeyCache = new byte[4];
        byte[] KeyCache = new byte[10];
        public byte TriggerDeadArea = 120;
        public byte RemoteDeadAreaOffset = 50;

        public ScriptJoyMap(DefaultMapConfig _config) : base(_config)
        {
            KeyBoardConfig = new KeyBoardConfig();
            XinputConfig = new XInputConfig();
            Scripts = new Dictionary<int, string>();
            config = _config;
        }

        public override void Dispose()
        {
            this.KeyBoardJoyMap.Dispose();
            this.XinputJoyMap.Dispose();
        }
        public override bool Initialize(Action<Exception> FailedCallback)
        {
            try
            {
                Engine = new V8ScriptEngine();

                FileHelper fh = new FileHelper();
                //创建脚本数据库 读取所有脚本
                if (!Directory.Exists("joyScripts"))
                {
                    Directory.CreateDirectory("joyScripts");
                }
                foreach (var file in Directory.GetFiles("joyScripts"))
                {
                    var name= System.IO.Path.GetFileNameWithoutExtension(file);
                    var nameHash = name.GetHashCode();
                    var scriptStr = fh.readFile(file);
                    if (!this.Scripts.ContainsKey(nameHash))
                    {
                        scriptStr = Regex.Replace(scriptStr, "var.*?onJoyKey.*?\\=", $"var onJoyKey_{nameHash.ToString()}=");
                        Engine.Execute(scriptStr);
                        this.Scripts.Add(nameHash, scriptStr);
                    }
                }
                
                this.KeyBoardJoyMap = new KeyBoardJoyMap(this.KeyBoardConfig);
                this.XinputJoyMap = new VitualXinputJoyMap(this.XinputConfig);
                this.KeyBoardJoyMap.Initialize(FailedCallback);
                this.XinputJoyMap.Initialize(FailedCallback);

                Engine.AddHostObject("keyBoard", this.KeyBoardJoyMap);
                Engine.AddHostObject("xinput", this.XinputJoyMap);
                Engine.AddHostType("Xbox360Report", typeof(Xbox360Report));
                Engine.AddHostType("Xbox360ReportExtensions", typeof(Xbox360ReportExtensions));
                Engine.AddHostType("Xbox360Axes", typeof(Xbox360Axes));
                Engine.AddHostType("Xbox360Buttons", typeof(Xbox360Buttons));
                ////TODO 读取脚本文件加载
                //Engine.Execute(this.Scripts["default".GetHashCode()]);

                return true;
            }
            catch(Exception e)
            {
                FailedCallback(e);
                return false;
            }
        }

        public override void OnKeyArray(byte[] buffer)
        {
            byte[] _holdKeyCache = new byte[4];
            for (int i = 0; i < buffer.Length; i++)
            {
                if (!HoldKeyCache.Contains(buffer[i]))
                {
                    ParseKey(buffer[i]);
                }
                _holdKeyCache[i] = buffer[i];
            }
            foreach (var _hk in HoldKeyCache)
            {
                if (!_holdKeyCache.Contains(_hk))
                {
                    ParseKey(_hk, 2);
                }
            }
            HoldKeyCache = _holdKeyCache;
        }

        private void ParseKey(byte bt, uint dwFlags = 0)
        {
            int target;
            if (this.config.Keymap.TryGetValue(bt, out target))
            {
                KeyEvent(bt,target, 0, dwFlags, 0);
            }
        }

        public void KeyEvent(int baseKey,int targetScript, byte bScan, uint dwFlags, uint dwExtraInfo)
        {
            if (targetScript < 0)
            {
                return;
            }
            Engine.Execute($"onJoyKey_{targetScript}({baseKey},{dwFlags});");
            //keybd_event((Keys)key, bScan, dwFlags, dwExtraInfo);
        }
        public override void OnLTrigger(byte[] value)
        {
            if (value[0] > TriggerDeadArea)
            {
                if (KeyCache[0] == 0)
                {
                    KeyEvent((int)TimeBoxScriptButton.LT,this.config.LTrigger, 0, 0, 0);
                    KeyCache[0] = 1;
                }
            }
            else
            {
                KeyEvent((int)TimeBoxScriptButton.LT,this.config.LTrigger, 0, 2, 0);
                KeyCache[0] = 0;
            }
        }

        public override void OnRTrigger(byte[] value)
        {
            if (value[0] > TriggerDeadArea)
            {
                if (KeyCache[1] == 0)
                {
                    KeyEvent((int)TimeBoxScriptButton.RT,this.config.RTrigger, 0, 0, 0);
                    KeyCache[1] = 1;
                }
            }
            else
            {
                KeyEvent((int)TimeBoxScriptButton.RT,this.config.RTrigger, 0, 2, 0);
                KeyCache[1] = 0;
            }
        }

        public override void OnLeftRemote(byte[] x, byte[] y)
        {

            if (x[0] > 128 && (x[0] - RemoteDeadAreaOffset) > 128)
            {
                KeyCache[3] = 0;
                KeyEvent((int)TimeBoxScriptButton.LSLEFT,this.config.LeftRemoteLeft, 0, 2, 0);
                if (KeyCache[2] == 0)
                {
                    KeyEvent((int)TimeBoxScriptButton.LSRIGHT, this.config.LeftRemoteRight, 0, 0, 0);
                    KeyCache[2] = 1;
                }
            }
            else if (x[0] < 128 && (x[0] + RemoteDeadAreaOffset) < 128)
            {
                KeyCache[2] = 0;
                KeyEvent((int)TimeBoxScriptButton.LSRIGHT, this.config.LeftRemoteRight, 0, 2, 0);
                if (KeyCache[3] == 0)
                {
                    KeyEvent((int)TimeBoxScriptButton.LSLEFT, this.config.LeftRemoteLeft, 0, 0, 0);
                    KeyCache[3] = 1;
                }
            }
            else
            {
                KeyEvent((int)TimeBoxScriptButton.LSLEFT, this.config.LeftRemoteLeft, 0, 2, 0);
                KeyEvent((int)TimeBoxScriptButton.LSRIGHT, this.config.LeftRemoteRight, 0, 2, 0);
                KeyCache[2] = 0;
                KeyCache[3] = 0;
            }



            if (y[0] > 128 && (y[0] - RemoteDeadAreaOffset) > 128)
            {
                KeyCache[5] = 0;
                KeyEvent((int)TimeBoxScriptButton.LSUP, this.config.LeftRemoteUp, 0, 2, 0);
                if (KeyCache[4] == 0)
                {
                    KeyEvent((int)TimeBoxScriptButton.LSDOWN, this.config.LeftRemoteDown, 0, 0, 0);
                    KeyCache[4] = 1;
                }
            }
            else if (y[0] < 128 && (y[0] + RemoteDeadAreaOffset) < 128)
            {
                KeyCache[4] = 0;
                KeyEvent((int)TimeBoxScriptButton.LSDOWN,this.config.LeftRemoteDown, 0, 2, 0);
                if (KeyCache[5] == 0)
                {
                    KeyEvent((int)TimeBoxScriptButton.LSUP,this.config.LeftRemoteUp, 0, 0, 0);
                    KeyCache[5] = 1;
                }
            }
            else
            {
                KeyEvent((int)TimeBoxScriptButton.LSUP, this.config.LeftRemoteUp, 0, 2, 0);
                KeyEvent((int)TimeBoxScriptButton.LSDOWN,this.config.LeftRemoteDown, 0, 2, 0);
                KeyCache[4] = 0;
                KeyCache[5] = 0;
            }
        }

        public override void OnRightRemote(byte[] x, byte[] y)
        {
            if (x[0] > 128 && (x[0] - RemoteDeadAreaOffset) > 128)
            {
                KeyCache[7] = 0;
                KeyEvent((int)TimeBoxScriptButton.RSLEFT, this.config.RightRemoteLeft, 0, 2, 0);
                if (KeyCache[6] == 0)
                {
                    KeyEvent((int)TimeBoxScriptButton.RSRIGHT, this.config.RightRemoteRight, 0, 0, 0);
                    KeyCache[6] = 1;
                }
            }
            else if (x[0] < 128 && (x[0] + RemoteDeadAreaOffset) < 128)
            {
                KeyCache[6] = 0;
                KeyEvent((int)TimeBoxScriptButton.RSRIGHT, this.config.RightRemoteRight, 0, 2, 0);
                if (KeyCache[7] == 0)
                {
                    KeyEvent((int)TimeBoxScriptButton.RSLEFT, this.config.RightRemoteLeft, 0, 0, 0);
                    KeyCache[7] = 1;
                }
            }
            else
            {
                KeyEvent((int)TimeBoxScriptButton.RSLEFT, this.config.RightRemoteLeft, 0, 2, 0);
                KeyEvent((int)TimeBoxScriptButton.RSRIGHT,this.config.RightRemoteRight, 0, 2, 0);
                KeyCache[6] = 0;
                KeyCache[7] = 0;
            }



            if (y[0] > 128 && (y[0] - RemoteDeadAreaOffset) > 128)
            {
                KeyCache[9] = 0;
                KeyEvent((int)TimeBoxScriptButton.RSUP, this.config.RightRemoteUp, 0, 2, 0);
                if (KeyCache[8] == 0)
                {
                    KeyEvent((int)TimeBoxScriptButton.RSDOWN, this.config.RightRemoteDown, 0, 0, 0);
                    KeyCache[8] = 1;
                }
            }
            else if (y[0] < 128 && (y[0] + RemoteDeadAreaOffset) < 128)
            {
                KeyCache[8] = 0;
                KeyEvent((int)TimeBoxScriptButton.RSDOWN, this.config.RightRemoteDown, 0, 2, 0);
                if (KeyCache[9] == 0)
                {
                    KeyEvent((int)TimeBoxScriptButton.RSUP, this.config.RightRemoteUp, 0, 0, 0);
                    KeyCache[9] = 1;
                }
            }
            else
            {
                KeyEvent((int)TimeBoxScriptButton.RSUP,this.config.RightRemoteUp, 0, 2, 0);
                KeyEvent((int)TimeBoxScriptButton.RSDOWN,this.config.RightRemoteDown, 0, 2, 0);
                KeyCache[8] = 0;
                KeyCache[9] = 0;
            }
        }
    }
}
