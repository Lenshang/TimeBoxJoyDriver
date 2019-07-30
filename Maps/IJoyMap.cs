using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeBoxJoy
{
    public abstract class IJoyMap
    {
        public void SetKeyArray(byte[] buffer)
        {
            this.OnKeyArray(buffer);
        }

        public void SetLeftRemote(byte[] x,byte[] y)
        {

        }

        public abstract void OnKeyArray(byte[] buffer);
    }
}
