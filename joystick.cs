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
using TimeBoxJoy.Utils;

namespace TimeBoxJoy
{

    public class JoyStick
    {
        private BluetoothClient cli;

        private BluetoothEndPoint ep;

        private NetworkStream peerStream;

        private uint joyStickId;

        private IJoyMap joyMap;

        private int HeartBeatInterval=0;

        private byte[] buffer = new byte[18];
        private byte[] sign;

        //按键相关Buffer
        byte[] _checkHeader = new byte[3];
        byte[] _check = new byte[1];
        byte[] _keyArray = new byte[4];
        byte[] _placeHolder = new byte[1];
        byte[] _leftX = new byte[1];
        byte[] _leftY = new byte[1];
        byte[] _rightX = new byte[1];
        byte[] _rightY = new byte[1];
        byte[] _leftTrigger = new byte[1];
        byte[] _rightTrigger = new byte[1];
        byte[] _btSum = new byte[3];

        public Action<byte[]> OnReceive;
        public JoyStick(string macAddr)
        {
            BluetoothAddress address = BluetoothAddress.Parse(macAddr);
            Guid serialPort = BluetoothService.SerialPort;
            this.ep = new BluetoothEndPoint(address, serialPort);
            this.cli = new BluetoothClient();
            this.sign = HexStrTobyte("EEC10F");
        }
        public void SetJoyMap(IJoyMap map)
        {
            this.joyMap = map;
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
            StartRead();
        }

