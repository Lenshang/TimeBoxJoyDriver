using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TimeBoxJoy.Utils;

namespace TimeBoxJoy.Maps
{
    public class KeyBoardConfig
    {
        public Dictionary<byte, Keys> Keymap { get; set; } = new Dictionary<byte, Keys>();

        public Keys LTrigger { get; set; } = Keys.J;
        public Keys RTrigger { get; set; } = Keys.L;
        public Keys LeftRemoteUp { get; set; } = Keys.W;
        public Keys LeftRemoteDown { get; set; } = Keys.S;
        public Keys LeftRemoteLeft { get; set; } = Keys.A;
        public Keys LeftRemoteRight { get; set; } = Keys.D;
        public Keys RightRemoteUp { get; set; } = Keys.Up;
        public Keys RightRemoteDown { get; set; } = Keys.Down;
        public Keys RightRemoteLeft { get; set; } = Keys.Left;
        public Keys RightRemoteRight { get; set; } = Keys.Right;

        public KeyBoardConfig()
        {
            Keymap.Add(4, Keys.U);//A
            Keymap.Add(22, Keys.I);//B
            Keymap.Add(27, Keys.O);//x
            Keymap.Add(29, Keys.P);//y
            Keymap.Add(26, Keys.Q);//L1
            Keymap.Add(20, Keys.E);//R1

            Keymap.Add(82, Keys.W);//up
            Keymap.Add(81, Keys.S);//down
            Keymap.Add(80, Keys.A);//left
            Keymap.Add(79, Keys.D);//right
            Keymap.Add(24, Keys.Z);//back L
            Keymap.Add(23, Keys.C);//back R
            Keymap.Add(89, Keys.T);//Help
            Keymap.Add(74, Keys.Y);//Home

            Keymap.Add(88, Keys.G);//back
            Keymap.Add(44, Keys.H);//start
            Keymap.Add(96, Keys.V);//LC
            Keymap.Add(97, Keys.B);//RC
        }
    }
    public class KeyBoardJoyMap : IJoyMap
    {
        KeyBoardConfig config;
        public KeyBoardJoyMap()
        {
            FileHelper fh = new FileHelper();
            if (File.Exists("kbmap.config"))
            {
                var str=fh.readFile("kbmap.config");
                this.config = JsonConvert.DeserializeObject<KeyBoardConfig>(str);
            }
            else
            {
                this.config = new KeyBoardConfig();
                var str = JsonConvert.SerializeObject(this.config);
                fh.SaveFile("kbmap.config", str);
            }
        }
        byte[] HoldKeyCache = new byte[4];
        byte[] KeyCache = new byte[10];
        public byte TriggerDeadArea = 120;
        public byte RemoteDeadAreaOffset = 50;
        public override void OnKeyArray(byte[] buffer)
        {
            byte[] _holdKeyCache = new byte[4];
            for(int i = 0; i < buffer.Length; i++)
            {
                if (!HoldKeyCache.Contains(buffer[i]))
                {
                    ParseKey(buffer[i]);
                }
                _holdKeyCache[i] = buffer[i];
            }
            foreach(var _hk in HoldKeyCache)
            {
                if (!_holdKeyCache.Contains(_hk))
                {
                    ParseKey(_hk, 2);
                }
            }
            HoldKeyCache = _holdKeyCache;
        }

