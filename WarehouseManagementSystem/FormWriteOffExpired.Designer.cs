namespace WarehouseManagementSystem
{
    partial class FormWriteOffExpired
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
            this.dgvExpiredProducts = new System.Windows.Forms.DataGridView();
            this.btnWriteOffSelected = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.lblSelectedInfo = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgvExpiredProducts)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvExpiredProducts
            // 
            this.dgvExpiredProducts.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvExpiredProducts.Location = new System.Drawing.Point(82, 134);
            this.dgvExpiredProducts.Margin = new System.Windows.Forms.Padding(4);
            this.dgvExpiredProducts.Name = "dgvExpiredProducts";
            this.dgvExpiredProducts.RowHeadersWidth = 51;
            this.dgvExpiredProducts.RowTemplate.Height = 24;
            this.dgvExpiredProducts.Size = new System.Drawing.Size(813, 254);
            this.dgvExpiredProducts.TabIndex = 0;
            this.dgvExpiredProducts.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvExpiredProducts_CellContentClick);
            // 
            // btnWriteOffSelected
            // 
            this.btnWriteOffSelected.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.btnWriteOffSelected.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.btnWriteOffSelected.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnWriteOffSelected.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnWriteOffSelected.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.btnWriteOffSelected.Location = new System.Drawing.Point(507, 429);
            this.btnWriteOffSelected.Margin = new System.Windows.Forms.Padding(4);
            this.btnWriteOffSelected.Name = "btnWriteOffSelected";
            this.btnWriteOffSelected.Size = new System.Drawing.Size(182, 45);
            this.btnWriteOffSelected.TabIndex = 1;
            this.btnWriteOffSelected.Text = "Списать выбранные";
            this.btnWriteOffSelected.UseVisualStyleBackColor = false;
            this.btnWriteOffSelected.Click += new System.EventHandler(this.btnWriteOffSelected_Click);
            // 
            // btnRefresh
            // 
            this.btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRefresh.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnRefresh.Location = new System.Drawing.Point(766, 438);
            this.btnRefresh.Margin = new System.Windows.Forms.Padding(4);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(129, 45);
            this.btnRefresh.TabIndex = 2;
            this.btnRefresh.Text = "Обновить";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // lblSelectedInfo
            // 
            this.lblSelectedInfo.AutoSize = true;
            this.lblSelectedInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblSelectedInfo.Location = new System.Drawing.Point(78, 414);
            this.lblSelectedInfo.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblSelectedInfo.Name = "lblSelectedInfo";
            this.lblSelectedInfo.Size = new System.Drawing.Size(343, 20);
            this.lblSelectedInfo.TabIndex = 3;
            this.lblSelectedInfo.Text = "Выбрано товаров: 0, сумма убытка: 0.00 руб.";
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.lblTitle.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.lblTitle.Location = new System.Drawing.Point(78, 59);
            this.lblTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(375, 25);
            this.lblTitle.TabIndex = 4;
            this.lblTitle.Text = "Списание просроченных товаров";
            // 
            // FormWriteOffExpired
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(980, 562);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblSelectedInfo);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.btnWriteOffSelected);
            this.Controls.Add(this.dgvExpiredProducts);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "FormWriteOffExpired";
            this.Text = "FormWriteOffExpired";
            ((System.ComponentModel.ISupportInitialize)(this.dgvExpiredProducts)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvExpiredProducts;
        private System.Windows.Forms.Button btnWriteOffSelected;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Label lblSelectedInfo;
        private System.Windows.Forms.Label lblTitle;
    }
}