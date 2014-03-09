using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NP_Shop_Wizardry
{
    public partial class CookiesForm : Form
    {
        public CookiesForm(string cookies)
        {
            InitializeComponent();
            textBox1.Text = cookies;
        }
    }
}
