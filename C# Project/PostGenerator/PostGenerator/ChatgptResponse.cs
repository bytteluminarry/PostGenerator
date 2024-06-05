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
    public partial class ChatgptResponse : Form
    {
        string result;
        public ChatgptResponse(string result)
        {
            InitializeComponent();
            this.result = result;
            richTextBox1.Text = result;
            button1.Click += Button1_Click;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(result);
        }
    }
}