        #region 传输+解析
        public void StartRead()
        {
            //版本1
            //this.BeginReadStream(_checkHeader, 3, ParseHeader);
            //版本2
            this.BeginReadStream(buffer, 18, ParseAll);

            //this.peerStream.BeginRead(_checkHeader, 0,3,new AsyncCallback(ParseHeader),null);
        }
        /// <summary>
        /// 版本2 一口气读取18个字节
        /// </summary>
        /// <param name="ar"></param>
        public void ParseAll(IAsyncResult ar)
        {
            if (EndReadStream(ar) >= 0)
            {
                MemoryStream ms = new MemoryStream(buffer);
                ms.Read(_checkHeader, 0, 3);
                if (!byteEqual(this.sign, _checkHeader))
                {
                    this.BeginReadStream(buffer, 18, ParseAll);
                    return;
                }
                ms.Read(_check, 0, 1);
                ms.Read(_keyArray, 0, 4);
                ms.Read(_placeHolder, 0, 1);
                ms.Read(_leftX, 0, 1);
                ms.Read(_leftY, 0, 1);
                ms.Read(_rightX, 0, 1);
                ms.Read(_rightY, 0, 1);
                ms.Read(_leftTrigger, 0, 1);
                ms.Read(_rightTrigger, 0, 1);
                ms.Read(_btSum, 0, 3);
                ParseAction();
                this.BeginReadStream(buffer, 18, ParseAll);
            }
        }
        public void ParseHeader(IAsyncResult ar)
        {
            if (EndReadStream(ar) >= 0)
            {
                if (!byteEqual(this.sign, _checkHeader))
                {
                    StartRead();
                }
                else
                {
                    this.BeginReadStream(_check, 1, ParseCheck);
                    //this.peerStream.BeginRead(_check, 0, 1, new AsyncCallback(ParseCheck), null);
                }
            }

        }
        public void ParseCheck(IAsyncResult ar)
        {
            if (EndReadStream(ar) >= 0)
            {
                //TODO 验证Check

                this.BeginReadStream(_keyArray, 4, ParseKeyArray);
                //this.peerStream.BeginRead(_keyArray, 0, 4, new AsyncCallback(ParseKeyArray), null);
            }

        }
        public void ParseKeyArray(IAsyncResult ar)
        {
            if (EndReadStream(ar) >= 0)
            {
                this.BeginReadStream(_placeHolder, 1, ParsePlaceHolder);
                //this.peerStream.BeginRead(_placeHolder, 0, 1, new AsyncCallback(ParsePlaceHolder), null);
            }
        }
        public void ParsePlaceHolder(IAsyncResult ar)
        {
            if (EndReadStream(ar) >= 0)
            {
                this.BeginReadStream(_leftX, 1, ParseLeftX);
                //this.peerStream.BeginRead(_leftX, 0, 1, new AsyncCallback(ParseLeftX), null);
            }
        }
        /// <summary>
        /// 解析左摇杆X轴
        /// </summary>
        /// <param name="ar"></param>
        public void ParseLeftX(IAsyncResult ar)
        {
            if (EndReadStream(ar) >= 0)
            {
                //TODO 
                this.BeginReadStream(_leftY, 1, ParseLeftY);
                //this.peerStream.BeginRead(_leftY, 0, 1, new AsyncCallback(ParseLeftY), null);
            }
        }
        /// <summary>
        /// 解析左摇杆Y轴
        /// </summary>
        /// <param name="ar"></param>
        public void ParseLeftY(IAsyncResult ar)
        {
            if (EndReadStream(ar) >= 0)
            {
                this.BeginReadStream(_rightX, 1, ParseRightX);
                //this.peerStream.BeginRead(_rightX, 0, 1, new AsyncCallback(ParseRightX), null);
            }
        }
        /// <summary>
        /// 解析右摇杆X轴
        /// </summary>
        /// <param name="ar"></param>
        public void ParseRightX(IAsyncResult ar)
        {
            if (EndReadStream(ar) >= 0)
            {
                //TODO 
                this.BeginReadStream(_rightY, 1, ParseRightY);
                //this.peerStream.BeginRead(_rightY, 0, 1, new AsyncCallback(ParseRightY), null);
            }
        }
        /// <summary>
        /// 解析右摇杆Y轴
        /// </summary>
        /// <param name="ar"></param>
        public void ParseRightY(IAsyncResult ar)
        {
            if (EndReadStream(ar) >= 0)
            {
                this.BeginReadStream(_leftTrigger, 1, ParseLeftTrigger);
                //this.peerStream.BeginRead(_leftTrigger, 0, 1, new AsyncCallback(ParseLeftTrigger), null);
            }

        }
        /// <summary>
        /// 解析左扳机键
        /// </summary>
        /// <param name="ar"></param>
        public void ParseLeftTrigger(IAsyncResult ar)
        {
            if (EndReadStream(ar) >= 0)
            {
                this.BeginReadStream(_rightTrigger, 1, ParseRightTrigger);
                //this.peerStream.BeginRead(_rightTrigger, 0, 1, new AsyncCallback(ParseRightTrigger), null);
            }
        }
        /// <summary>
        /// 解析右扳机键
        /// </summary>
        /// <param name="ar"></param>
        public void ParseRightTrigger(IAsyncResult ar)
        {
            if (EndReadStream(ar) >= 0)
            {
                this.BeginReadStream(_btSum, 3, ParseButtomSum);
                //this.peerStream.BeginRead(_btSum, 0, 3, new AsyncCallback(ParseButtomSum), null);
            }
        }
        /// <summary>
        /// 解析按键和值计算验证
        /// </summary>
        /// <param name="ar"></param>
        public void ParseButtomSum(IAsyncResult ar)
        {
            if (EndReadStream(ar) >= 0)
            {
                ParseAction();
                StartRead();
            }
        }
        private bool BeginReadStream(byte[] buffer,int size,Action<IAsyncResult> callback)
        {
            try
            {
                this.peerStream.BeginRead(buffer, 0, size, new AsyncCallback(callback), null);
                return true;
            }
            catch
            {
                this.Disconnect();
                return false;
            }
        }
        private int EndReadStream(IAsyncResult ar)
        {
            try
            {
                var count = this.peerStream.EndRead(ar);
                return count;
            }
            catch
            {
                this.Disconnect();
                return -1;
            }
        }
        #endregion
        public void ParseAction()
        {
            //验证Check
            uint num = 0;
            foreach (byte b in _keyArray)
            {
                num += b;
            }
            num += _leftX[0];
            num += _leftY[0];
            num += _rightX[0];
            num += _rightY[0];
            num += _leftTrigger[0];
            num += _rightTrigger[0];
            foreach (byte b in _btSum)
            {
                num += b;
            }
            num = (num - 15u) % 256u;
            if (num == _check[0])
            {
#if DEBUG
                //测试用
                byte[] all = new byte[18];
                _checkHeader.CopyTo(all, 0);
                _check.CopyTo(all, 3);
                _keyArray.CopyTo(all, 4);
                _leftX.CopyTo(all, 9);
                _leftY.CopyTo(all, 10);
                _rightX.CopyTo(all, 11);
                _rightY.CopyTo(all, 12);
                _leftTrigger.CopyTo(all, 13);
                _rightTrigger.CopyTo(all, 14);
                _btSum.CopyTo(all, 15);
                this.OnReceive?.Invoke(all);
                //System.Diagnostics.Debug.WriteLine(HexHelper.byteToHexStr(all, all.Length));
#endif
                //TODO 
                this.joyMap.OnBuffer(_keyArray, _leftTrigger, _rightTrigger, _leftX, _leftY, _rightX, _rightY);
            }
        }
        public bool CheckConnect()
        {
            HeartBeatInterval += 1;
            if (HeartBeatInterval < 5)
            {
                return true;
            }

            HeartBeatInterval = 0;
            try
            {
                this.peerStream.Write(new byte[1], 0, 1);
                return true;
            }
            catch
            {
                this.Disconnect();
                return false;
            }
        }
        public void Disconnect()
        {
            try
            {
                this.peerStream.Dispose();
                this.cli.Dispose();
                this.joyMap.Dispose();
            }
            catch
            {

            }
        }
        public void SendData(byte[] data)
        {
            this.peerStream.Write(data,0,data.Length);
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
