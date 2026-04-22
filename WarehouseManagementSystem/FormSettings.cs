using Npgsql;
using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WarehouseManagementSystem.Helpers;

namespace WarehouseManagementSystem
{
    public partial class FormSettings : Form
    {
        public FormSettings()
        {
            InitializeComponent();
        }

        private void FormSettings_Load(object sender, EventArgs e)
        {
            Text = "Настройки";
            LoadSettings();
        }

        private void LoadSettings()
        {
            try
            {
                string sql = "SELECT SettingKey, SettingValue FROM AppSettings";
                DataTable data = DatabaseHelper.ExecuteQuery(sql);

                foreach (DataRow row in data.Rows)
                {
                    string key = row["SettingKey"].ToString();
                    string value = row["SettingValue"].ToString();

                    if (key == "DiscountPercentage")
                        numDiscountPercent.Value = Convert.ToInt32(value);
                    else if (key == "DiscountDaysBeforeExpiry")
                        numDiscountDays.Value = Convert.ToInt32(value);
                }
            }
            catch (Exception ex)
            {
                numDiscountPercent.Value = 20;
                numDiscountDays.Value = 30;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                SaveSetting("DiscountPercentage", numDiscountPercent.Value.ToString());

                SaveSetting("DiscountDaysBeforeExpiry", numDiscountDays.Value.ToString());

                MessageBox.Show(String.SettingsSavedSuccess, String.SuccessTitle,
    MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(String.SettingsSaveError, ex.Message), String.ErrorTitle,
      MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void SaveSetting(string key, string value)
        {
            string sql = @"
        INSERT INTO AppSettings (SettingKey, SettingValue, Description)
        VALUES (@key, @value, '')
        ON CONFLICT (SettingKey) 
        DO UPDATE SET SettingValue = @value, UpdatedAt = CURRENT_TIMESTAMP";

            var parameters = new[]
            {
        new NpgsqlParameter("@key", key),
        new NpgsqlParameter("@value", value)
    };

            DatabaseHelper.ExecuteNonQuery(sql, parameters);
        }
    }
}