        private void ParseKey(byte bt,uint dwFlags = 0)
        {
            Keys target;
            if(this.config.Keymap.TryGetValue(bt,out target))
            {
                keybd_event(target, 0, dwFlags, 0);
            }
        }
        /// <summary>
        /// 导入模拟键盘的方法
        /// </summary>
        /// <param name="bVk" >按键的虚拟键值</param>
        /// <param name= "bScan" >扫描码，一般不用设置，用0代替就行</param>
        /// <param name= "dwFlags" >选项标志：0：表示按下，2：表示松开</param>
        /// <param name= "dwExtraInfo">一般设置为0</param>
        [DllImport("user32.dll", EntryPoint = "keybd_event", SetLastError = true)]
        public static extern void keybd_event(Keys bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

        public override void OnLTrigger(byte[] value)
        {
            if (value[0] > TriggerDeadArea)
            {
                if (KeyCache[0] == 0)
                {
                    keybd_event(this.config.LTrigger, 0, 0, 0);
                    KeyCache[0] = 1;
                }
            }
            else
            {
                keybd_event(this.config.LTrigger, 0, 2, 0);
                KeyCache[0] = 0;
            }
        }

        public override void OnRTrigger(byte[] value)
        {
            if (value[0] > TriggerDeadArea)
            {
                if (KeyCache[1] == 0)
                {
                    keybd_event(this.config.RTrigger, 0, 0, 0);
                    KeyCache[1] = 1;
                }
            }
            else
            {
                keybd_event(this.config.RTrigger, 0, 2, 0);
                KeyCache[1] = 0;
            }
        }

        public override void OnLeftRemote(byte[] x, byte[] y)
        {
            if (x[0] > 128 && (x[0] - RemoteDeadAreaOffset) > 128)
            {
                KeyCache[3] = 0;
                keybd_event(this.config.LeftRemoteLeft, 0, 2, 0);
                if (KeyCache[2] == 0)
                {
                    keybd_event(this.config.LeftRemoteRight, 0, 0, 0);
                    KeyCache[2] = 1;
                }
            }
            else if (x[0] < 128 && (x[0] + RemoteDeadAreaOffset) < 128)
            {
                KeyCache[2] = 0;
                keybd_event(this.config.LeftRemoteRight, 0, 2, 0);
                if (KeyCache[3] == 0)
                {
                    keybd_event(this.config.LeftRemoteLeft, 0, 0, 0);
                    KeyCache[3] = 1;
                }
            }
            else
            {
                keybd_event(this.config.LeftRemoteLeft, 0, 2, 0);
                keybd_event(this.config.LeftRemoteRight, 0, 2, 0);
                KeyCache[2] = 0;
                KeyCache[3] = 0;
            }



            if (y[0] > 128 && (y[0] - RemoteDeadAreaOffset) > 128)
            {
                KeyCache[5] = 0;
                keybd_event(this.config.LeftRemoteUp, 0, 2, 0);
                if (KeyCache[4] == 0)
                {
                    keybd_event(this.config.LeftRemoteDown, 0, 0, 0);
                    KeyCache[4] = 1;
                }
            }
            else if (y[0] < 128 && (y[0] + RemoteDeadAreaOffset) < 128)
            {
                KeyCache[4] = 0;
                keybd_event(this.config.LeftRemoteDown, 0, 2, 0);
                if (KeyCache[5] == 0)
                {
                    keybd_event(this.config.LeftRemoteUp, 0, 0, 0);
                    KeyCache[5] = 1;
                }
            }
            else
            {
                keybd_event(this.config.LeftRemoteUp, 0, 2, 0);
                keybd_event(this.config.LeftRemoteDown, 0, 2, 0);
                KeyCache[4] = 0;
                KeyCache[5] = 0;
            }
        }

        public override void OnRightRemote(byte[] x, byte[] y)
        {
            if (x[0] > 128 && (x[0] - RemoteDeadAreaOffset) > 128)
            {
                KeyCache[7] = 0;
                keybd_event(this.config.RightRemoteLeft, 0, 2, 0);
                if (KeyCache[6] == 0)
                {
                    keybd_event(this.config.RightRemoteRight, 0, 0, 0);
                    KeyCache[6] = 1;
                }
            }
            else if (x[0] < 128 && (x[0] + RemoteDeadAreaOffset) < 128)
            {
                KeyCache[6] = 0;
                keybd_event(this.config.RightRemoteRight, 0, 2, 0);
                if (KeyCache[7] == 0)
                {
                    keybd_event(this.config.RightRemoteLeft, 0, 0, 0);
                    KeyCache[7] = 1;
                }
            }
            else
            {
                keybd_event(this.config.RightRemoteLeft, 0, 2, 0);
                keybd_event(this.config.RightRemoteRight, 0, 2, 0);
                KeyCache[6] = 0;
                KeyCache[7] = 0;
            }



            if (y[0] > 128 && (y[0] - RemoteDeadAreaOffset) > 128)
            {
                KeyCache[9] = 0;
                keybd_event(this.config.RightRemoteUp, 0, 2, 0);
                if (KeyCache[8] == 0)
                {
                    keybd_event(this.config.RightRemoteDown, 0, 0, 0);
                    KeyCache[8] = 1;
                }
            }
            else if (y[0] < 128 && (y[0] + RemoteDeadAreaOffset) < 128)
            {
                KeyCache[8] = 0;
                keybd_event(this.config.RightRemoteDown, 0, 2, 0);
                if (KeyCache[9] == 0)
                {
                    keybd_event(this.config.RightRemoteUp, 0, 0, 0);
                    KeyCache[9] = 1;
                }
            }
            else
            {
                keybd_event(this.config.RightRemoteUp, 0, 2, 0);
                keybd_event(this.config.RightRemoteDown, 0, 2, 0);
                KeyCache[8] = 0;
                KeyCache[9] = 0;
            }
        }
    }
}
