using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeBoxJoy.MapConfig
{
    public class ScriptModeConfig:DefaultMapConfig
    {
        public ScriptModeConfig()
        {
            this.Name = "Default Script Map";
            this.MapType = MapType.SCRIPT;
            this.Keymap = new Dictionary<byte, Int32>();
            Keymap.Add(4, 1948332219);//A
            Keymap.Add(22, 1948332219);//B
            Keymap.Add(27, 1948332219);//x
            Keymap.Add(29, 1948332219);//y
            Keymap.Add(26, 1948332219);//L1
            Keymap.Add(20, 1948332219);//R1

            Keymap.Add(82, 1948332219);//up
            Keymap.Add(81, 1948332219);//down
            Keymap.Add(80, 1948332219);//left
            Keymap.Add(79, 1948332219);//right
            Keymap.Add(24, 1948332219);//back L
            Keymap.Add(23, 1948332219);//back R
            Keymap.Add(89, 1948332219);//Help
            Keymap.Add(74, 1948332219);//Home

            Keymap.Add(88, 1948332219);//back
            Keymap.Add(44, 1948332219);//start
            Keymap.Add(96, 1948332219);//LC
            Keymap.Add(97, 1948332219);//RC

            this.LTrigger = 1948332219;
            this.RTrigger = 1948332219;
            this.LeftRemoteUp = 1948332219;
            this.LeftRemoteDown = 1948332219;
            this.LeftRemoteLeft = 1948332219;
            this.LeftRemoteRight = 1948332219;
            this.RightRemoteUp = 1948332219;
            this.RightRemoteDown = 1948332219;
            this.RightRemoteLeft = 1948332219;
            this.RightRemoteRight = 1948332219;
        }
    }
}
