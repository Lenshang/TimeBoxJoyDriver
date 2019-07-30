using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeBoxJoy
{
    public abstract class IJoyMap
    {
        public void SetLeftRemote(byte[] x,byte[] y)
        {

        }

        public abstract void OnKeyArray(byte[] buffer);
        public abstract void OnLTrigger(byte[] value);
        public abstract void OnRTrigger(byte[] value);
        public abstract void OnLeftRemote(byte[] x, byte[] y);
        public abstract void OnRightRemote(byte[] x, byte[] y);
    }
}
