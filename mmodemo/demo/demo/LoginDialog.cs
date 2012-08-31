using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace demo
{
    public partial class LoginDialog : Form
    {
        public LoginDialog()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            bool valid = true;
            if (textUserName.Text == "")
            {
                toolTip1.Show("用户名不能为空", textUserName, 1000);
                valid = false;
            }
            else
            {
                Match m = Regex.Match(textUserName.Text, "[A-Za-z0-9]+$");
                if (m.Length == 0)
                {
                    toolTip1.Show("用户名包含非法字符", textUserName, 1000);
                    valid = false;
                }
            }
            if (textPassword.Text == "")
            {
                toolTip2.Show("密码不能为空", textPassword, 1000);
                valid = false;
            }
            if (valid)
            {
                MainGame.LoginToServer(textUserName.Text, textPassword.Text);
            }
        }

        private void LoginDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
          
        }
    }
}
