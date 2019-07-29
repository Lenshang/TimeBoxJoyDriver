using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TimeBoxJoy
{

    public class JoyStick
    {
        private BluetoothClient cli;

        private BluetoothEndPoint ep;

        private NetworkStream peerStream;

        private uint joyStickId;

        private Xbox360Controller vjoy;

        private byte[] buffer = new byte[64];
        private byte[] sign;

        //按键相关Buffer
        byte[] _checkSign = new byte[3];
        byte _check = new byte();
        byte[] _tmpArry = new byte[4];
        byte _leftX = new byte();
        byte _leftY = new byte();
        byte _rightX = new byte();
        byte _rightY = new byte();
        byte _leftTrigger = new byte();
        byte _rightTrigger = new byte();
        byte _bt1 = new byte();
        byte _bt2 = new byte();
        byte _bt3 = new byte();

        public Action<byte[]> OnReceive;
        public JoyStick(string macAddr)
        {
            BluetoothAddress address = BluetoothAddress.Parse(macAddr);
            Guid serialPort = BluetoothService.SerialPort;
            this.ep = new BluetoothEndPoint(address, serialPort);
            this.cli = new BluetoothClient();
            this.vjoy = new Xbox360Controller(new ViGEmClient());
            this.sign = HexStrTobyte("EEC10F");
        }

        public bool StartConnect(int id)
        {
            bool result;
            try
            {
                this.cli.Connect(this.ep);
                this.joyStickId = (uint)(id + 1);
                this.peerStream = this.cli.GetStream();
                Stream _stream = this.peerStream;
                byte[] secret = new byte[]
                {
                    3,
                    85,
                    170,
                    1,
                    11,
                    1,
                    0,
                    0
                };
                secret[6] = (byte)this.joyStickId;
                _stream.Write(this.getSignStr(secret), 0, 8);
                result = true;
            }
            catch (Exception arg_5D_0)
            {
                Console.WriteLine(arg_5D_0);
                result = false;
            }
            return result;
        }
        public void startFeed()
        {
            //Thread thr = new Thread(new ThreadStart(()=> {
            //    while (true)
            //    {
            //        if (this.peerStream.DataAvailable)
            //        {
            //            byte[] outbyte = new byte[18];
            //            if (this.peerStream.Read(outbyte, 0, 18) == 18)
            //            {
            //                string text = byteToHexStr(outbyte, 18);
            //                if (checkSign(text))
            //                {
            //                    callback.Invoke(text);
            //                }
            //            }
            //        }
            //        else
            //        {
            //            Thread.Sleep(100);
            //        }
            //    }
            //}));
            //thr.Start();
            StartRead();
        }
        public void StartRead()
        {
            this.peerStream.BeginRead(buffer,0,64,new AsyncCallback(BeginReadCallback),null);
        }
        public void BeginReadCallback(IAsyncResult ar)
        {
            var count=this.peerStream.EndRead(ar);
            //if (count == 18)
            //{
            //    this.OnReceive?.Invoke(buffer);
            //}
            //else
            //{
            //    this.OnReceive?.Invoke(buffer);
            //}
            this.OnReceive?.Invoke(buffer);
            Parse();
            StartRead();
        }
        public bool CheckConnect()
        {
            return this.cli.Connected;
        }

        private bool checkSign(string keyCode)
        {
            uint num = 0u;
            if (keyCode.Substring(0, 6) != "EEC10F")
            {
                return false;
            }
            string text = keyCode.Substring(6, 2);
            for (int i = 4; i < 18; i++)
            {
                num += Convert.ToUInt32(keyCode.Substring(i * 2, 2), 16);
            }
            num = (num - 15u) % 256u;
            if (num != Convert.ToUInt32(text, 16))
            {
                Console.WriteLine(string.Concat(new string[]
                {
                    keyCode,
                    "|Err:",
                    num.ToString(),
                    "|",
                    text
                }));
                return false;
            }
            return true;
        }

        private void Parse()
        {
            MemoryStream stream = new MemoryStream(this.buffer);
            stream.Seek(0, SeekOrigin.Begin);
            StreamReader reader = new StreamReader(stream);


            stream.Read(_checkSign, 0, 3);
            if (!byteEqual(this.sign, _checkSign))
            {
                return;
            }

            
        }
        private byte[] getSignStr(byte[] signStr)
        {
            int i = 0;
            byte b = 2;
            while (i < signStr.Length - 1)
            {
                b = (byte)(b - signStr[i] & 255);
                i++;
            }
            signStr[7] = b;
            return signStr;
        }

        private bool byteEqual(byte[] b1, byte[] b2)
        {
            var count = b1.Count();
            if (count != b2.Count())
            {
                return false;
            }
            for(int i=0;i< count; i++)
            {
                if (b1[i] != b2[i])
                {
                    return false;
                }
            }
            return true;
        }
        private byte[] HexStrTobyte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2).Trim(), 16);
            return returnBytes;
        }
    }
}
