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
    public partial class ModifyMessageInfo : Form
    {
        DataGridView dataGridView;
        int rowIndex;

        public ModifyMessageInfo(DataGridView dataGridView, int rowIndex)
        {
            InitializeComponent();
            this.dataGridView = dataGridView;
            this.rowIndex = rowIndex;

            textBox1.Text = dataGridView.Rows[rowIndex].Cells[0].Value.ToString();
            textBox2.Text = dataGridView.Rows[rowIndex].Cells[1].Value.ToString();

            button1.Click += Button1_Click;

            this.Shown += Modify_MessageInfo_Shown;
        }

        private void Modify_MessageInfo_Shown(object sender, EventArgs e)
        {
            label1.Focus();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("You need to specify the message priority.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(textBox2.Text))
            {
                MessageBox.Show("You need to specify the message content.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            dataGridView.Rows[rowIndex].Cells[0].Value = textBox1.Text;
            dataGridView.Rows[rowIndex].Cells[1].Value = textBox2.Text;
            this.Close();
        }
    }
}
