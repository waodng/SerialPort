namespace SerialPortWgd
{
    partial class NumRichTextBox
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.richTextData = new System.Windows.Forms.RichTextBox();
            this.lblNum = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.lblNum);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.richTextData);
            this.splitContainer1.Size = new System.Drawing.Size(405, 267);
            this.splitContainer1.SplitterDistance = 35;
            this.splitContainer1.SplitterWidth = 1;
            this.splitContainer1.TabIndex = 0;
            // 
            // richTextData
            // 
            this.richTextData.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.richTextData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextData.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(200)));
            this.richTextData.Location = new System.Drawing.Point(0, 0);
            this.richTextData.Name = "richTextData";
            this.richTextData.Size = new System.Drawing.Size(369, 267);
            this.richTextData.TabIndex = 0;
            this.richTextData.Text = "";
            this.richTextData.VScroll += new System.EventHandler(this.richTextData_VScroll);
            this.richTextData.FontChanged += new System.EventHandler(this.richTextData_FontChanged);
            this.richTextData.TextChanged += new System.EventHandler(this.richTextData_TextChanged);
            this.richTextData.Resize += new System.EventHandler(this.richTextData_Resize);
            // 
            // lblNum
            // 
            this.lblNum.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblNum.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.lblNum.Location = new System.Drawing.Point(0, 0);
            this.lblNum.Name = "lblNum";
            this.lblNum.Size = new System.Drawing.Size(35, 267);
            this.lblNum.TabIndex = 1;
            this.lblNum.Text = "1\r\n12\r\n123\r\n1234\r\n5\r\n6\r\n7\r\n8\r\n9\r\n10\r\n11\r\n12\r\n13\r\n14\r\n15\r\n16";
            this.lblNum.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // NumRichTextBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "NumRichTextBox";
            this.Size = new System.Drawing.Size(405, 267);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Label lblNum;
        private System.Windows.Forms.RichTextBox richTextData;
    }
}
