using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeBoxJoy.MapConfig;

namespace TimeBoxJoy.Maps
{
    public class MixModeJoyMap : IJoyMap
    {
        DefaultMapConfig KeyBoardConfig { get; set; }
        DefaultMapConfig XinputConfig { get; set; }
        IJoyMap KeyBoardJoyMap { get; set; }
        IJoyMap XinputJoyMap { get; set; }
        ///// <summary>
        ///// 虚拟手柄的BYTE信号缓冲区
        ///// </summary>
        //byte[] XinputKeyBuffer { get; set; } = new byte[4];
        //byte[] XinputLTriggerBuffer { get; set; } = new byte[1];
        //byte[] XinputRTriggerBuffer { get; set; } = new byte[1];
        //byte[] XinputLRemoteXBuffer { get; set; } = new byte[1];
        //byte[] XinputLRemoteYBuffer { get; set; } = new byte[1];
        //byte[] XinputRRemoteXBuffer { get; set; } = new byte[1];
        //byte[] XinputRRemoteYBuffer { get; set; } = new byte[1];
        ///// <summary>
        ///// 键盘的BYTE信号缓冲区
        ///// </summary>
        //byte[] KeyBoardKeyBuffer { get; set; } = new byte[4];
        //byte[] KeyBoardLTriggerBuffer { get; set; } = new byte[1];
        //byte[] KeyBoardRTriggerBuffer { get; set; } = new byte[1];
        //byte[] KeyBoardLRemoteXBuffer { get; set; } = new byte[1];
        //byte[] KeyBoardLRemoteYBuffer { get; set; } = new byte[1];
        //byte[] KeyBoardRRemoteXBuffer { get; set; } = new byte[1];
        //byte[] KeyBoardRRemoteYBuffer { get; set; } = new byte[1];
        public MixModeJoyMap(DefaultMapConfig _config) : base(_config)
        {
            if (_config==null)
            {
                _config = new XInputConfig();
            }

            KeyBoardConfig = new DefaultMapConfig();
            XinputConfig = new DefaultMapConfig();
            config = _config;
        }
        public override bool Initialize(Action<Exception> FailedCallback)
        {
            try
            {
                foreach (var kv in config.Keymap)
                {
                    if (kv.Value >= 65535)//大于65535的归为360摇杆
                    {
                        this.XinputConfig.Keymap[kv.Key] = kv.Value - 65535;
                    }
                    else
                    {
                        this.KeyBoardConfig.Keymap[kv.Key] = kv.Value;
                    }
                }

                this.XinputConfig.LeftRemoteX = this.config.LeftRemoteX - 65535;
                this.XinputConfig.LeftRemoteY = this.config.LeftRemoteY - 65535;
                this.XinputConfig.RightRemoteX = this.config.RightRemoteX - 65535;
                this.XinputConfig.RightRemoteY = this.config.RightRemoteY - 65535;

                this.KeyBoardConfig.LeftRemoteUp = this.config.LeftRemoteUp;
                this.KeyBoardConfig.LeftRemoteDown = this.config.LeftRemoteDown;
                this.KeyBoardConfig.LeftRemoteLeft = this.config.LeftRemoteLeft;
                this.KeyBoardConfig.LeftRemoteRight = this.config.LeftRemoteRight;
                this.KeyBoardConfig.RightRemoteUp = this.config.RightRemoteUp;
                this.KeyBoardConfig.RightRemoteDown = this.config.RightRemoteDown;
                this.KeyBoardConfig.RightRemoteLeft = this.config.RightRemoteLeft;
                this.KeyBoardConfig.RightRemoteRight = this.config.RightRemoteRight;

                if (this.config.LTrigger >= 65535)
                {
                    this.XinputConfig.LTrigger = this.config.LTrigger - 65535;
                }
                else
                {
                    this.KeyBoardConfig.LTrigger = this.config.LTrigger;
                }

                if (this.config.RTrigger >= 65535)
                {
                    this.XinputConfig.RTrigger = this.config.RTrigger - 65535;
                }
                else
                {
                    this.KeyBoardConfig.RTrigger = this.config.RTrigger;
                }
                this.KeyBoardJoyMap = new KeyBoardJoyMap(this.KeyBoardConfig);
                this.XinputJoyMap = new VitualXinputJoyMap(this.XinputConfig);
                this.KeyBoardJoyMap.Initialize(FailedCallback);
                this.XinputJoyMap.Initialize(FailedCallback);
                return true;
            }
            catch(Exception ex)
            {
                FailedCallback.Invoke(ex);
                return false;
            }
        }

        public override void OnBuffer(byte[] keyBuffer, byte[] lTrigger, byte[] rTrigger, byte[] lRemoteX, byte[] lRemoteY, byte[] rRemoteX, byte[] rRemoteY)
        {
            this.XinputJoyMap.OnBuffer(keyBuffer, lTrigger, rTrigger, lRemoteX, lRemoteY, rRemoteX, rRemoteY);
            this.KeyBoardJoyMap.OnBuffer(keyBuffer, lTrigger, rTrigger, lRemoteX, lRemoteY, rRemoteX, rRemoteY);
        }

        public override void Dispose()
        {
            this.KeyBoardJoyMap.Dispose();
            this.XinputJoyMap.Dispose();
        }
    }
}
