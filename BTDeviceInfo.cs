using InTheHand.Net;
using InTheHand.Net.Sockets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TimeBoxJoy.MapConfig;
using TimeBoxJoy.Maps;
using TimeBoxJoy.Utils;

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
        public IJoyMap joyMap { get; set; }
        public DefaultMapConfig mapConfig { get; set; }
        public BTDeviceInfo(BluetoothDeviceInfo deviceinfo)
        {
            bluetoothDeviceInfo = deviceinfo;
        }
        public deviceState State { get; set; } = deviceState.DISCONNECT;

        public void SetJoyMap(JoyManager manager,DefaultMapConfig config =null)
        {
            IJoyMap map = new KeyBoardJoyMap();
            if (config != null)
            {
                switch (config.MapType)
                {
                    case MapType.KEYBOARD:
                        if (this.joyMap != null)
                        {
                            this.joyMap.Dispose();
                        }
                        this.mapConfig = config;
                        this.joyMap = new KeyBoardJoyMap(config);
                        this.joyMap.Initialize(f=> { });
                        this.joyStick.SetJoyMap(this.joyMap);
                        return;
                    case MapType.XINPUT:
                        if (this.joyMap != null)
                        {
                            this.joyMap.Dispose();
                        }
                        this.mapConfig = config;
                        this.joyMap = new VitualXinputJoyMap(config);
                        if(!this.joyMap.Initialize(ex => {
                            manager.WriteLog(ex.Message + "\r\n已切换回键盘映射模式");
                        }))
                        {
                            map = new KeyBoardJoyMap();
                        }
                        this.joyStick.SetJoyMap(this.joyMap);
                        return;
                    case MapType.MIX:
                        if (this.joyMap != null)
                        {
                            this.joyMap.Dispose();
                        }
                        this.mapConfig = config;
                        this.joyMap = new MixModeJoyMap(config);
                        if (!this.joyMap.Initialize(ex => {
                            manager.WriteLog(ex.Message + "\r\n已切换回键盘映射模式");
                        }))
                        {
                            map = new KeyBoardJoyMap();
                        }
                        this.joyStick.SetJoyMap(this.joyMap);
                        return;
                }
            }

            map = new VitualXinputJoyMap();
            if (!map.Initialize(ex => {
                manager.WriteLog(ex.Message + "\r\n已切换回键盘映射模式");
                //MessageBox.Show(ex.Message + "\r\n已切换回键盘映射模式");
            }))
            {
                map = new KeyBoardJoyMap();
            }
            this.joyMap = map;
            this.mapConfig = map.config;
            this.joyStick.SetJoyMap(map);
        }

        public void SendData(byte[] data)
        {
            this.joyStick.SendData(data);
        }
        public override string ToString()
        {
            return this.bluetoothDeviceInfo.DeviceAddress + $"[{this.State}]";
        }
    }
}
