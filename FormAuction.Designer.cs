namespace NP_Shop_Wizardry
{
    partial class FormAuction
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormAuction));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.listBoxAuctions = new System.Windows.Forms.ListBox();
            this.buttonRemoveSelectedAuctions = new System.Windows.Forms.Button();
            this.dataBox = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.buttonStart = new System.Windows.Forms.Button();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonSave = new System.Windows.Forms.Button();
            this.buttonAddAuction = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.listBoxAuctions);
            this.splitContainer1.Panel1.Controls.Add(this.buttonRemoveSelectedAuctions);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.dataBox);
            this.splitContainer1.Panel2.Controls.Add(this.panel1);
            this.splitContainer1.Panel2.Controls.Add(this.textBox3);
            this.splitContainer1.Panel2.Controls.Add(this.textBox2);
            this.splitContainer1.Panel2.Controls.Add(this.textBox1);
            this.splitContainer1.Panel2.Controls.Add(this.label3);
            this.splitContainer1.Panel2.Controls.Add(this.label2);
            this.splitContainer1.Panel2.Controls.Add(this.label1);
            this.splitContainer1.Panel2.Controls.Add(this.buttonSave);
            this.splitContainer1.Panel2.Controls.Add(this.buttonAddAuction);
            this.splitContainer1.Size = new System.Drawing.Size(537, 260);
            this.splitContainer1.SplitterDistance = 177;
            this.splitContainer1.TabIndex = 0;
            // 
            // listBoxAuctions
            // 
            this.listBoxAuctions.Dock = System.Windows.Forms.DockStyle.Top;
            this.listBoxAuctions.FormattingEnabled = true;
            this.listBoxAuctions.Location = new System.Drawing.Point(0, 0);
            this.listBoxAuctions.Name = "listBoxAuctions";
            this.listBoxAuctions.Size = new System.Drawing.Size(177, 160);
            this.listBoxAuctions.TabIndex = 2;
            // 
            // buttonRemoveSelectedAuctions
            // 
            this.buttonRemoveSelectedAuctions.Location = new System.Drawing.Point(12, 166);
            this.buttonRemoveSelectedAuctions.Name = "buttonRemoveSelectedAuctions";
            this.buttonRemoveSelectedAuctions.Size = new System.Drawing.Size(128, 23);
            this.buttonRemoveSelectedAuctions.TabIndex = 1;
            this.buttonRemoveSelectedAuctions.Text = "Remove Selected";
            this.buttonRemoveSelectedAuctions.UseVisualStyleBackColor = true;
            this.buttonRemoveSelectedAuctions.Click += new System.EventHandler(this.buttonRemoveSelectedAuctions_Click);
            // 
            // dataBox
            // 
            this.dataBox.Location = new System.Drawing.Point(16, 207);
            this.dataBox.Multiline = true;
            this.dataBox.Name = "dataBox";
            this.dataBox.Size = new System.Drawing.Size(261, 41);
            this.dataBox.TabIndex = 10;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.PaleGreen;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.buttonStart);
            this.panel1.Location = new System.Drawing.Point(169, 139);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(108, 50);
            this.panel1.TabIndex = 9;
            // 
            // buttonStart
            // 
            this.buttonStart.Location = new System.Drawing.Point(15, 13);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(75, 23);
            this.buttonStart.TabIndex = 8;
            this.buttonStart.Text = "Start!";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(96, 71);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(100, 20);
            this.textBox3.TabIndex = 7;
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(96, 48);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(100, 20);
            this.textBox2.TabIndex = 6;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(96, 24);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(181, 20);
            this.textBox1.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 74);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(80, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Min. Increment:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 51);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Start Price:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(61, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Item Name:";
            // 
            // buttonSave
            // 
            this.buttonSave.Location = new System.Drawing.Point(16, 166);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(129, 23);
            this.buttonSave.TabIndex = 1;
            this.buttonSave.Text = "Save Auctions";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // buttonAddAuction
            // 
            this.buttonAddAuction.Location = new System.Drawing.Point(69, 106);
            this.buttonAddAuction.Name = "buttonAddAuction";
            this.buttonAddAuction.Size = new System.Drawing.Size(75, 23);
            this.buttonAddAuction.TabIndex = 0;
            this.buttonAddAuction.Text = "Add Item Auction";
            this.buttonAddAuction.UseVisualStyleBackColor = true;
            this.buttonAddAuction.Click += new System.EventHandler(this.buttonAddAuction_Click);
            // 
            // FormAuction
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(537, 260);
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormAuction";
            this.Text = "Auctions";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button buttonRemoveSelectedAuctions;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.Button buttonAddAuction;
        private System.Windows.Forms.ListBox listBoxAuctions;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.TextBox dataBox;
    }
}