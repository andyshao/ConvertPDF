using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConvertPDF
{
    public partial class Form2 : Form
    {
        public bool checkPass = false;
        private const string PASS = "KWXVNwMu8BTyDqQlEfvPycQD7WMZFQ0tyfLNgwbuLbmqf3P0hZRWS6Xdwa8f9ZBIDPvUZHEDhtU3wXC4IZQTLfGMXx7WFqa3YRf0EQFyUdWBFCJkxLi0kvzfyuyDEoke4YuVRogPf79j4td9ep2S40SNOnpsbsW7QYNWBNSK3CNJWJo6uJxIPCjItdDRu0GUOylO3aqquloBqbvlSKpDKssdJPuTQLLimcf0zkIFW0Msa7OpcqnhZ4JZuvrDe7o";
        public Form2()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(textBox1.Text))
            {
                MessageBox.Show("Please enter key");
                textBox1.Focus();
                return;
            }
            if (PASS.Equals(textBox1.Text))
            {
                checkPass = true;
                this.Dispose();
            }
            else
            {
                MessageBox.Show("Key is wrong.Try again");
                checkPass = false;
                return;
            }
        }
    }
}
