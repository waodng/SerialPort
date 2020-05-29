namespace SerialPortWgd
{
    partial class numTextBox
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.vScrollBar1 = new System.Windows.Forms.VScrollBar();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.txtRow = new System.Windows.Forms.RichTextBox();
            this.txtContent = new SerialPortWgd.wRichText();
            this.SuspendLayout();
            // 
            // vScrollBar1
            // 
            this.vScrollBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.vScrollBar1.LargeChange = 13;
            this.vScrollBar1.Location = new System.Drawing.Point(478, 0);
            this.vScrollBar1.Maximum = 15;
            this.vScrollBar1.Name = "vScrollBar1";
            this.vScrollBar1.Size = new System.Drawing.Size(17, 318);
            this.vScrollBar1.TabIndex = 10;
            this.vScrollBar1.Visible = false;
            this.vScrollBar1.ValueChanged += new System.EventHandler(this.vScrollBar1_ValueChanged);
            // 
            // timer1
            // 
            this.timer1.Interval = 30;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // txtRow
            // 
            this.txtRow.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.txtRow.BackColor = System.Drawing.Color.LightGray;
            this.txtRow.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtRow.Cursor = System.Windows.Forms.Cursors.Default;
            this.txtRow.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtRow.Location = new System.Drawing.Point(0, 0);
            this.txtRow.Name = "txtRow";
            this.txtRow.ReadOnly = true;
            this.txtRow.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.txtRow.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.txtRow.Size = new System.Drawing.Size(28, 319);
            this.txtRow.TabIndex = 13;
            this.txtRow.Text = "1\n12\n123\n4";
            this.txtRow.SizeChanged += new System.EventHandler(this.txtRow_SizeChanged);
            this.txtRow.TextChanged += new System.EventHandler(this.txtRow_TextChanged);
            this.txtRow.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.txtRow_MouseDoubleClick);
            // 
            // txtContent
            // 
            this.txtContent.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtContent.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtContent.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtContent.HideSelection = false;
            this.txtContent.Location = new System.Drawing.Point(28, 1);
            this.txtContent.Name = "txtContent";
            this.txtContent.Size = new System.Drawing.Size(466, 318);
            this.txtContent.TabIndex = 12;
            this.txtContent.Text = "";
            this.txtContent.SizeChanged += new System.EventHandler(this.txtContent_SizeChanged);
            this.txtContent.TextChanged += new System.EventHandler(this.txtContect_TextChanged);
            this.txtContent.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtContent_KeyDown);
            this.txtContent.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtContent_KeyUp);
            this.txtContent.MouseDown += new System.Windows.Forms.MouseEventHandler(this.txtContent_MouseDown);
            // 
            // numTextBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.txtRow);
            this.Controls.Add(this.vScrollBar1);
            this.Controls.Add(this.txtContent);
            this.Name = "numTextBox";
            this.Size = new System.Drawing.Size(495, 319);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.VScrollBar vScrollBar1;
        private System.Windows.Forms.Timer timer1;
        private wRichText txtContent;
        private System.Windows.Forms.RichTextBox txtRow;
    }
}
