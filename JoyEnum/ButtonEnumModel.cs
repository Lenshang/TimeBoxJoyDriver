using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeBoxJoy.MapConfig;

namespace TimeBoxJoy.JoyEnum
{
    public class ButtonEnumModel
    {
        int _value { get; set; }
        string _name { get; set; }
        public ButtonEnumModel(string name,int value,int offset=0)
        {
            this._name = name;
            this._value = value + offset;
        }
        public int ToInt()
        {
            return this._value;
        }
        public override string ToString()
        {
            return _name;
        }
    }
}
