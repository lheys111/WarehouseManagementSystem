namespace WarehouseManagementSystem.Forms
{
    partial class FormAdminMain
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem справочникиToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem товарыToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem категорииToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem администрированиеToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem историяОтгрузокToolStripMenuItem;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.Panel panelContent;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.справочникиToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.товарыToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.категорииToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.администрированиеToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.историяОтгрузокToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.отчетПоОтгрузкамToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.списаниеПросрочкиToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.сменитьРольToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnExit = new System.Windows.Forms.Button();
            this.panelContent = new System.Windows.Forms.Panel();
            this.настройкиToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip
            // 
            this.menuStrip.BackColor = System.Drawing.Color.MediumAquamarine;
            this.menuStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip.ImeMode = System.Windows.Forms.ImeMode.Hiragana;
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.справочникиToolStripMenuItem,
            this.администрированиеToolStripMenuItem,
            this.сменитьРольToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Padding = new System.Windows.Forms.Padding(6, 2, 0, 2);
            this.menuStrip.Size = new System.Drawing.Size(1349, 28);
            this.menuStrip.TabIndex = 0;
            this.menuStrip.Text = "menuStrip";
            // 
            // справочникиToolStripMenuItem
            // 
            this.справочникиToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.товарыToolStripMenuItem,
            this.категорииToolStripMenuItem});
            this.справочникиToolStripMenuItem.Name = "справочникиToolStripMenuItem";
            this.справочникиToolStripMenuItem.Size = new System.Drawing.Size(117, 24);
            this.справочникиToolStripMenuItem.Text = "Справочники";
            // 
            // товарыToolStripMenuItem
            // 
            this.товарыToolStripMenuItem.Name = "товарыToolStripMenuItem";
            this.товарыToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.товарыToolStripMenuItem.Text = "Товары";
            this.товарыToolStripMenuItem.Click += new System.EventHandler(this.товарыToolStripMenuItem_Click);
            // 
            // категорииToolStripMenuItem
            // 
            this.категорииToolStripMenuItem.Name = "категорииToolStripMenuItem";
            this.категорииToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.категорииToolStripMenuItem.Text = "Категории";
            this.категорииToolStripMenuItem.Click += new System.EventHandler(this.категорииToolStripMenuItem_Click);
            // 
            // администрированиеToolStripMenuItem
            // 
            this.администрированиеToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.историяОтгрузокToolStripMenuItem,
            this.отчетПоОтгрузкамToolStripMenuItem,
            this.списаниеПросрочкиToolStripMenuItem,
            this.настройкиToolStripMenuItem});
            this.администрированиеToolStripMenuItem.Name = "администрированиеToolStripMenuItem";
            this.администрированиеToolStripMenuItem.Size = new System.Drawing.Size(170, 24);
            this.администрированиеToolStripMenuItem.Text = "Администрирование";
            // 
            // историяОтгрузокToolStripMenuItem
            // 
            this.историяОтгрузокToolStripMenuItem.Name = "историяОтгрузокToolStripMenuItem";
            this.историяОтгрузокToolStripMenuItem.Size = new System.Drawing.Size(240, 26);
            this.историяОтгрузокToolStripMenuItem.Text = "История отгрузок";
            this.историяОтгрузокToolStripMenuItem.Click += new System.EventHandler(this.историяОтгрузокToolStripMenuItem_Click);
            // 
            // отчетПоОтгрузкамToolStripMenuItem
            // 
            this.отчетПоОтгрузкамToolStripMenuItem.Name = "отчетПоОтгрузкамToolStripMenuItem";
            this.отчетПоОтгрузкамToolStripMenuItem.Size = new System.Drawing.Size(240, 26);
            this.отчетПоОтгрузкамToolStripMenuItem.Text = "Отчет по отгрузкам";
            this.отчетПоОтгрузкамToolStripMenuItem.Click += new System.EventHandler(this.отчетПоОтгрузкамToolStripMenuItem_Click);
            // 
            // списаниеПросрочкиToolStripMenuItem
            // 
            this.списаниеПросрочкиToolStripMenuItem.Name = "списаниеПросрочкиToolStripMenuItem";
            this.списаниеПросрочкиToolStripMenuItem.Size = new System.Drawing.Size(240, 26);
            this.списаниеПросрочкиToolStripMenuItem.Text = "Списание просрочки";
            this.списаниеПросрочкиToolStripMenuItem.Click += new System.EventHandler(this.списаниеПросрочкиToolStripMenuItem_Click);
            // 
            // сменитьРольToolStripMenuItem
            // 
            this.сменитьРольToolStripMenuItem.Name = "сменитьРольToolStripMenuItem";
            this.сменитьРольToolStripMenuItem.Size = new System.Drawing.Size(183, 24);
            this.сменитьРольToolStripMenuItem.Text = "Сменить пользователя";
            this.сменитьРольToolStripMenuItem.Click += new System.EventHandler(this.сменитьРольToolStripMenuItem_Click);
            // 
            // btnExit
            // 
            this.btnExit.BackColor = System.Drawing.Color.MediumAquamarine;
            this.btnExit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExit.Location = new System.Drawing.Point(1230, 0);
            this.btnExit.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(106, 32);
            this.btnExit.TabIndex = 1;
            this.btnExit.Text = "ВЫХОД";
            this.btnExit.UseVisualStyleBackColor = false;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // panelContent
            // 
            this.panelContent.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelContent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.panelContent.Location = new System.Drawing.Point(13, 78);
            this.panelContent.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.panelContent.Name = "panelContent";
            this.panelContent.Size = new System.Drawing.Size(1323, 565);
            this.panelContent.TabIndex = 2;
            // 
            // настройкиToolStripMenuItem
            // 
            this.настройкиToolStripMenuItem.Name = "настройкиToolStripMenuItem";
            this.настройкиToolStripMenuItem.Size = new System.Drawing.Size(240, 26);
            this.настройкиToolStripMenuItem.Text = "Настройки";
            this.настройкиToolStripMenuItem.Click += new System.EventHandler(this.настройкиToolStripMenuItem_Click);
            // 
            // FormAdminMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1349, 656);
            this.Controls.Add(this.panelContent);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.menuStrip);
            this.MainMenuStrip = this.menuStrip;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "FormAdminMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Складская система - Администратор";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        private System.Windows.Forms.ToolStripMenuItem отчетПоОтгрузкамToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem сменитьРольToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem списаниеПросрочкиToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem настройкиToolStripMenuItem;
    }
}