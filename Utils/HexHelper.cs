using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeBoxJoy.Utils
{
    public class HexHelper
    {
        public static string byteToHexStr(byte[] bytes, int length)
        {
            string returnStr = "";
            if (bytes != null)
            {
                for (int i = 0; i < length; i++)
                {
                    returnStr += bytes[i].ToString("X2");
                }
            }
            return returnStr;
        }

        public static byte[] getSignStr(byte[] signStr)
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
    }
}
