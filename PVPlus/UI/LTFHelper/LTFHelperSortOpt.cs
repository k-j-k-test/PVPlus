﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PVPlus.UI
{
    public partial class LTFHelperSortOpt : Form
    {
        public LTFHelperSortOpt()
        {
            InitializeComponent();
            textBox1.Text = "a1";
        }

        public string[] GetExpressions()
        {
            return new string[] { textBox1.Text, textBox2.Text, textBox3.Text, textBox4.Text };
        }

        private void LTFHelperSortOpt_Load(object sender, EventArgs e)
        {

        }
    }
}
