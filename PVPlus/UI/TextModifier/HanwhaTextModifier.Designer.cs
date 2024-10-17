namespace PVPlus.UI.TextModifier
{
    partial class HanwhaTextModifier
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.button1 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.PVPLUSExepnseTextBox = new System.Windows.Forms.TextBox();
            this.HanwhaExpenseTextBox = new System.Windows.Forms.TextBox();
            this.JongtextBox = new System.Windows.Forms.TextBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.prdCodeTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.errorTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.labelProgress = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.SystemColors.HighlightText;
            this.button1.Location = new System.Drawing.Point(662, 7);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(100, 30);
            this.button1.TabIndex = 4;
            this.button1.Text = "Convert";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(69, 12);
            this.label1.TabIndex = 5;
            this.label1.Text = "한화 사업비";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 194);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(92, 12);
            this.label2.TabIndex = 6;
            this.label2.Text = "PVPLUS 사업비";
            // 
            // PVPLUSExepnseTextBox
            // 
            this.PVPLUSExepnseTextBox.Location = new System.Drawing.Point(12, 213);
            this.PVPLUSExepnseTextBox.MaxLength = 1000000000;
            this.PVPLUSExepnseTextBox.Multiline = true;
            this.PVPLUSExepnseTextBox.Name = "PVPLUSExepnseTextBox";
            this.PVPLUSExepnseTextBox.ReadOnly = true;
            this.PVPLUSExepnseTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.PVPLUSExepnseTextBox.Size = new System.Drawing.Size(750, 140);
            this.PVPLUSExepnseTextBox.TabIndex = 3;
            this.PVPLUSExepnseTextBox.WordWrap = false;
            // 
            // HanwhaExpenseTextBox
            // 
            this.HanwhaExpenseTextBox.Location = new System.Drawing.Point(12, 44);
            this.HanwhaExpenseTextBox.MaxLength = 1000000000;
            this.HanwhaExpenseTextBox.Multiline = true;
            this.HanwhaExpenseTextBox.Name = "HanwhaExpenseTextBox";
            this.HanwhaExpenseTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.HanwhaExpenseTextBox.Size = new System.Drawing.Size(750, 140);
            this.HanwhaExpenseTextBox.TabIndex = 2;
            this.HanwhaExpenseTextBox.WordWrap = false;
            // 
            // JongtextBox
            // 
            this.JongtextBox.Location = new System.Drawing.Point(14, 381);
            this.JongtextBox.MaxLength = 1000000000;
            this.JongtextBox.Multiline = true;
            this.JongtextBox.Name = "JongtextBox";
            this.JongtextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.JongtextBox.Size = new System.Drawing.Size(400, 100);
            this.JongtextBox.TabIndex = 7;
            this.JongtextBox.WordWrap = false;
            this.JongtextBox.TextChanged += new System.EventHandler(this.JongtextBox_TextChanged);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(14, 360);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(128, 16);
            this.checkBox1.TabIndex = 9;
            this.checkBox1.Text = "세만기 종코드 조건";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // prdCodeTextBox
            // 
            this.prdCodeTextBox.Location = new System.Drawing.Point(540, 12);
            this.prdCodeTextBox.Name = "prdCodeTextBox";
            this.prdCodeTextBox.Size = new System.Drawing.Size(116, 21);
            this.prdCodeTextBox.TabIndex = 10;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(481, 16);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 11;
            this.label3.Text = "상품코드";
            // 
            // errorTextBox
            // 
            this.errorTextBox.Location = new System.Drawing.Point(428, 381);
            this.errorTextBox.MaxLength = 1000000000;
            this.errorTextBox.Multiline = true;
            this.errorTextBox.Name = "errorTextBox";
            this.errorTextBox.ReadOnly = true;
            this.errorTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.errorTextBox.Size = new System.Drawing.Size(334, 100);
            this.errorTextBox.TabIndex = 12;
            this.errorTextBox.WordWrap = false;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(426, 361);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(31, 12);
            this.label4.TabIndex = 13;
            this.label4.Text = "error";
            // 
            // labelProgress
            // 
            this.labelProgress.AutoSize = true;
            this.labelProgress.Location = new System.Drawing.Point(219, 15);
            this.labelProgress.Name = "labelProgress";
            this.labelProgress.Size = new System.Drawing.Size(64, 12);
            this.labelProgress.TabIndex = 14;
            this.labelProgress.Text = "Progress: ";
            // 
            // HanwhaTextModifier
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(840, 529);
            this.Controls.Add(this.labelProgress);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.errorTextBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.prdCodeTextBox);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.JongtextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.PVPLUSExepnseTextBox);
            this.Controls.Add(this.HanwhaExpenseTextBox);
            this.Name = "HanwhaTextModifier";
            this.Text = "HanwhaTextModifier";
            this.Load += new System.EventHandler(this.HanwhaTextModifier_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox PVPLUSExepnseTextBox;
        private System.Windows.Forms.TextBox HanwhaExpenseTextBox;
        private System.Windows.Forms.TextBox JongtextBox;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.TextBox prdCodeTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox errorTextBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label labelProgress;
    }
}