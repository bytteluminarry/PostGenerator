using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PostGenerator
{
    public partial class MessageInfo : Form
    {
        DataGridView dataGridView;

        public MessageInfo(DataGridView dataGridView)
        {
            InitializeComponent();
            int priority = dataGridView.Rows.Count;
            textBox1.Text = (priority + 1).ToString();

            this.dataGridView = dataGridView;

            button1.Click += Button1_Click;

            this.Shown += MessageInfo_Shown;
        }

        private void MessageInfo_Shown(object sender, EventArgs e)
        {
            label1.Focus();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("You need to specify the message priority.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (string.IsNullOrWhiteSpace(textBox2.Text))
            {
                MessageBox.Show("You need to specify the message content.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            dataGridView.Rows.Add(textBox1.Text, textBox2.Text);
            this.Close();
        }
    }
}
