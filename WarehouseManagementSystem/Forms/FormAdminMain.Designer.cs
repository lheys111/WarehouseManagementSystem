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
            this.настройкиToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.сменитьРольToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnExit = new System.Windows.Forms.Button();
            this.panelContent = new System.Windows.Forms.Panel();
            this.menuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip
            // 
            this.menuStrip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.menuStrip.ImageScalingSize = new System.Drawing.Size(50, 150);
            this.menuStrip.ImeMode = System.Windows.Forms.ImeMode.Hiragana;
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.справочникиToolStripMenuItem,
            this.администрированиеToolStripMenuItem,
            this.сменитьРольToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(1349, 33);
            this.menuStrip.TabIndex = 0;
            this.menuStrip.Text = "menuStrip";
            // 
            // справочникиToolStripMenuItem
            // 
            this.справочникиToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.товарыToolStripMenuItem,
            this.категорииToolStripMenuItem});
            this.справочникиToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.справочникиToolStripMenuItem.Name = "справочникиToolStripMenuItem";
            this.справочникиToolStripMenuItem.Size = new System.Drawing.Size(137, 29);
            this.справочникиToolStripMenuItem.Text = "Справочники";
            // 
            // товарыToolStripMenuItem
            // 
            this.товарыToolStripMenuItem.Name = "товарыToolStripMenuItem";
            this.товарыToolStripMenuItem.Size = new System.Drawing.Size(182, 30);
            this.товарыToolStripMenuItem.Text = "Товары";
            this.товарыToolStripMenuItem.Click += new System.EventHandler(this.товарыToolStripMenuItem_Click);
            // 
            // категорииToolStripMenuItem
            // 
            this.категорииToolStripMenuItem.Name = "категорииToolStripMenuItem";
            this.категорииToolStripMenuItem.Size = new System.Drawing.Size(182, 30);
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
            this.администрированиеToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.администрированиеToolStripMenuItem.Name = "администрированиеToolStripMenuItem";
            this.администрированиеToolStripMenuItem.Size = new System.Drawing.Size(197, 29);
            this.администрированиеToolStripMenuItem.Text = "Администрирование";
            // 
            // историяОтгрузокToolStripMenuItem
            // 
            this.историяОтгрузокToolStripMenuItem.Name = "историяОтгрузокToolStripMenuItem";
            this.историяОтгрузокToolStripMenuItem.Size = new System.Drawing.Size(271, 30);
            this.историяОтгрузокToolStripMenuItem.Text = "История отгрузок";
            this.историяОтгрузокToolStripMenuItem.Click += new System.EventHandler(this.историяОтгрузокToolStripMenuItem_Click);
            // 
            // отчетПоОтгрузкамToolStripMenuItem
            // 
            this.отчетПоОтгрузкамToolStripMenuItem.Name = "отчетПоОтгрузкамToolStripMenuItem";
            this.отчетПоОтгрузкамToolStripMenuItem.Size = new System.Drawing.Size(271, 30);
            this.отчетПоОтгрузкамToolStripMenuItem.Text = "Отчет по отгрузкам";
            this.отчетПоОтгрузкамToolStripMenuItem.Click += new System.EventHandler(this.отчетПоОтгрузкамToolStripMenuItem_Click);
            // 
            // списаниеПросрочкиToolStripMenuItem
            // 
            this.списаниеПросрочкиToolStripMenuItem.Name = "списаниеПросрочкиToolStripMenuItem";
            this.списаниеПросрочкиToolStripMenuItem.Size = new System.Drawing.Size(271, 30);
            this.списаниеПросрочкиToolStripMenuItem.Text = "Списание просрочки";
            this.списаниеПросрочкиToolStripMenuItem.Click += new System.EventHandler(this.списаниеПросрочкиToolStripMenuItem_Click);
            // 
            // настройкиToolStripMenuItem
            // 
            this.настройкиToolStripMenuItem.Name = "настройкиToolStripMenuItem";
            this.настройкиToolStripMenuItem.Size = new System.Drawing.Size(271, 30);
            this.настройкиToolStripMenuItem.Text = "Настройки";
            this.настройкиToolStripMenuItem.Click += new System.EventHandler(this.настройкиToolStripMenuItem_Click);
            // 
            // сменитьРольToolStripMenuItem
            // 
            this.сменитьРольToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.сменитьРольToolStripMenuItem.Name = "сменитьРольToolStripMenuItem";
            this.сменитьРольToolStripMenuItem.ShowShortcutKeys = false;
            this.сменитьРольToolStripMenuItem.Size = new System.Drawing.Size(211, 29);
            this.сменитьРольToolStripMenuItem.Text = "Сменить пользователя";
            this.сменитьРольToolStripMenuItem.Click += new System.EventHandler(this.сменитьРольToolStripMenuItem_Click);
            // 
            // btnExit
            // 
            this.btnExit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExit.BackColor = System.Drawing.Color.White;
            this.btnExit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExit.Location = new System.Drawing.Point(1241, 0);
            this.btnExit.Margin = new System.Windows.Forms.Padding(4);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(95, 28);
            this.btnExit.TabIndex = 1;
            this.btnExit.Text = "ВЫХОД";
            this.btnExit.UseVisualStyleBackColor = false;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // panelContent
            // 
            this.panelContent.BackColor = System.Drawing.Color.White;
            this.panelContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelContent.Location = new System.Drawing.Point(0, 33);
            this.panelContent.Margin = new System.Windows.Forms.Padding(4);
            this.panelContent.Name = "panelContent";
            this.panelContent.Size = new System.Drawing.Size(1349, 623);
            this.panelContent.TabIndex = 2;
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
            this.Margin = new System.Windows.Forms.Padding(4);
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