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

        public virtual void OnKeyArray(byte[] buffer)
        {

        }
        public virtual void OnLTrigger(byte[] value)
        {

        }
        public virtual void OnRTrigger(byte[] value)
        {

        }
        public virtual void OnLeftRemote(byte[] x, byte[] y)
        {

        }
        public virtual void OnRightRemote(byte[] x, byte[] y)
        {

        }

        public virtual void OnBuffer(byte[] keyBuffer, byte[] lTrigger, byte[] rTrigger, byte[] lRemoteX, byte[] lRemoteY, byte[] rRemoteX, byte[] rRemoteY)
        {
            OnKeyArray(keyBuffer);
            OnLeftRemote(lRemoteX, lRemoteY);
            OnRightRemote(rRemoteX, rRemoteY);
            OnLTrigger(lTrigger);
            OnRTrigger(rTrigger);
        }
    }
}
