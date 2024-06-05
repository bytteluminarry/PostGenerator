using Microsoft.Data.Sqlite;
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
using System.Data.SqlClient;
using RestSharp;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Data.Entity.Infrastructure;
using System.Diagnostics.Eventing.Reader;

namespace PostGenerator
{
    public partial class Main : Form
    {
        private const string PlaceholderText = "Search for a template...";
        public static Main mainForm;
        ArrayList arraylist;
        string apiKey = "";

        public Main()
        {
            InitializeComponent();
            arraylist = new ArrayList();
            mainForm = this;
            dataGridView1.CellBorderStyle = DataGridViewCellBorderStyle.None;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.ColumnHeadersHeight = 30;
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.ColorTranslator.FromHtml("#fff");
            dataGridView1.RowTemplate.Height = 30;
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.CellBorderStyle = DataGridViewCellBorderStyle.Single;

            SetColumns();
            InitializeTextBox();
            FillTemplatesGrid();

            textBox1.GotFocus += TextBox1_GotFocus;
            textBox1.LostFocus += TextBox1_LostFocus;

            button1.Click += Button1_Click;

            dataGridView1.CellClick += DataGridView1_CellClick;

            textBox1.TextChanged += TextBox1_TextChanged;

            pictureBox1.Cursor = Cursors.Hand;
            pictureBox1.Click += PictureBox1_Click;
        }

