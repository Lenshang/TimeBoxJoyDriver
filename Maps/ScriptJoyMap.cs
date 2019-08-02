using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeBoxJoy.MapConfig;

namespace TimeBoxJoy.Maps
{
    public class ScriptJoyMap : IJoyMap
    {
        public ScriptJoyMap(DefaultMapConfig _config) : base(_config)
        {
            //TODO 创建脚本数据库 读取所有脚本
            if (!Directory.Exists("joyScripts"))
            {
                Directory.CreateDirectory("joyScripts");
            }
            foreach(var item in Directory.GetFiles("joyScripts"))
            {

            }
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public override bool Initialize(Action<Exception> FailedCallback)
        {
            throw new NotImplementedException();
        }
    }
}
