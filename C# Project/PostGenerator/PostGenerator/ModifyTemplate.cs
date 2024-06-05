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
    public partial class ModifyTemplate : Form
    {
        string templateID;

        public ModifyTemplate(string templateID)
        {
            InitializeComponent();
            this.templateID = templateID;
            dataGridView1.CellBorderStyle = DataGridViewCellBorderStyle.None;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.ColumnHeadersHeight = 30;
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.ColorTranslator.FromHtml("#fff");
            dataGridView1.RowTemplate.Height = 30;
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.CellBorderStyle = DataGridViewCellBorderStyle.Single;

            dataGridView1.CellClick += DataGridView1_CellClick;

            this.Shown += ModifyTemplate_Shown;

            SetColumns();
            FillData();

            button1.Click += Button1_Click;
            button2.Click += Button2_Click;
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("You need to specify the template name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (dataGridView1.Rows.Count == 0)
            {
                MessageBox.Show("You need to add at least one message to the list view.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            Helper.sqliteConn.Open();
            SQLiteCommand cmd;

            cmd = new SQLiteCommand("update template set name = @name, description = @description where id = @templateID", Helper.sqliteConn);
            cmd.Parameters.AddWithValue("@name", textBox1.Text);
            cmd.Parameters.AddWithValue("@description", textBox2.Text);
            cmd.Parameters.AddWithValue("@templateID", templateID);
            cmd.ExecuteNonQuery();

            cmd = new SQLiteCommand("delete from message where template_id = @templateID", Helper.sqliteConn);
            cmd.Parameters.AddWithValue("@templateID", templateID);
            cmd.ExecuteNonQuery();

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                cmd = new SQLiteCommand("insert into message(priority, message, template_id) values(@priority, @message, @id)", Helper.sqliteConn);
                cmd.Parameters.AddWithValue("@priority", row.Cells[0].Value.ToString());
                cmd.Parameters.AddWithValue("@message", row.Cells[1].Value.ToString());
                cmd.Parameters.AddWithValue("@id", templateID);
                cmd.ExecuteNonQuery();
            }

            Helper.sqliteConn.Close();
            Main.mainForm.FillTemplatesGrid();
            MessageBox.Show("Template modified successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            new MessageInfo(dataGridView1).ShowDialog();
        }

        private void FillData()
        {
            dataGridView1.Rows.Clear();
            Helper.sqliteConn.Open();
            SQLiteCommand cmd = new SQLiteCommand("select * from template where id = @id", Helper.sqliteConn);
            cmd.Parameters.AddWithValue("@id", templateID);
            SQLiteDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                reader.Read();
                textBox1.Text = reader.GetValue(1).ToString();
                textBox2.Text = reader.GetValue(2).ToString();
            }
            reader.Close();
            cmd = new SQLiteCommand("select * from message where template_id = @id", Helper.sqliteConn);
            cmd.Parameters.AddWithValue("@id", templateID);
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                dataGridView1.Rows.Add(reader.GetValue(1), reader.GetValue(2), null, null, reader.GetValue(0));
            }
            Helper.sqliteConn.Close();
        }

        private void ModifyTemplate_Shown(object sender, EventArgs e)
        {
            label1.Focus();
        }

        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 2)
            {
                //Modify
                new ModifyMessageInfo(dataGridView1, e.RowIndex).ShowDialog();
            }
            else if (e.ColumnIndex == 3)
            {
                //Delete
                dataGridView1.Rows.RemoveAt(e.RowIndex);
            }
        }

        private void SetColumns()
        {
            dataGridView1.Columns.Clear();

            dataGridView1.Columns.Add("Priority", "Priority");
            dataGridView1.Columns.Add("Message", "Message");

            DataGridViewButtonColumn modifyColumn = new DataGridViewButtonColumn();
            modifyColumn.HeaderText = "";
            modifyColumn.Text = "Modify";
            modifyColumn.UseColumnTextForButtonValue = true;
            modifyColumn.FlatStyle = FlatStyle.Flat;
            modifyColumn.DefaultCellStyle.BackColor = Color.FromArgb(210, 210, 210);
            dataGridView1.Columns.Add(modifyColumn);

            DataGridViewButtonColumn deleteColumn = new DataGridViewButtonColumn();
            deleteColumn.HeaderText = "";
            deleteColumn.Text = "Delete";
            deleteColumn.UseColumnTextForButtonValue = true;
            deleteColumn.FlatStyle = FlatStyle.Flat;
            deleteColumn.DefaultCellStyle.BackColor = Color.Tomato;
            deleteColumn.DefaultCellStyle.ForeColor = Color.White;
            dataGridView1.Columns.Add(deleteColumn);

            dataGridView1.Columns.Add("ID", "ID");
            dataGridView1.Columns[dataGridView1.Columns.Count - 1].Visible = false;
        }
    }
}