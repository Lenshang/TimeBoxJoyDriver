using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TimeBoxJoy.MapConfig
{
    class KeyBoardConfig : DefaultMapConfig
    {

        public KeyBoardConfig()
        {
            this.Name = "Default KeyBoard Map";
            this.MapType = MapType.KEYBOARD;
            this.Keymap= new Dictionary<byte, Int32>();
            Keymap.Add(4, (int)Keys.U);//A
            Keymap.Add(22, (int)Keys.I);//B
            Keymap.Add(27, (int)Keys.O);//x
            Keymap.Add(29, (int)Keys.P);//y
            Keymap.Add(26, (int)Keys.Q);//L1
            Keymap.Add(20, (int)Keys.E);//R1

            Keymap.Add(82, (int)Keys.W);//up
            Keymap.Add(81, (int)Keys.S);//down
            Keymap.Add(80, (int)Keys.A);//left
            Keymap.Add(79, (int)Keys.D);//right
            Keymap.Add(24, (int)Keys.Z);//back L
            Keymap.Add(23, (int)Keys.C);//back R
            Keymap.Add(89, (int)Keys.T);//Help
            Keymap.Add(74, (int)Keys.Y);//Home

            Keymap.Add(88, (int)Keys.G);//back
            Keymap.Add(44, (int)Keys.H);//start
            Keymap.Add(96, (int)Keys.V);//LC
            Keymap.Add(97, (int)Keys.B);//RC

            this.LTrigger = (int)Keys.J;
            this.RTrigger = (int)Keys.L;
            this.LeftRemoteUp = (int)Keys.W;
            this.LeftRemoteDown = (int)Keys.S;
            this.LeftRemoteLeft = (int)Keys.A;
            this.LeftRemoteRight = (int)Keys.D;
            this.RightRemoteUp = (int)Keys.Up;
            this.RightRemoteDown = (int)Keys.Down;
            this.RightRemoteLeft = (int)Keys.Left;
            this.RightRemoteRight = (int)Keys.Right;
        }
    }
}
