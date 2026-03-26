namespace WarehouseManagementSystem.Forms
{
    partial class FormStorekeeperMain
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
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.складToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.остаткиToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.документыToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.отгрузкиToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.новаяОтгрузкаToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnExit = new System.Windows.Forms.Button();
            this.panelContent = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.новаяПоставкаToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip.SuspendLayout();
            this.panelContent.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip
            // 
            this.menuStrip.BackColor = System.Drawing.Color.MediumAquamarine;
            this.menuStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.складToolStripMenuItem,
            this.документыToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(1082, 28);
            this.menuStrip.TabIndex = 0;
            this.menuStrip.Text = "menuStrip1";
            // 
            // складToolStripMenuItem
            // 
            this.складToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.остаткиToolStripMenuItem});
            this.складToolStripMenuItem.Name = "складToolStripMenuItem";
            this.складToolStripMenuItem.Size = new System.Drawing.Size(63, 24);
            this.складToolStripMenuItem.Text = "Склад";
            // 
            // остаткиToolStripMenuItem
            // 
            this.остаткиToolStripMenuItem.Name = "остаткиToolStripMenuItem";
            this.остаткиToolStripMenuItem.Size = new System.Drawing.Size(146, 26);
            this.остаткиToolStripMenuItem.Text = "Остатки";
            // 
            // документыToolStripMenuItem
            // 
            this.документыToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.отгрузкиToolStripMenuItem,
            this.новаяОтгрузкаToolStripMenuItem,
            this.новаяПоставкаToolStripMenuItem});
            this.документыToolStripMenuItem.Name = "документыToolStripMenuItem";
            this.документыToolStripMenuItem.Size = new System.Drawing.Size(101, 24);
            this.документыToolStripMenuItem.Text = "Документы";
            // 
            // отгрузкиToolStripMenuItem
            // 
            this.отгрузкиToolStripMenuItem.Name = "отгрузкиToolStripMenuItem";
            this.отгрузкиToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.отгрузкиToolStripMenuItem.Text = "Отгрузки";
            // 
            // новаяОтгрузкаToolStripMenuItem
            // 
            this.новаяОтгрузкаToolStripMenuItem.Name = "новаяОтгрузкаToolStripMenuItem";
            this.новаяОтгрузкаToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.новаяОтгрузкаToolStripMenuItem.Text = "Новая отгрузка";
            // 
            // btnExit
            // 
            this.btnExit.BackColor = System.Drawing.Color.MediumAquamarine;
            this.btnExit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExit.Location = new System.Drawing.Point(977, 0);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(75, 28);
            this.btnExit.TabIndex = 1;
            this.btnExit.Text = "ВЫХОД";
            this.btnExit.UseVisualStyleBackColor = false;
            // 
            // panelContent
            // 
            this.panelContent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.panelContent.Controls.Add(this.label1);
            this.panelContent.Location = new System.Drawing.Point(12, 68);
            this.panelContent.Name = "panelContent";
            this.panelContent.Size = new System.Drawing.Size(1058, 519);
            this.panelContent.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(420, 263);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(161, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Область дочерних окон";
            // 
            // новаяПоставкаToolStripMenuItem
            // 
            this.новаяПоставкаToolStripMenuItem.Name = "новаяПоставкаToolStripMenuItem";
            this.новаяПоставкаToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.новаяПоставкаToolStripMenuItem.Text = "Новая поставка";
            this.новаяПоставкаToolStripMenuItem.Click += new System.EventHandler(this.новаяПоставкаToolStripMenuItem_Click);
            // 
            // FormStorekeeperMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1082, 648);
            this.Controls.Add(this.panelContent);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.menuStrip);
            this.MainMenuStrip = this.menuStrip;
            this.Name = "FormStorekeeperMain";
            this.Load += new System.EventHandler(this.FormStorekeeperMain_Load);
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.panelContent.ResumeLayout(false);
            this.panelContent.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.Panel panelContent;
        private System.Windows.Forms.ToolStripMenuItem складToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem остаткиToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem документыToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem отгрузкиToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem новаяОтгрузкаToolStripMenuItem;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolStripMenuItem новаяПоставкаToolStripMenuItem;
    }
}