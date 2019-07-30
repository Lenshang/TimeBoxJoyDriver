using InTheHand.Net;
using InTheHand.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeBoxJoy
{
    public enum deviceState
    {
        DISCONNECT,
        CONNECTING,
        CONNECT,
        LOST
    }
    public class BTDeviceInfo
    {
        public BluetoothDeviceInfo bluetoothDeviceInfo { get; set; }

        public JoyStick joyStick { get; set; }
        public BTDeviceInfo(BluetoothDeviceInfo deviceinfo)
        {
            bluetoothDeviceInfo = deviceinfo;
        }
        public deviceState State { get; set; } = deviceState.DISCONNECT;

        public override string ToString()
        {
            return this.bluetoothDeviceInfo.DeviceAddress + $"[{this.State}]";
        }
    }
}
