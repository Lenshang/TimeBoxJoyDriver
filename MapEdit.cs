using Nefarius.ViGEm.Client.Targets.Xbox360;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TimeBoxJoy.JoyEnum;
using TimeBoxJoy.MapConfig;
using TimeBoxJoy.Utils;

namespace TimeBoxJoy
{
    public partial class MapEdit : Form
    {
        JoyManager manager { get; set; }
        List<DefaultMapConfig> configs { get; set; }
        BTDeviceInfo device { get; set; }
        public MapEdit(JoyManager _manager,List<DefaultMapConfig> _configs, BTDeviceInfo _device)
        {
            InitializeComponent();
            this.manager = _manager;
            this.configs = _configs;
            this.device = _device;
        }

        private void MapEdit_Load(object sender, EventArgs e)
        {
            foreach(var config in this.configs)
            {
                this.cbConfigList.Items.Add(config);
            }
            var useConfig = this.configs.Where(i => i.Name == device.mapConfig.Name).FirstOrDefault();
            if (useConfig != null)
            {
                cbConfigList.SelectedIndex = this.configs.IndexOf(useConfig);
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            DefaultMapConfig config = (DefaultMapConfig)this.cbConfigList.SelectedItem;
            if (config != null)
            {
                this.BindToConfig(config);
                if (!Directory.Exists("maps"))
                {
                    Directory.CreateDirectory("maps");
                }
                FileHelper fh = new FileHelper();
                string configStr = JsonConvert.SerializeObject(config);
                fh.SaveFile(Path.Combine("maps", config.Name+".config"), configStr);

                this.device.SetJoyMap(this.manager,config);
                
                MessageBox.Show("配置切换成功!");
            }
            this.Close();
        }

        private void CbConfigList_SelectedIndexChanged(object sender, EventArgs e)
        {
            DefaultMapConfig config = (DefaultMapConfig)this.cbConfigList.SelectedItem;
            if (config != null)
            {
                if (config.MapType == MapType.XINPUT)
                {
                    SetXinputDefault();
                    BindXinput(config);
                }
                else if (config.MapType == MapType.KEYBOARD)
                {
                    SetKeyboardDefault();
                    BindKeyBoard(config);
                }
            }
        }

        private void SetXinputDefault()
        {
            ClearComboBox();
            cbLSUp.Enabled = false;
            cbLSDown.Enabled = false;
            cbLSLeft.Enabled = false;
            cbLSRight.Enabled = false;
            cbRSUp.Enabled = false;
            cbRSDown.Enabled = false;
            cbRSLeft.Enabled = false;
            cbRSRight.Enabled = false;
            cbLSX.Enabled = true;
            cbLSY.Enabled = true;
            cbRSX.Enabled = true;
            cbRSY.Enabled = true;
            var btList = GetEnumArray<Xbox360Buttons>().Select(i => (object)i).ToList();
            btList.Add((Object)TimeBoxButton.NONE);
            var bts = btList.ToArray();
            cbA.Items.AddRange(bts);
            cbB.Items.AddRange(bts);
            cbX.Items.AddRange(bts);
            cbY.Items.AddRange(bts);

            cbBack.Items.AddRange(bts);
            cbStart.Items.AddRange(bts);

            cbBackL.Items.AddRange(bts);
            cbBackR.Items.AddRange(bts);
            
            cbHelp.Items.AddRange(bts);
            cbHome.Items.AddRange(bts);

            cbLB.Items.AddRange(bts);
            cbRB.Items.AddRange(bts);

            cbUp.Items.AddRange(bts);
            cbDown.Items.AddRange(bts);
            cbLeft.Items.AddRange(bts);
            cbRight.Items.AddRange(bts);
            cbLSB.Items.AddRange(bts);
            cbRSB.Items.AddRange(bts);
            var axeList = GetEnumArray<Xbox360Axes>().Select(i => (object)i).ToList();
            axeList.Add(TimeBoxButton.NONE);
            var axes = axeList.ToArray();

            cbLT.Items.AddRange(axes);
            cbRT.Items.AddRange(axes);
            cbLSX.Items.AddRange(axes);
            cbLSY.Items.AddRange(axes);
            cbRSX.Items.AddRange(axes);
            cbRSY.Items.AddRange(axes);
        }
        private void SetKeyboardDefault()
        {
            ClearComboBox();
            cbLSUp.Enabled = true;
            cbLSDown.Enabled = true;
            cbLSLeft.Enabled = true;
            cbLSRight.Enabled = true;
            cbRSUp.Enabled = true;
            cbRSDown.Enabled = true;
            cbRSLeft.Enabled = true;
            cbRSRight.Enabled = true;
            cbLSX.Enabled = false;
            cbLSY.Enabled = false;
            cbRSX.Enabled = false;
            cbRSY.Enabled = false;

            var bts = GetEnumArray<Keys>().Select(i => (object)i).ToArray();
            cbA.Items.AddRange(bts);
            cbB.Items.AddRange(bts);
            cbX.Items.AddRange(bts);
            cbY.Items.AddRange(bts);

            cbBack.Items.AddRange(bts);
            cbStart.Items.AddRange(bts);

            cbBackL.Items.AddRange(bts);
            cbBackR.Items.AddRange(bts);

            cbHelp.Items.AddRange(bts);
            cbHome.Items.AddRange(bts);

            cbLB.Items.AddRange(bts);
            cbRB.Items.AddRange(bts);

            cbUp.Items.AddRange(bts);
            cbDown.Items.AddRange(bts);
            cbLeft.Items.AddRange(bts);
            cbRight.Items.AddRange(bts);
            cbLT.Items.AddRange(bts);
            cbRT.Items.AddRange(bts);
            cbLSUp.Items.AddRange(bts);
            cbLSDown.Items.AddRange(bts);
            cbLSLeft.Items.AddRange(bts);
            cbLSRight.Items.AddRange(bts);
            cbRSUp.Items.AddRange(bts);
            cbRSDown.Items.AddRange(bts);
            cbRSLeft.Items.AddRange(bts);
            cbRSRight.Items.AddRange(bts);

            cbLSB.Items.AddRange(bts);
            cbRSB.Items.AddRange(bts);
        }

        private void BindXinput(DefaultMapConfig config)
        {
            cbA.SelectedItem = (Xbox360Buttons)config.Keymap[(byte)TimeBoxButton.A];
            cbB.SelectedItem = (Xbox360Buttons)config.Keymap[(byte)TimeBoxButton.B];
            cbX.SelectedItem = (Xbox360Buttons)config.Keymap[(byte)TimeBoxButton.X];
            cbY.SelectedItem = (Xbox360Buttons)config.Keymap[(byte)TimeBoxButton.Y];

            cbBack.SelectedItem = (Xbox360Buttons)config.Keymap[(byte)TimeBoxButton.BACK];
            cbStart.SelectedItem = (Xbox360Buttons)config.Keymap[(byte)TimeBoxButton.START];

            cbBackL.SelectedItem = (Xbox360Buttons)config.Keymap[(byte)TimeBoxButton.LBACK];
            cbBackR.SelectedItem = (Xbox360Buttons)config.Keymap[(byte)TimeBoxButton.RBACK];

            cbHelp.SelectedItem = (Xbox360Buttons)config.Keymap[(byte)TimeBoxButton.HELP];
            cbHome.SelectedItem = (Xbox360Buttons)config.Keymap[(byte)TimeBoxButton.HOME];

            cbLB.SelectedItem = (Xbox360Buttons)config.Keymap[(byte)TimeBoxButton.LB];
            cbRB.SelectedItem = (Xbox360Buttons)config.Keymap[(byte)TimeBoxButton.RB];

            cbUp.SelectedItem = (Xbox360Buttons)config.Keymap[(byte)TimeBoxButton.UP];
            cbDown.SelectedItem = (Xbox360Buttons)config.Keymap[(byte)TimeBoxButton.DOWN];
            cbLeft.SelectedItem = (Xbox360Buttons)config.Keymap[(byte)TimeBoxButton.LEFT];
            cbRight.SelectedItem = (Xbox360Buttons)config.Keymap[(byte)TimeBoxButton.RIGHT];

            cbLT.SelectedItem = (Xbox360Axes)config.LTrigger;
            cbRT.SelectedItem = (Xbox360Axes)config.RTrigger;
            cbLSX.SelectedItem = (Xbox360Axes)config.LeftRemoteX;
            cbLSY.SelectedItem = (Xbox360Axes)config.LeftRemoteY;
            cbRSX.SelectedItem = (Xbox360Axes)config.RightRemoteX;
            cbRSY.SelectedItem = (Xbox360Axes)config.RightRemoteY;

            cbLSB.SelectedItem= (Xbox360Buttons)config.Keymap[(byte)TimeBoxButton.LSBUTTON];
            cbRSB.SelectedItem = (Xbox360Buttons)config.Keymap[(byte)TimeBoxButton.RSBUTTON];
        }
        private void BindKeyBoard(DefaultMapConfig config)
        {
            cbA.SelectedItem = (Keys)config.Keymap[(byte)TimeBoxButton.A];
            cbB.SelectedItem = (Keys)config.Keymap[(byte)TimeBoxButton.B];
            cbX.SelectedItem = (Keys)config.Keymap[(byte)TimeBoxButton.X];
            cbY.SelectedItem = (Keys)config.Keymap[(byte)TimeBoxButton.Y];

            cbBack.SelectedItem = (Keys)config.Keymap[(byte)TimeBoxButton.BACK];
            cbStart.SelectedItem = (Keys)config.Keymap[(byte)TimeBoxButton.START];

            cbBackL.SelectedItem = (Keys)config.Keymap[(byte)TimeBoxButton.LBACK];
            cbBackR.SelectedItem = (Keys)config.Keymap[(byte)TimeBoxButton.RBACK];

            cbHelp.SelectedItem = (Keys)config.Keymap[(byte)TimeBoxButton.HELP];
            cbHome.SelectedItem = (Keys)config.Keymap[(byte)TimeBoxButton.HOME];

            cbLB.SelectedItem = (Keys)config.Keymap[(byte)TimeBoxButton.LB];
            cbRB.SelectedItem = (Keys)config.Keymap[(byte)TimeBoxButton.RB];

            cbUp.SelectedItem = (Keys)config.Keymap[(byte)TimeBoxButton.UP];
            cbDown.SelectedItem = (Keys)config.Keymap[(byte)TimeBoxButton.DOWN];
            cbLeft.SelectedItem = (Keys)config.Keymap[(byte)TimeBoxButton.LEFT];
            cbRight.SelectedItem = (Keys)config.Keymap[(byte)TimeBoxButton.RIGHT];

            cbLT.SelectedItem = (Keys)config.LTrigger;
            cbRT.SelectedItem = (Keys)config.RTrigger;

            cbLSUp.SelectedItem = (Keys)config.LeftRemoteUp;
            cbLSDown.SelectedItem = (Keys)config.LeftRemoteDown;
            cbLSLeft.SelectedItem = (Keys)config.LeftRemoteLeft;
            cbLSRight.SelectedItem = (Keys)config.LeftRemoteRight;

            cbRSUp.SelectedItem = (Keys)config.RightRemoteUp;
            cbRSDown.SelectedItem = (Keys)config.RightRemoteDown;
            cbRSLeft.SelectedItem = (Keys)config.RightRemoteLeft;
            cbRSRight.SelectedItem = (Keys)config.RightRemoteRight;

            cbLSB.SelectedItem = (Keys)config.Keymap[(byte)TimeBoxButton.LSBUTTON];
            cbRSB.SelectedItem = (Keys)config.Keymap[(byte)TimeBoxButton.RSBUTTON];
        }

        /// <summary>
        /// 反向绑定当前设置到Config
        /// </summary>
        /// <param name="config"></param>
        private void BindToConfig(DefaultMapConfig config)
        {
            config.Keymap[(byte)TimeBoxButton.A] = Convert.ToInt32(cbA.SelectedItem);
            config.Keymap[(byte)TimeBoxButton.B] = Convert.ToInt32(cbB.SelectedItem);
            config.Keymap[(byte)TimeBoxButton.X] = Convert.ToInt32(cbX.SelectedItem);
            config.Keymap[(byte)TimeBoxButton.Y] = Convert.ToInt32(cbY.SelectedItem);
            config.Keymap[(byte)TimeBoxButton.BACK] = Convert.ToInt32(cbBack.SelectedItem);
            config.Keymap[(byte)TimeBoxButton.START] = Convert.ToInt32(cbStart.SelectedItem);
            config.Keymap[(byte)TimeBoxButton.LBACK] = Convert.ToInt32(cbBackL.SelectedItem);
            config.Keymap[(byte)TimeBoxButton.RBACK] = Convert.ToInt32(cbBackR.SelectedItem);

            config.Keymap[(byte)TimeBoxButton.HELP] = Convert.ToInt32(cbHelp.SelectedItem);
            config.Keymap[(byte)TimeBoxButton.HOME] = Convert.ToInt32(cbHome.SelectedItem);

            config.Keymap[(byte)TimeBoxButton.LB] = Convert.ToInt32(cbLB.SelectedItem);
            config.Keymap[(byte)TimeBoxButton.RB] = Convert.ToInt32(cbRB.SelectedItem);

            config.Keymap[(byte)TimeBoxButton.UP] = Convert.ToInt32(cbUp.SelectedItem);
            config.Keymap[(byte)TimeBoxButton.DOWN] = Convert.ToInt32(cbDown.SelectedItem);
            config.Keymap[(byte)TimeBoxButton.LEFT] = Convert.ToInt32(cbLeft.SelectedItem);
            config.Keymap[(byte)TimeBoxButton.RIGHT] = Convert.ToInt32(cbRight.SelectedItem);

            config.Keymap[(byte)TimeBoxButton.LSBUTTON] = Convert.ToInt32(cbLSB.SelectedItem);
            config.Keymap[(byte)TimeBoxButton.RSBUTTON] = Convert.ToInt32(cbRSB.SelectedItem);

            config.LeftRemoteX = Convert.ToInt32(cbLSX.SelectedItem);
            config.LeftRemoteY = Convert.ToInt32(cbLSY.SelectedItem);
            config.RightRemoteX = Convert.ToInt32(cbRSX.SelectedItem);
            config.RightRemoteY = Convert.ToInt32(cbRSY.SelectedItem);
            config.LTrigger= Convert.ToInt32(cbLT.SelectedItem);
            config.RTrigger = Convert.ToInt32(cbRT.SelectedItem);

            config.LeftRemoteUp= Convert.ToInt32(cbLSUp.SelectedItem);
            config.LeftRemoteDown = Convert.ToInt32(cbLSDown.SelectedItem);
            config.LeftRemoteLeft = Convert.ToInt32(cbLSLeft.SelectedItem);
            config.LeftRemoteRight = Convert.ToInt32(cbLSRight.SelectedItem);

            config.RightRemoteUp = Convert.ToInt32(cbRSUp.SelectedItem);
            config.RightRemoteDown = Convert.ToInt32(cbRSDown.SelectedItem);
            config.RightRemoteLeft = Convert.ToInt32(cbRSLeft.SelectedItem);
            config.RightRemoteRight = Convert.ToInt32(cbRSRight.SelectedItem);
        }
        private void ClearComboBox()
        {
            cbA.Items.Clear();
            cbB.Items.Clear();
            cbX.Items.Clear();
            cbY.Items.Clear();

            cbBack.Items.Clear();
            cbStart.Items.Clear();

            cbBackL.Items.Clear();
            cbBackR.Items.Clear();

            cbHelp.Items.Clear();
            cbHome.Items.Clear();

            cbLB.Items.Clear();
            cbRB.Items.Clear();

            cbUp.Items.Clear();
            cbDown.Items.Clear();
            cbLeft.Items.Clear();
            cbRight.Items.Clear();

            cbLT.Items.Clear();
            cbRT.Items.Clear();
            cbLSX.Items.Clear();
            cbLSY.Items.Clear();
            cbRSX.Items.Clear();
            cbRSY.Items.Clear();

            cbLSUp.Items.Clear();
            cbLSDown.Items.Clear();
            cbLSLeft.Items.Clear();
            cbLSRight.Items.Clear();
            cbRSUp.Items.Clear();
            cbRSDown.Items.Clear();
            cbRSLeft.Items.Clear();
            cbRSRight.Items.Clear();

            cbLSB.Items.Clear();
            cbRSB.Items.Clear();
        }
        private T[] GetEnumArray<T>()
        {
            List<T> r = new List<T>();
            foreach(var item in Enum.GetValues(typeof(T)))
            {
                r.Add((T)item);
            }
            return r.ToArray();
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            NewMap newmap = new NewMap(config=> {
                this.Invoke(new Action(()=> {
                    if (config != null)
                    {
                        this.configs.Add(config);
                        this.cbConfigList.Items.Add(config);
                        cbConfigList.SelectedIndex = this.configs.IndexOf(config);
                    }
                }));
            });
            newmap.ShowDialog();
        }
    }
}
