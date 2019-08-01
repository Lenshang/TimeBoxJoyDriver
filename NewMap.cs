using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TimeBoxJoy.MapConfig;

namespace TimeBoxJoy
{
    public partial class NewMap : Form
    {
        //DefaultMapConfig config { get; set; }
        Action<DefaultMapConfig> OnSuccess { get; set; }
        public NewMap(Action<DefaultMapConfig> _onSuccess)
        {
            InitializeComponent();
            //config = _config;
            OnSuccess = _onSuccess;
        }

        private void NewMap_Load(object sender, EventArgs e)
        {
            this.comboBox1.SelectedIndex = 0;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text)||
                !IsFileNameValid(textBox1.Text))
            {
                MessageBox.Show("不合法的名称！");
                return;
            }
            DefaultMapConfig config = null;
            if (this.comboBox1.SelectedIndex == 0)
            {
                config = new KeyBoardConfig();
            }
            else if (this.comboBox1.SelectedIndex == 1)
            {
                config = new XInputConfig();
            }
            else
            {
                config = new MixModeConfig();
            }
            config.Name = textBox1.Text;
            this.OnSuccess(config);
            this.Close();
        }
        private bool IsFileNameValid(string name)
        {
            bool isFilename = true;
            string[] errorStr = new string[] { "/", "\\", ":", ",", "*", "?", "\"", "<", ">", "|" };

            if (string.IsNullOrEmpty(name))
            {
                isFilename = false;
            }
            else
            {
                for (int i = 0; i < errorStr.Length; i++)
                {
                    if (name.Contains(errorStr[i]))
                    {
                        isFilename = false;
                        break;
                    }
                }
            }
            return isFilename;
        }
    }
}
