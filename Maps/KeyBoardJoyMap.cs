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
        public Dictionary<byte, Keys> keymap { get; set; } = new Dictionary<byte, Keys>();

        public KeyBoardConfig()
        {
            keymap.Add(4, Keys.U);//A
            keymap.Add(22, Keys.I);//B
            keymap.Add(27, Keys.O);//x
            keymap.Add(29, Keys.P);//y
            keymap.Add(26, Keys.Q);//L1
            keymap.Add(20, Keys.E);//R1

            keymap.Add(82, Keys.W);//up
            keymap.Add(81, Keys.S);//down
            keymap.Add(80, Keys.A);//left
            keymap.Add(79, Keys.D);//right
            keymap.Add(24, Keys.Z);//back L
            keymap.Add(23, Keys.C);//back R
            keymap.Add(89, Keys.T);//Help
            keymap.Add(74, Keys.Y);//Home

            keymap.Add(88, Keys.G);//back
            keymap.Add(44, Keys.H);//start
            keymap.Add(96, Keys.V);//LC
            keymap.Add(97, Keys.B);//RC
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

        private void ParseKey(byte bt,uint dwFlags = 1)
        {
            Keys target;
            if(this.config.keymap.TryGetValue(bt,out target))
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
    }
}
