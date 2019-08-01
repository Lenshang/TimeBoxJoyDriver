using Nefarius.ViGEm.Client.Targets.Xbox360;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeBoxJoy.MapConfig
{
    public class MixModeConfig: DefaultMapConfig
    {
        public MixModeConfig()
        {
            this.Name = "Default MixMode Map";
            this.MapType = MapType.MIX;
            this.LTrigger = (int)Xbox360Axes.LeftTrigger + 65535;
            this.RTrigger = (int)Xbox360Axes.RightTrigger + 65535;
            this.LeftRemoteX = (int)Xbox360Axes.LeftThumbX + 65535;
            this.LeftRemoteY = (int)Xbox360Axes.LeftThumbY + 65535;
            this.RightRemoteX = (int)Xbox360Axes.RightThumbX + 65535;
            this.RightRemoteY = (int)Xbox360Axes.RightThumbY + 65535;
            this.Keymap = new Dictionary<byte, int>();
            Keymap.Add(4, (int)Xbox360Buttons.A + 65535);//A
            Keymap.Add(22, (int)Xbox360Buttons.B + 65535);//B
            Keymap.Add(27, (int)Xbox360Buttons.X + 65535);//x
            Keymap.Add(29, (int)Xbox360Buttons.Y + 65535);//y
            Keymap.Add(26, (int)Xbox360Buttons.LeftShoulder + 65535);//L1
            Keymap.Add(20, (int)Xbox360Buttons.RightShoulder + 65535);//R1

            Keymap.Add(82, (int)Xbox360Buttons.Up + 65535);//up
            Keymap.Add(81, (int)Xbox360Buttons.Down + 65535);//down
            Keymap.Add(80, (int)Xbox360Buttons.Left + 65535);//left
            Keymap.Add(79, (int)Xbox360Buttons.Right + 65535);//right
            Keymap.Add(24, (int)Xbox360Buttons.LeftThumb + 65535);//back L
            Keymap.Add(23, (int)Xbox360Buttons.RightThumb + 65535);//back R
            Keymap.Add(89, 0);//Help
            Keymap.Add(74, (int)Xbox360Buttons.Guide + 65535);//Home

            Keymap.Add(88, (int)Xbox360Buttons.Back + 65535);//back
            Keymap.Add(44, (int)Xbox360Buttons.Start + 65535);//start
            Keymap.Add(96, (int)Xbox360Buttons.LeftThumb + 65535);//LC
            Keymap.Add(97, (int)Xbox360Buttons.RightThumb + 65535);//RC
        }
    }
}
