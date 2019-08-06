using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TimeBoxJoy.MapConfig;
using TimeBoxJoy.Utils;

namespace TimeBoxJoy.Maps
{
    public class KeyBoardJoyMap : IJoyMap
    {
        //KeyBoardConfig config;
        public KeyBoardJoyMap(MapConfig.DefaultMapConfig config = null) : base(config)
        {
            if (config != null)
            {
                this.config = config;
            }
            else
            {
                this.config = new KeyBoardConfig();
            }
            this.Name = "Defalt Keyboard Map";
        }
        byte[] HoldKeyCache = new byte[4];
        byte[] KeyCache = new byte[10];
        public byte TriggerDeadArea = 120;
        public byte RemoteDeadAreaOffset = 50;

        public override bool Initialize(Action<Exception> FailedCallback)
        {
            return true;
        }
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

        public void ParseKey(byte bt,uint dwFlags = 0)
        {
            int target;
            if(this.config.Keymap.TryGetValue(bt,out target))
            {
                KeyEvent(target, 0, dwFlags, 0);
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

        public void KeyEvent(int key, byte bScan, uint dwFlags, uint dwExtraInfo)
        {
            if (key < 0)
            {
                return;
            }
            keybd_event((Keys)key, bScan, dwFlags, dwExtraInfo);
        }
        public override void OnLTrigger(byte[] value)
        {
            if (value[0] > TriggerDeadArea)
            {
                if (KeyCache[0] == 0)
                {
                    KeyEvent(this.config.LTrigger, 0, 0, 0);
                    KeyCache[0] = 1;
                }
            }
            else
            {
                KeyEvent(this.config.LTrigger, 0, 2, 0);
                KeyCache[0] = 0;
            }
        }

        public override void OnRTrigger(byte[] value)
        {
            if (value[0] > TriggerDeadArea)
            {
                if (KeyCache[1] == 0)
                {
                    KeyEvent(this.config.RTrigger, 0, 0, 0);
                    KeyCache[1] = 1;
                }
            }
            else
            {
                KeyEvent(this.config.RTrigger, 0, 2, 0);
                KeyCache[1] = 0;
            }
        }

        public override void OnLeftRemote(byte[] x, byte[] y)
        {
            
            if (x[0] > 128 && (x[0] - RemoteDeadAreaOffset) > 128)
            {
                KeyCache[3] = 0;
                KeyEvent(this.config.LeftRemoteLeft, 0, 2, 0);
                if (KeyCache[2] == 0)
                {
                    KeyEvent(this.config.LeftRemoteRight, 0, 0, 0);
                    KeyCache[2] = 1;
                }
            }
            else if (x[0] < 128 && (x[0] + RemoteDeadAreaOffset) < 128)
            {
                KeyCache[2] = 0;
                KeyEvent(this.config.LeftRemoteRight, 0, 2, 0);
                if (KeyCache[3] == 0)
                {
                    KeyEvent(this.config.LeftRemoteLeft, 0, 0, 0);
                    KeyCache[3] = 1;
                }
            }
            else
            {
                KeyEvent(this.config.LeftRemoteLeft, 0, 2, 0);
                KeyEvent(this.config.LeftRemoteRight, 0, 2, 0);
                KeyCache[2] = 0;
                KeyCache[3] = 0;
            }



            if (y[0] > 128 && (y[0] - RemoteDeadAreaOffset) > 128)
            {
                KeyCache[5] = 0;
                KeyEvent(this.config.LeftRemoteUp, 0, 2, 0);
                if (KeyCache[4] == 0)
                {
                    KeyEvent(this.config.LeftRemoteDown, 0, 0, 0);
                    KeyCache[4] = 1;
                }
            }
            else if (y[0] < 128 && (y[0] + RemoteDeadAreaOffset) < 128)
            {
                KeyCache[4] = 0;
                KeyEvent(this.config.LeftRemoteDown, 0, 2, 0);
                if (KeyCache[5] == 0)
                {
                    KeyEvent(this.config.LeftRemoteUp, 0, 0, 0);
                    KeyCache[5] = 1;
                }
            }
            else
            {
                KeyEvent(this.config.LeftRemoteUp, 0, 2, 0);
                KeyEvent(this.config.LeftRemoteDown, 0, 2, 0);
                KeyCache[4] = 0;
                KeyCache[5] = 0;
            }
        }

        public override void OnRightRemote(byte[] x, byte[] y)
        {
            if (x[0] > 128 && (x[0] - RemoteDeadAreaOffset) > 128)
            {
                KeyCache[7] = 0;
                KeyEvent(this.config.RightRemoteLeft, 0, 2, 0);
                if (KeyCache[6] == 0)
                {
                    KeyEvent(this.config.RightRemoteRight, 0, 0, 0);
                    KeyCache[6] = 1;
                }
            }
            else if (x[0] < 128 && (x[0] + RemoteDeadAreaOffset) < 128)
            {
                KeyCache[6] = 0;
                KeyEvent(this.config.RightRemoteRight, 0, 2, 0);
                if (KeyCache[7] == 0)
                {
                    KeyEvent(this.config.RightRemoteLeft, 0, 0, 0);
                    KeyCache[7] = 1;
                }
            }
            else
            {
                KeyEvent(this.config.RightRemoteLeft, 0, 2, 0);
                KeyEvent(this.config.RightRemoteRight, 0, 2, 0);
                KeyCache[6] = 0;
                KeyCache[7] = 0;
            }



            if (y[0] > 128 && (y[0] - RemoteDeadAreaOffset) > 128)
            {
                KeyCache[9] = 0;
                KeyEvent(this.config.RightRemoteUp, 0, 2, 0);
                if (KeyCache[8] == 0)
                {
                    KeyEvent(this.config.RightRemoteDown, 0, 0, 0);
                    KeyCache[8] = 1;
                }
            }
            else if (y[0] < 128 && (y[0] + RemoteDeadAreaOffset) < 128)
            {
                KeyCache[8] = 0;
                KeyEvent(this.config.RightRemoteDown, 0, 2, 0);
                if (KeyCache[9] == 0)
                {
                    KeyEvent(this.config.RightRemoteUp, 0, 0, 0);
                    KeyCache[9] = 1;
                }
            }
            else
            {
                KeyEvent(this.config.RightRemoteUp, 0, 2, 0);
                KeyEvent(this.config.RightRemoteDown, 0, 2, 0);
                KeyCache[8] = 0;
                KeyCache[9] = 0;
            }
        }

        public override void Dispose()
        {
            
        }
    }
}
