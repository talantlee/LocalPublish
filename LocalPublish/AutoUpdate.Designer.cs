namespace LocalPublish
{
    partial class AutoUpdate
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AutoUpdate));
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.lbl_updateinfo = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.btn_updated = new System.Windows.Forms.Button();
            this.lbl_currvesion = new System.Windows.Forms.Label();
            this.lbl_newVersion = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // progressBar1
            // 
            this.progressBar1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.progressBar1.Location = new System.Drawing.Point(0, 345);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(688, 13);
            this.progressBar1.TabIndex = 0;
            // 
            // lbl_updateinfo
            // 
            this.lbl_updateinfo.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lbl_updateinfo.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.lbl_updateinfo, 3);
            this.lbl_updateinfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbl_updateinfo.Location = new System.Drawing.Point(3, 6);
            this.lbl_updateinfo.Name = "lbl_updateinfo";
            this.lbl_updateinfo.Size = new System.Drawing.Size(181, 20);
            this.lbl_updateinfo.TabIndex = 1;
            this.lbl_updateinfo.Text = "点击“更新”按钮开始更新";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(12, 19);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(73, 20);
            this.label2.TabIndex = 1;
            this.label2.Text = "当前版本";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.Location = new System.Drawing.Point(345, 19);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(73, 20);
            this.label3.TabIndex = 1;
            this.label3.Text = "最新版本";
            // 
            // btn_updated
            // 
            this.btn_updated.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_updated.Location = new System.Drawing.Point(623, 3);
            this.btn_updated.Name = "btn_updated";
            this.btn_updated.Size = new System.Drawing.Size(62, 27);
            this.btn_updated.TabIndex = 2;
            this.btn_updated.Text = "更新";
            this.btn_updated.UseVisualStyleBackColor = true;
            this.btn_updated.Click += new System.EventHandler(this.btn_updated_Click);
            // 
            // lbl_currvesion
            // 
            this.lbl_currvesion.AutoSize = true;
            this.lbl_currvesion.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbl_currvesion.ForeColor = System.Drawing.Color.DarkRed;
            this.lbl_currvesion.Location = new System.Drawing.Point(91, 19);
            this.lbl_currvesion.Name = "lbl_currvesion";
            this.lbl_currvesion.Size = new System.Drawing.Size(31, 20);
            this.lbl_currvesion.TabIndex = 4;
            this.lbl_currvesion.Text = "1.0";
            // 
            // lbl_newVersion
            // 
            this.lbl_newVersion.AutoSize = true;
            this.lbl_newVersion.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbl_newVersion.ForeColor = System.Drawing.Color.DarkRed;
            this.lbl_newVersion.Location = new System.Drawing.Point(439, 19);
            this.lbl_newVersion.Name = "lbl_newVersion";
            this.lbl_newVersion.Size = new System.Drawing.Size(237, 20);
            this.lbl_newVersion.TabIndex = 4;
            this.lbl_newVersion.Text = "获取不到版本,请通过主程序更新";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(688, 358);
            this.pictureBox1.TabIndex = 7;
            this.pictureBox1.TabStop = false;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 44.51774F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 34.40609F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 21.07617F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 104F));
            this.tableLayoutPanel1.Controls.Add(this.lbl_updateinfo, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.btn_updated, 3, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 312);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 33F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 8F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(688, 33);
            this.tableLayoutPanel1.TabIndex = 5;
            // 
            // AutoUpdate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(688, 358);
            this.Controls.Add(this.lbl_newVersion);
            this.Controls.Add(this.lbl_currvesion);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.pictureBox1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AutoUpdate";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "AutoUpdate";
            this.Load += new System.EventHandler(this.AutoUpdate_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label lbl_updateinfo;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btn_updated;
        private System.Windows.Forms.Label lbl_currvesion;
        private System.Windows.Forms.Label lbl_newVersion;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}