        private void PictureBox1_Click(object sender, EventArgs e)
        {
            new Settings().ShowDialog();
        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textBox1.Text))
            {
                if (textBox1.Text == PlaceholderText)
                    FillTemplatesGrid();
                else  FillTemplatesGridQuery(textBox1.Text);
            }
            else
            {
                FillTemplatesGrid();
            }
        }

        private async void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Ensure the click is on a valid cell
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            string templateID = dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString();
            if (e.ColumnIndex == 3)
            {
                // Modify
                new ModifyTemplate(templateID).ShowDialog();
            }
            else if (e.ColumnIndex == 4)
            {
                // Delete
                DialogResult result = MessageBox.Show("Are you sure you want to delete this template?", "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // User confirmed deletion
                    try
                    {
                        Helper.sqliteConn.Open();

                        using (SQLiteCommand deleteCmd = new SQLiteCommand("DELETE FROM template WHERE id = @templateID", Helper.sqliteConn))
                        {
                            deleteCmd.Parameters.AddWithValue("@templateID", templateID);

                            int rowsAffected = deleteCmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Template deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                MessageBox.Show("Template not found or could not be deleted.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        if (Helper.sqliteConn.State == ConnectionState.Open)
                            Helper.sqliteConn.Close();
                        FillTemplatesGrid();
                    }
                }
            }
            else if (e.ColumnIndex == 5)
            {
                // Select
                Helper.sqliteConn.Open();
                SQLiteCommand cmd = new SQLiteCommand("select api from settings", Helper.sqliteConn);
                object key = cmd.ExecuteScalar();
                Helper.sqliteConn.Close();
                if (key != null && key != DBNull.Value)
                {
                    apiKey = (string)key;
                }
                else
                {
                    MessageBox.Show("You need to add an API key in settings first.", "API Key Missing", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                PleaseWait pleaseWaitForm = new PleaseWait();
                pleaseWaitForm.Show();
                await Task.Delay(2000);
                try
                {
                    arraylist.Clear();
                    Helper.sqliteConn.Open();
                    using (cmd = new SQLiteCommand("select * from message where template_id = @id order by priority", Helper.sqliteConn))
                    {
                        cmd.Parameters.AddWithValue("@id", templateID);
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    SendMessageToChatGPT(reader.GetValue(2).ToString());
                                }

                                //string[] array = (string[])arraylist.ToArray(typeof(string));
                                //string result = String.Join("\n\n", array);
                                string result = arraylist[arraylist.Count - 1].ToString();
                                new ChatgptResponse(result).ShowDialog();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Database error: {ex.Message}");
                }
                finally
                {
                    if (Helper.sqliteConn.State == System.Data.ConnectionState.Open)
                        Helper.sqliteConn.Close();
                    pleaseWaitForm.Close();
                }
            }
        }

        private void SendMessageToChatGPT(string messageContent)
        {
            var client = new RestClient("https://api.openai.com/v1/chat/completions");
            var request = new RestRequest();
            request.Method = Method.Post;
            request.AddHeader("Authorization", $"Bearer {apiKey}");
            request.AddHeader("Content-Type", "application/json");

            var body = new JObject
            {
                { "model", "gpt-3.5-turbo" },
                { "messages", new JArray(
                    new JObject
                    {
                        { "role", "user" },
                        { "content", messageContent }
                    })
                }
            };

            request.AddParameter("application/json", body.ToString(), ParameterType.RequestBody);

            try
            {
                var response = client.Execute(request);
                if (response.IsSuccessful)
                {
                    var jsonResponse = JObject.Parse(response.Content);
                    var chatResponse = jsonResponse["choices"][0]["message"]["content"].ToString();
                    arraylist.Add(chatResponse);
                    MessageBox.Show($"ChatGPT: {chatResponse}");
                }
                else
                {
                    //MessageBox.Show($"Error: {response.ErrorMessage}");
                    //MessageBox.Show($"Response: {response.Content}");
                    arraylist.Add(response.Content);
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"Exception: {ex.Message}");
                arraylist.Add($"Exception: {ex.Message}");
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            new NewTemplate().ShowDialog();
        }

        private void TextBox1_LostFocus(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                textBox1.Text = PlaceholderText;
                textBox1.ForeColor = System.Drawing.Color.Gray;
            }
        }

        private void TextBox1_GotFocus(object sender, EventArgs e)
        {
            if (textBox1.Text == PlaceholderText)
            {
                textBox1.Text = "";
                textBox1.ForeColor = System.Drawing.Color.Black;
            }
        }

        private void InitializeTextBox()
        {
            textBox1.Text = PlaceholderText;
            textBox1.ForeColor = System.Drawing.Color.Gray;
        }

        private void SetColumns()
        {
            dataGridView1.Columns.Clear();

            dataGridView1.Columns.Add("ID", "ID");
            dataGridView1.Columns.Add("Template_Name", "Template Name");
            dataGridView1.Columns.Add("Template_Description", "Template Description");

            DataGridViewButtonColumn modifyColumn = new DataGridViewButtonColumn();
            modifyColumn.HeaderText = "";
            modifyColumn.Text = "Modify";
            modifyColumn.UseColumnTextForButtonValue = true;
            modifyColumn.FlatStyle = FlatStyle.Flat;
            modifyColumn.DefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);
            dataGridView1.Columns.Add(modifyColumn);

            DataGridViewButtonColumn deleteColumn = new DataGridViewButtonColumn();
            deleteColumn.HeaderText = "";
            deleteColumn.Text = "Delete";
            deleteColumn.UseColumnTextForButtonValue = true;
            deleteColumn.FlatStyle = FlatStyle.Flat;
            deleteColumn.DefaultCellStyle.BackColor = Color.Tomato;
            deleteColumn.DefaultCellStyle.ForeColor= Color.White;
            dataGridView1.Columns.Add(deleteColumn);

            DataGridViewButtonColumn selectColumn = new DataGridViewButtonColumn();
            selectColumn.HeaderText = "";
            selectColumn.Text = "Generate Post";
            selectColumn.UseColumnTextForButtonValue = true;
            selectColumn.FlatStyle = FlatStyle.Flat;
            selectColumn.DefaultCellStyle.BackColor = Color.FromArgb(210, 210, 210);
            dataGridView1.Columns.Add(selectColumn);

            dataGridView1.Columns[0].Width = 80;

            dataGridView1.Columns[3].Width = 80;
            dataGridView1.Columns[4].Width = 80;
            dataGridView1.Columns[5].Width = 100;
        }

        public void FillTemplatesGrid()
        {
            dataGridView1.Rows.Clear();
            Helper.sqliteConn.Open();
            SQLiteCommand cmd = new SQLiteCommand("select * from template", Helper.sqliteConn);
            SQLiteDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                dataGridView1.Rows.Add(reader.GetValue(0), reader.GetValue(1), reader.GetValue(2));
            }
            Helper.sqliteConn.Close();
        }

        public void FillTemplatesGridQuery(string query)
        {
            dataGridView1.Rows.Clear();
            Helper.sqliteConn.Open();
            SQLiteCommand cmd = new SQLiteCommand("select * from template where lower(trim(name)) like '%" + query.Trim().ToLower() + "%'", Helper.sqliteConn);
            SQLiteDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                dataGridView1.Rows.Add(reader.GetValue(0), reader.GetValue(1), reader.GetValue(2));
            }
            Helper.sqliteConn.Close();
        }
    }
}