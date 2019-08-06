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
        List<Xbox360Buttons> allXinputBtEnum { get; set; }
        List<Xbox360Axes> allXinputAxesEnum { get; set; }
        List<Keys> allKeysEnum { get; set; }
        List<ButtonEnumModel> allMixBt { get; set; }
        List<ButtonEnumModel> allScripts { get; set; }
        public MapEdit(JoyManager _manager,List<DefaultMapConfig> _configs, BTDeviceInfo _device)
        {
            InitializeComponent();
            this.manager = _manager;
            this.configs = _configs;
            this.device = _device;
            allXinputBtEnum = GetEnumArray<Xbox360Buttons>();
            allXinputAxesEnum = GetEnumArray<Xbox360Axes>();
            allScripts = new List<ButtonEnumModel>();
            allKeysEnum = GetEnumArray<Keys>();
            allMixBt = new List<ButtonEnumModel>();
            allMixBt.AddRange(this.allXinputBtEnum.Select(i => new ButtonEnumModel("360:" + i.ToString(), (int)i, 65535)));
            allMixBt.AddRange(this.allKeysEnum.Select(i => new ButtonEnumModel(i.ToString(), (int)i)));
            allMixBt.Add(new ButtonEnumModel("未设定", (int)TimeBoxButton.NONE));

            if (!Directory.Exists("joyScripts"))
            {
                Directory.CreateDirectory("joyScripts");
            }
            foreach (var file in Directory.GetFiles("joyScripts"))
            {
                var name = System.IO.Path.GetFileNameWithoutExtension(file);
                var nameHash = name.GetHashCode();
                allScripts.Add(new ButtonEnumModel(name, nameHash));
            }
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
                else if (config.MapType == MapType.MIX)
                {
                    SetMixModeDefault();
                    BindMixMode(config);
                }
                else if (config.MapType == MapType.SCRIPT)
                {
                    SetScriptDefault();
                    BindScriptMode(config);
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
            var btList = allXinputBtEnum.Select(i => new ButtonEnumModel(i.ToString(),(int)i)).ToList();
            btList.Add(new ButtonEnumModel("未设定",(int)TimeBoxButton.NONE));
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
            var axeList = allXinputAxesEnum.Select(i => new ButtonEnumModel(i.ToString(), (int)i)).ToList();
            axeList.Add(new ButtonEnumModel("未设定", (int)TimeBoxButton.AxesNone));
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

            var bts = allKeysEnum.Select(i => new ButtonEnumModel(i.ToString(), (int)i)).ToArray();
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
        private void SetMixModeDefault()
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

            cbA.Items.AddRange(allMixBt.ToArray());
            cbB.Items.AddRange(allMixBt.ToArray());
            cbX.Items.AddRange(allMixBt.ToArray());
            cbY.Items.AddRange(allMixBt.ToArray());

            cbBack.Items.AddRange(allMixBt.ToArray());
            cbStart.Items.AddRange(allMixBt.ToArray());

            cbBackL.Items.AddRange(allMixBt.ToArray());
            cbBackR.Items.AddRange(allMixBt.ToArray());

            cbHelp.Items.AddRange(allMixBt.ToArray());
            cbHome.Items.AddRange(allMixBt.ToArray());

            cbLB.Items.AddRange(allMixBt.ToArray());
            cbRB.Items.AddRange(allMixBt.ToArray());

            cbUp.Items.AddRange(allMixBt.ToArray());
            cbDown.Items.AddRange(allMixBt.ToArray());
            cbLeft.Items.AddRange(allMixBt.ToArray());
            cbRight.Items.AddRange(allMixBt.ToArray());
            cbLSB.Items.AddRange(allMixBt.ToArray());
            cbRSB.Items.AddRange(allMixBt.ToArray());

            var axeList = allXinputAxesEnum.Select(i => new ButtonEnumModel("360:"+i.ToString(), (int)i,65535)).ToList();
            axeList.Add(new ButtonEnumModel("未设定", (int)TimeBoxButton.AxesNone));
            var axes = axeList.ToArray();

            cbLT.Items.AddRange(axes);
            cbRT.Items.AddRange(axes);
            cbLSX.Items.AddRange(axes);
            cbLSY.Items.AddRange(axes);
            cbRSX.Items.AddRange(axes);
            cbRSY.Items.AddRange(axes);
        }

        private void SetScriptDefault()
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

            //var bts = allKeysEnum.Select(i => new ButtonEnumModel(i.ToString(), (int)i)).ToArray();
            var bts = allScripts.ToArray();
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
        private void BindScriptMode(DefaultMapConfig config)
        {
            cbA.SelectedIndex = allScripts.FindIndex(i => i.ToInt() == config.Keymap[(byte)TimeBoxButton.A]);
            cbB.SelectedIndex = allScripts.FindIndex(i => i.ToInt() == config.Keymap[(byte)TimeBoxButton.B]);
            cbX.SelectedIndex = allScripts.FindIndex(i => i.ToInt() == config.Keymap[(byte)TimeBoxButton.X]);
            cbY.SelectedIndex = allScripts.FindIndex(i => i.ToInt() == config.Keymap[(byte)TimeBoxButton.Y]);

            cbBack.SelectedIndex = allScripts.FindIndex(i => i.ToInt() == config.Keymap[(byte)TimeBoxButton.BACK]);
            cbStart.SelectedIndex = allScripts.FindIndex(i => i.ToInt() == config.Keymap[(byte)TimeBoxButton.START]);

            cbBackL.SelectedIndex = allScripts.FindIndex(i => i.ToInt() == config.Keymap[(byte)TimeBoxButton.LBACK]);
            cbBackR.SelectedIndex = allScripts.FindIndex(i => i.ToInt() == config.Keymap[(byte)TimeBoxButton.RBACK]);

            cbHelp.SelectedIndex = allScripts.FindIndex(i => i.ToInt() == config.Keymap[(byte)TimeBoxButton.HELP]);
            cbHome.SelectedIndex = allScripts.FindIndex(i => i.ToInt() == config.Keymap[(byte)TimeBoxButton.HOME]);

            cbLB.SelectedIndex = allScripts.FindIndex(i => i.ToInt() == config.Keymap[(byte)TimeBoxButton.LB]);
            cbRB.SelectedIndex = allScripts.FindIndex(i => i.ToInt() == config.Keymap[(byte)TimeBoxButton.RB]);

            cbUp.SelectedIndex = allScripts.FindIndex(i => i.ToInt() == config.Keymap[(byte)TimeBoxButton.UP]);
            cbDown.SelectedIndex = allScripts.FindIndex(i => i.ToInt() == config.Keymap[(byte)TimeBoxButton.DOWN]);
            cbLeft.SelectedIndex = allScripts.FindIndex(i => i.ToInt() == config.Keymap[(byte)TimeBoxButton.LEFT]);
            cbRight.SelectedIndex = allScripts.FindIndex(i => i.ToInt() == config.Keymap[(byte)TimeBoxButton.RIGHT]);

            cbLT.SelectedIndex = allScripts.FindIndex(i => i.ToInt() == config.LTrigger);
            cbRT.SelectedIndex = allScripts.FindIndex(i => i.ToInt() == config.RTrigger);

            cbLSUp.SelectedIndex = allScripts.FindIndex(i => i.ToInt() == config.LeftRemoteUp);
            cbLSDown.SelectedIndex = allScripts.FindIndex(i => i.ToInt() == config.LeftRemoteDown);
            cbLSLeft.SelectedIndex = allScripts.FindIndex(i => i.ToInt() == config.LeftRemoteLeft);
            cbLSRight.SelectedIndex = allScripts.FindIndex(i => i.ToInt() == config.LeftRemoteRight);

            cbRSUp.SelectedIndex = allScripts.FindIndex(i => i.ToInt() == config.RightRemoteUp);
            cbRSDown.SelectedIndex = allScripts.FindIndex(i => i.ToInt() == config.RightRemoteDown);
            cbRSLeft.SelectedIndex = allScripts.FindIndex(i => i.ToInt() == config.RightRemoteLeft);
            cbRSRight.SelectedIndex = allScripts.FindIndex(i => i.ToInt() == config.RightRemoteRight);

            cbLSB.SelectedIndex = allScripts.FindIndex(i => i.ToInt() == config.Keymap[(byte)TimeBoxButton.LSBUTTON]);
            cbRSB.SelectedIndex = allScripts.FindIndex(i => i.ToInt() == config.Keymap[(byte)TimeBoxButton.RSBUTTON]);
        }
        private void BindXinput(DefaultMapConfig config)
        {
            cbA.SelectedIndex = this.allXinputBtEnum.IndexOf((Xbox360Buttons)config.Keymap[(byte)TimeBoxButton.A]);
            cbB.SelectedIndex = this.allXinputBtEnum.IndexOf((Xbox360Buttons)config.Keymap[(byte)TimeBoxButton.B]);
            cbX.SelectedIndex = this.allXinputBtEnum.IndexOf((Xbox360Buttons)config.Keymap[(byte)TimeBoxButton.X]);
            cbY.SelectedIndex = this.allXinputBtEnum.IndexOf((Xbox360Buttons)config.Keymap[(byte)TimeBoxButton.Y]);

            cbBack.SelectedIndex = this.allXinputBtEnum.IndexOf((Xbox360Buttons)config.Keymap[(byte)TimeBoxButton.BACK]);
            cbStart.SelectedIndex = this.allXinputBtEnum.IndexOf((Xbox360Buttons)config.Keymap[(byte)TimeBoxButton.START]);

            cbBackL.SelectedIndex = this.allXinputBtEnum.IndexOf((Xbox360Buttons)config.Keymap[(byte)TimeBoxButton.LBACK]);
            cbBackR.SelectedIndex = this.allXinputBtEnum.IndexOf((Xbox360Buttons)config.Keymap[(byte)TimeBoxButton.RBACK]);

            cbHelp.SelectedIndex = this.allXinputBtEnum.IndexOf((Xbox360Buttons)config.Keymap[(byte)TimeBoxButton.HELP]);
            cbHome.SelectedIndex = this.allXinputBtEnum.IndexOf((Xbox360Buttons)config.Keymap[(byte)TimeBoxButton.HOME]);

            cbLB.SelectedIndex = this.allXinputBtEnum.IndexOf((Xbox360Buttons)config.Keymap[(byte)TimeBoxButton.LB]);
            cbRB.SelectedIndex = this.allXinputBtEnum.IndexOf((Xbox360Buttons)config.Keymap[(byte)TimeBoxButton.RB]);

            cbUp.SelectedIndex = this.allXinputBtEnum.IndexOf((Xbox360Buttons)config.Keymap[(byte)TimeBoxButton.UP]);
            cbDown.SelectedIndex = this.allXinputBtEnum.IndexOf((Xbox360Buttons)config.Keymap[(byte)TimeBoxButton.DOWN]);
            cbLeft.SelectedIndex = this.allXinputBtEnum.IndexOf((Xbox360Buttons)config.Keymap[(byte)TimeBoxButton.LEFT]);
            cbRight.SelectedIndex = this.allXinputBtEnum.IndexOf((Xbox360Buttons)config.Keymap[(byte)TimeBoxButton.RIGHT]);

            cbLT.SelectedIndex = this.allXinputAxesEnum.IndexOf((Xbox360Axes)config.LTrigger);
            cbRT.SelectedIndex = this.allXinputAxesEnum.IndexOf((Xbox360Axes)config.RTrigger);
            cbLSX.SelectedIndex = this.allXinputAxesEnum.IndexOf((Xbox360Axes)config.LeftRemoteX);
            cbLSY.SelectedIndex = this.allXinputAxesEnum.IndexOf((Xbox360Axes)config.LeftRemoteY);
            cbRSX.SelectedIndex = this.allXinputAxesEnum.IndexOf((Xbox360Axes)config.RightRemoteX);
            cbRSY.SelectedIndex = this.allXinputAxesEnum.IndexOf((Xbox360Axes)config.RightRemoteY);

            cbLSB.SelectedIndex = this.allXinputBtEnum.IndexOf((Xbox360Buttons)config.Keymap[(byte)TimeBoxButton.LSBUTTON]);
            cbRSB.SelectedIndex = this.allXinputBtEnum.IndexOf((Xbox360Buttons)config.Keymap[(byte)TimeBoxButton.RSBUTTON]);
        }
        private void BindKeyBoard(DefaultMapConfig config)
        {
            cbA.SelectedIndex = this.allKeysEnum.IndexOf((Keys)config.Keymap[(byte)TimeBoxButton.A]);
            cbB.SelectedIndex = this.allKeysEnum.IndexOf((Keys)config.Keymap[(byte)TimeBoxButton.B]);
            cbX.SelectedIndex = this.allKeysEnum.IndexOf((Keys)config.Keymap[(byte)TimeBoxButton.X]);
            cbY.SelectedIndex = this.allKeysEnum.IndexOf((Keys)config.Keymap[(byte)TimeBoxButton.Y]);

            cbBack.SelectedIndex = this.allKeysEnum.IndexOf((Keys)config.Keymap[(byte)TimeBoxButton.BACK]);
            cbStart.SelectedIndex = this.allKeysEnum.IndexOf((Keys)config.Keymap[(byte)TimeBoxButton.START]);

            cbBackL.SelectedIndex = this.allKeysEnum.IndexOf((Keys)config.Keymap[(byte)TimeBoxButton.LBACK]);
            cbBackR.SelectedIndex = this.allKeysEnum.IndexOf((Keys)config.Keymap[(byte)TimeBoxButton.RBACK]);

            cbHelp.SelectedIndex = this.allKeysEnum.IndexOf((Keys)config.Keymap[(byte)TimeBoxButton.HELP]);
            cbHome.SelectedIndex = this.allKeysEnum.IndexOf((Keys)config.Keymap[(byte)TimeBoxButton.HOME]);

            cbLB.SelectedIndex = this.allKeysEnum.IndexOf((Keys)config.Keymap[(byte)TimeBoxButton.LB]);
            cbRB.SelectedIndex = this.allKeysEnum.IndexOf((Keys)config.Keymap[(byte)TimeBoxButton.RB]);

            cbUp.SelectedIndex = this.allKeysEnum.IndexOf((Keys)config.Keymap[(byte)TimeBoxButton.UP]);
            cbDown.SelectedIndex = this.allKeysEnum.IndexOf((Keys)config.Keymap[(byte)TimeBoxButton.DOWN]);
            cbLeft.SelectedIndex = this.allKeysEnum.IndexOf((Keys)config.Keymap[(byte)TimeBoxButton.LEFT]);
            cbRight.SelectedIndex = this.allKeysEnum.IndexOf((Keys)config.Keymap[(byte)TimeBoxButton.RIGHT]);

            cbLT.SelectedIndex = this.allKeysEnum.IndexOf((Keys)config.LTrigger);
            cbRT.SelectedIndex = this.allKeysEnum.IndexOf((Keys)config.RTrigger);

            cbLSUp.SelectedIndex = this.allKeysEnum.IndexOf((Keys)config.LeftRemoteUp);
            cbLSDown.SelectedIndex = this.allKeysEnum.IndexOf((Keys)config.LeftRemoteDown);
            cbLSLeft.SelectedIndex = this.allKeysEnum.IndexOf((Keys)config.LeftRemoteLeft);
            cbLSRight.SelectedIndex = this.allKeysEnum.IndexOf((Keys)config.LeftRemoteRight);

            cbRSUp.SelectedIndex = this.allKeysEnum.IndexOf((Keys)config.RightRemoteUp);
            cbRSDown.SelectedIndex = this.allKeysEnum.IndexOf((Keys)config.RightRemoteDown);
            cbRSLeft.SelectedIndex = this.allKeysEnum.IndexOf((Keys)config.RightRemoteLeft);
            cbRSRight.SelectedIndex = this.allKeysEnum.IndexOf((Keys)config.RightRemoteRight);

            cbLSB.SelectedIndex = this.allKeysEnum.IndexOf((Keys)config.Keymap[(byte)TimeBoxButton.LSBUTTON]);
            cbRSB.SelectedIndex = this.allKeysEnum.IndexOf((Keys)config.Keymap[(byte)TimeBoxButton.RSBUTTON]);
        }
        private void BindMixMode(DefaultMapConfig config)
        {
            cbA.SelectedIndex = this.allMixBt.FindIndex(i=>i.ToInt()== config.Keymap[(byte)TimeBoxButton.A]);
            cbB.SelectedIndex = this.allMixBt.FindIndex(i => i.ToInt() == config.Keymap[(byte)TimeBoxButton.B]);
            cbX.SelectedIndex = this.allMixBt.FindIndex(i => i.ToInt() == config.Keymap[(byte)TimeBoxButton.X]);
            cbY.SelectedIndex = this.allMixBt.FindIndex(i => i.ToInt() == config.Keymap[(byte)TimeBoxButton.Y]);

            cbBack.SelectedIndex = this.allMixBt.FindIndex(i => i.ToInt() == config.Keymap[(byte)TimeBoxButton.BACK]);
            cbStart.SelectedIndex = this.allMixBt.FindIndex(i => i.ToInt() == config.Keymap[(byte)TimeBoxButton.START]);

            cbBackL.SelectedIndex = this.allMixBt.FindIndex(i => i.ToInt() == config.Keymap[(byte)TimeBoxButton.LBACK]);
            cbBackR.SelectedIndex = this.allMixBt.FindIndex(i => i.ToInt() == config.Keymap[(byte)TimeBoxButton.RBACK]);

            cbHelp.SelectedIndex = this.allMixBt.FindIndex(i => i.ToInt() == config.Keymap[(byte)TimeBoxButton.HELP]);
            cbHome.SelectedIndex = this.allMixBt.FindIndex(i => i.ToInt() == config.Keymap[(byte)TimeBoxButton.HOME]);

            cbLB.SelectedIndex = this.allMixBt.FindIndex(i => i.ToInt() == config.Keymap[(byte)TimeBoxButton.LB]);
            cbRB.SelectedIndex = this.allMixBt.FindIndex(i => i.ToInt() == config.Keymap[(byte)TimeBoxButton.RB]);

            cbUp.SelectedIndex = this.allMixBt.FindIndex(i => i.ToInt() == config.Keymap[(byte)TimeBoxButton.UP]);
            cbDown.SelectedIndex = this.allMixBt.FindIndex(i => i.ToInt() == config.Keymap[(byte)TimeBoxButton.DOWN]);
            cbLeft.SelectedIndex = this.allMixBt.FindIndex(i => i.ToInt() == config.Keymap[(byte)TimeBoxButton.LEFT]);
            cbRight.SelectedIndex = this.allMixBt.FindIndex(i => i.ToInt() == config.Keymap[(byte)TimeBoxButton.RIGHT]);

            cbLT.SelectedIndex = this.allXinputAxesEnum.IndexOf((Xbox360Axes)(config.LTrigger-65535));
            cbRT.SelectedIndex = this.allXinputAxesEnum.IndexOf((Xbox360Axes)(config.RTrigger - 65535));
            cbLSX.SelectedIndex = this.allXinputAxesEnum.IndexOf((Xbox360Axes)(config.LeftRemoteX - 65535));
            cbLSY.SelectedIndex = this.allXinputAxesEnum.IndexOf((Xbox360Axes)(config.LeftRemoteY - 65535));
            cbRSX.SelectedIndex = this.allXinputAxesEnum.IndexOf((Xbox360Axes)(config.RightRemoteX - 65535));
            cbRSY.SelectedIndex = this.allXinputAxesEnum.IndexOf((Xbox360Axes)(config.RightRemoteY - 65535));

            cbLSB.SelectedIndex = this.allMixBt.FindIndex(i => i.ToInt() == config.Keymap[(byte)TimeBoxButton.LSBUTTON]);
            cbRSB.SelectedIndex = this.allMixBt.FindIndex(i => i.ToInt() == config.Keymap[(byte)TimeBoxButton.RSBUTTON]);
        }
        /// <summary>
        /// 反向绑定当前设置到Config
        /// </summary>
        /// <param name="config"></param>
        private void BindToConfig(DefaultMapConfig config)
        {
            config.Keymap[(byte)TimeBoxButton.A] = GetKeyValue(cbA.SelectedItem);
            config.Keymap[(byte)TimeBoxButton.B] = GetKeyValue(cbB.SelectedItem);
            config.Keymap[(byte)TimeBoxButton.X] = GetKeyValue(cbX.SelectedItem);
            config.Keymap[(byte)TimeBoxButton.Y] = GetKeyValue(cbY.SelectedItem);
            config.Keymap[(byte)TimeBoxButton.BACK] = GetKeyValue(cbBack.SelectedItem);
            config.Keymap[(byte)TimeBoxButton.START] = GetKeyValue(cbStart.SelectedItem);
            config.Keymap[(byte)TimeBoxButton.LBACK] = GetKeyValue(cbBackL.SelectedItem);
            config.Keymap[(byte)TimeBoxButton.RBACK] = GetKeyValue(cbBackR.SelectedItem);

            config.Keymap[(byte)TimeBoxButton.HELP] = GetKeyValue(cbHelp.SelectedItem);
            config.Keymap[(byte)TimeBoxButton.HOME] = GetKeyValue(cbHome.SelectedItem);

            config.Keymap[(byte)TimeBoxButton.LB] = GetKeyValue(cbLB.SelectedItem);
            config.Keymap[(byte)TimeBoxButton.RB] = GetKeyValue(cbRB.SelectedItem);

            config.Keymap[(byte)TimeBoxButton.UP] = GetKeyValue(cbUp.SelectedItem);
            config.Keymap[(byte)TimeBoxButton.DOWN] = GetKeyValue(cbDown.SelectedItem);
            config.Keymap[(byte)TimeBoxButton.LEFT] = GetKeyValue(cbLeft.SelectedItem);
            config.Keymap[(byte)TimeBoxButton.RIGHT] = GetKeyValue(cbRight.SelectedItem);

            config.Keymap[(byte)TimeBoxButton.LSBUTTON] = GetKeyValue(cbLSB.SelectedItem);
            config.Keymap[(byte)TimeBoxButton.RSBUTTON] = GetKeyValue(cbRSB.SelectedItem);

            config.LeftRemoteX = cbLSX.SelectedIndex==-1?-1: GetKeyValue(cbLSX.SelectedItem);
            config.LeftRemoteY = cbLSY.SelectedIndex == -1 ? -1 : GetKeyValue(cbLSY.SelectedItem);
            config.RightRemoteX = cbRSX.SelectedIndex == -1 ? -1 : GetKeyValue(cbRSX.SelectedItem);
            config.RightRemoteY = cbRSY.SelectedIndex == -1 ? -1 : GetKeyValue(cbRSY.SelectedItem);
            config.LTrigger= cbLT.SelectedIndex == -1 ? -1 : GetKeyValue(cbLT.SelectedItem);
            config.RTrigger = cbRT.SelectedIndex == -1 ? -1 : GetKeyValue(cbRT.SelectedItem);

            config.LeftRemoteUp= GetKeyValue(cbLSUp.SelectedItem);
            config.LeftRemoteDown = GetKeyValue(cbLSDown.SelectedItem);
            config.LeftRemoteLeft = GetKeyValue(cbLSLeft.SelectedItem);
            config.LeftRemoteRight = GetKeyValue(cbLSRight.SelectedItem);

            config.RightRemoteUp = GetKeyValue(cbRSUp.SelectedItem);
            config.RightRemoteDown = GetKeyValue(cbRSDown.SelectedItem);
            config.RightRemoteLeft = GetKeyValue(cbRSLeft.SelectedItem);
            config.RightRemoteRight = GetKeyValue(cbRSRight.SelectedItem);
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
        private List<T> GetEnumArray<T>()
        {
            List<T> r = new List<T>();
            foreach(var item in Enum.GetValues(typeof(T)))
            {
                r.Add((T)item);
            }
            return r;
        }
        private int GetKeyValue(object item)
        {
            if(item!=null)
                return ((ButtonEnumModel)item).ToInt();
            return 0;
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
