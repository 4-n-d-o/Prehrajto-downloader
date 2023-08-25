using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace downloader_2._0
{
    public partial class Form2 : Form
    {
        public string url;
        public Form2()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(box.Text);
            new ToastContentBuilder()
.AddText("Skopírované!")
.AddText(url)
.Show();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            box.Text = url;
        }

        private void box_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
