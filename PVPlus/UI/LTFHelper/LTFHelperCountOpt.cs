﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PVPlus.UI.LTFHelper
{
    public partial class LTFHelperCountOpt : Form
    {
        public LTFHelperCountOpt()
        {
            InitializeComponent();
            textBox1.Text = "a1";
        }

        public string GetExpression()
        {
            return textBox1.Text;
        }
    }
}
