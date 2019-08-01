using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace TimeBoxJoy.MapConfig
{
    public class XInputConfig : DefaultMapConfig
    {
        public XInputConfig()
        {
            this.Name = "Default 360(XInput) Map";
            this.MapType = MapType.XINPUT;
            this.LTrigger= (int)Xbox360Axes.LeftTrigger;
            this.RTrigger = (int)Xbox360Axes.RightTrigger;
            this.LeftRemoteX = (int)Xbox360Axes.LeftThumbX;
            this.LeftRemoteY = (int)Xbox360Axes.LeftThumbY;
            this.RightRemoteX = (int)Xbox360Axes.RightThumbX;
            this.RightRemoteY = (int)Xbox360Axes.RightThumbY;
            this.Keymap = new Dictionary<byte, int>();
            Keymap.Add(4, (int)Xbox360Buttons.A);//A
            Keymap.Add(22, (int)Xbox360Buttons.B);//B
            Keymap.Add(27, (int)Xbox360Buttons.X);//x
            Keymap.Add(29, (int)Xbox360Buttons.Y);//y
            Keymap.Add(26, (int)Xbox360Buttons.LeftShoulder);//L1
            Keymap.Add(20, (int)Xbox360Buttons.RightShoulder);//R1

            Keymap.Add(82, (int)Xbox360Buttons.Up);//up
            Keymap.Add(81, (int)Xbox360Buttons.Down);//down
            Keymap.Add(80, (int)Xbox360Buttons.Left);//left
            Keymap.Add(79, (int)Xbox360Buttons.Right);//right
            Keymap.Add(24, (int)Xbox360Buttons.LeftThumb);//back L
            Keymap.Add(23, (int)Xbox360Buttons.RightThumb);//back R
            Keymap.Add(89, 0);//Help
            Keymap.Add(74, (int)Xbox360Buttons.Guide);//Home

            Keymap.Add(88, (int)Xbox360Buttons.Back);//back
            Keymap.Add(44, (int)Xbox360Buttons.Start);//start
            Keymap.Add(96, (int)Xbox360Buttons.LeftThumb);//LC
            Keymap.Add(97, (int)Xbox360Buttons.RightThumb);//RC
        }
    }
}
