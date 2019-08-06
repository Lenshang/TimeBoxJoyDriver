using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeBoxJoy.Utils;

namespace TimeBoxJoy.MapConfig
{
    public enum MapType
    {
        KEYBOARD=0,
        XINPUT=1,
        MIX=2,
        SCRIPT=3
    }
    public class DefaultMapConfig
    {
        public string Name { get; set; } = "Ptilopsis";
        public MapType MapType { get; set; }
        public Dictionary<byte, Int32> Keymap { get; set; } = new Dictionary<byte, Int32>();
        public Int32 LTrigger { get; set; }
        public Int32 RTrigger { get; set; }
        public Int32 LeftRemoteUp { get; set; }
        public Int32 LeftRemoteDown { get; set; }
        public Int32 LeftRemoteLeft { get; set; }
        public Int32 LeftRemoteRight { get; set; }
        public Int32 RightRemoteUp { get; set; }
        public Int32 RightRemoteDown { get; set; }
        public Int32 RightRemoteLeft { get; set; }
        public Int32 RightRemoteRight { get; set; }
        public Int32 LeftRemoteX { get; set; }
        public Int32 LeftRemoteY { get; set; }
        public Int32 RightRemoteX { get; set; }
        public Int32 RightRemoteY { get; set; }
        public override string ToString()
        {
            return this.Name;
        }
        public static DefaultMapConfig GetConfig(string file)
        {
            try
            {
                FileHelper fh = new FileHelper();
                if (File.Exists(file))
                {
                    var str = fh.readFile(file);
                    DefaultMapConfig obj = JsonConvert.DeserializeObject<DefaultMapConfig>(str);
                    return obj;
                    //var _type = obj["MapType"].ToObject<MapType>();
                    //switch (_type)
                    //{
                    //    case MapType.XINPUT:
                    //        return obj.ToObject<XInputConfig>();
                    //    case MapType.KEYBOARD:
                    //        return obj.ToObject<KeyBoardConfig>();
                    //    default:
                    //        return null;
                    //}
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
