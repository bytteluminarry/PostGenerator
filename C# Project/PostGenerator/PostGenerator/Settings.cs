using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;

namespace PostGenerator
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();
            button1.Click += Button1_Click;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("You need to specify the api key.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            Helper.sqliteConn.Open();
            SQLiteCommand deleteCmd = new SQLiteCommand("DELETE FROM settings", Helper.sqliteConn);
            deleteCmd.ExecuteNonQuery();
            SQLiteCommand insertCmd = new SQLiteCommand("INSERT INTO settings(api) VALUES(@key)", Helper.sqliteConn);
            insertCmd.Parameters.AddWithValue("@key", textBox1.Text);
            insertCmd.ExecuteNonQuery();
            Helper.sqliteConn.Close();
            Main.mainForm.FillTemplatesGrid();
            MessageBox.Show("API key selected successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();

        }
    }
}
