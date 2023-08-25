using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.IO;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading;
using System.Net;
using Microsoft.Toolkit.Uwp.Notifications;

namespace downloader_2._0
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        public string zvolene;

        private void button1_Click(object sender, EventArgs e)
        {
            vyhladavanie();
        }
        private void vyhladavanie()
        {
            if (box.Text == "")
            {
                MessageBox.Show("Please enter valid keywords!");
            }
            else
            {
                vysledky.Items.Clear();
                string html = "none";
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = client.GetAsync("https://prehraj.to/hledej/" + box.Text.Replace(" ", "%20")).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        html = response.Content.ReadAsStringAsync().Result.Replace("\n", "");
                    }
                    else
                    {
                        MessageBox.Show("Server responded with error.");
                    }
                }

                string pattern = "<a class=\"video video--small video--link\" href=\"(.*?)\" tit";
                //Old query <div class=\"column\">\\s*<a href=
                MatchCollection matches = Regex.Matches(html, pattern);

                if (matches.Count > 0)
                {
                    foreach (Match match in matches)
                    {
                        string text = match.Groups[1].Value;
                        vysledky.Items.Add(text);
                    }
                }
                else
                {
                    MessageBox.Show("Nothing found for this search query. :(");
                }
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            if (vysledky.SelectedItem == null)
            {
                MessageBox.Show("Please select what you want to download.");
            }
            else
            {
                zvolene = vysledky.SelectedItem.ToString();
                string html = "none";
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = client.GetAsync("https://prehraj.to/" + zvolene).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        html = response.Content.ReadAsStringAsync().Result.Replace("\n", "");
                    }
                    else
                    {
                        MessageBox.Show("Server responded with error code.");
                    }
                }





                if (html != "none")
                {
                    string pattern = "var sources = (.*?);";
                    Match match = Regex.Match(html, pattern);

                    string sources = match.Groups[1].Value;

                    dynamic data = JsonConvert.DeserializeObject(sources);
                    string link = "err";
                    try
                    {
                        dynamic element = data[1];

                        link = element.file;
                    }
                    catch (Exception)
                    {
                        dynamic element = data[0];

                        link = element.file;
                    }
                    zacni_stahovanie(link);
                }

            }

        }
        private void zacni_stahovanie(string url)
        {
            SaveFileDialog g = new SaveFileDialog();
            g.Title = "Choose where to download the file.";
            g.InitialDirectory = @"C:\Users\" + Environment.UserName + "\\Desktop";
            g.DefaultExt = "mp4";
            g.Filter = "Video file (*.mp4)|*.mp4";
            g.FilterIndex = 2;
            g.ShowDialog();
            guna2ProgressBar1.Show();
            if (g.FileName != "")
            {
                Thread thread = new Thread(() => {
                    WebClient client = new WebClient();
                    client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(progress);
                    client.DownloadFileCompleted += new AsyncCompletedEventHandler(stiahnute);
                    client.DownloadFileAsync(new Uri(url), g.FileName);
                });
                thread.Start();
            } else
            {
                MessageBox.Show("Please choose where to save.");
            }
        }
        void progress(object sender, DownloadProgressChangedEventArgs e)
        {
            this.BeginInvoke((MethodInvoker)delegate {
                double bytesIn = double.Parse(e.BytesReceived.ToString());
                double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
                double percentage = bytesIn / totalBytes * 100;
                label2.Text = "Progress " + e.BytesReceived + " / " + e.TotalBytesToReceive;
                guna2ProgressBar1.Value = int.Parse(Math.Truncate(percentage).ToString());
            });
        }
        void stiahnute(object sender, AsyncCompletedEventArgs e)
        {
            new ToastContentBuilder()
    .AddText("Succesfully downloaded.")
    .AddText(zvolene)
    .Show();
            this.BeginInvoke((MethodInvoker)delegate {
                label2.Text = "Succesfully downloaded. " + zvolene;
            });
            wait(5000);
            this.BeginInvoke((MethodInvoker)delegate {
                guna2ProgressBar1.Hide();
                label2.Text = "";
                vysledky.Items.Clear();
                box.Text = "";
            });
        }
        public void wait(int milliseconds)
        {
            var timer1 = new System.Windows.Forms.Timer();
            if (milliseconds == 0 || milliseconds < 0) return;

            timer1.Interval = milliseconds;
            timer1.Enabled = true;
            timer1.Start();

            timer1.Tick += (s, e) =>
            {
                timer1.Enabled = false;
                timer1.Stop();
            };

            while (timer1.Enabled)
            {
                System.Windows.Forms.Application.DoEvents();
            }
        }


        private void Form1_Load(object sender, EventArgs e)
        {
        }

        void Resizelabel(int percentage, Control control)
        {
            float currentSize = control.Font.Size;
            control.Font = new Font(control.Font.FontFamily, currentSize * (percentage / 100f), control.Font.Style);
        }
        void ResizeControl(int percentage, Control control)
        {
            int newWidth = (int)(control.Width * (percentage / 100f));
            int newHeight = (int)(control.Height * (percentage / 100f));
            int newX = (int)(control.Location.X * (percentage / 100f));
            int newY = (int)(control.Location.Y * (percentage / 100f));
            control.Size = new Size(newWidth, newHeight);
            control.Location = new Point(newX, newY);
            if (control is Label)
            {
                Label label = (Label)control;
                float currentSize = label.Font.Size;
                label.Font = new Font(label.Font.FontFamily, currentSize * (percentage / 100f), label.Font.Style);
                label.AutoSize = true;
            }
        }





        void ResizeForm(int percentage)
        {
            foreach (Control control in Controls)
            {
                ResizeControl(percentage, control);
            }
            ResizeControl(percentage, this);
        }
        void resize(int percentage)
        {
            ResizeForm(percentage);
            Resizelabel(percentage, hladaj);
            Resizelabel(percentage, stiahnut);
            Resizelabel(percentage, label2);
            Resizelabel(percentage, box);
            Resizelabel(percentage, vysledky);
            Resizelabel(percentage, button1);
        }

        void ResetResize()
        {

        }
        private void label2_Click(object sender, EventArgs e)
        {

        }
        public Boolean maximized = false;
        private void guna2ControlBox2_Click(object sender, EventArgs e)
        {
            if (maximized == true)
            {
                resize(50);
                maximized = false;
            }
            else
            {
                maximized = true;
                resize(200);
            }
        }

        public int limit = 0;
        private void box_TextChanged(object sender, EventArgs e)
        {

                if (limit == 5)
                {
                    vyhladavanie();
                    limit = 0;
                }
                else
                    limit++;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            try
            {
                zvolene = vysledky.SelectedItem.ToString();
                string html = "none";
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = client.GetAsync("https://prehraj.to/" + zvolene).Result;

                    if (response.IsSuccessStatusCode)
                        html = response.Content.ReadAsStringAsync().Result.Replace("\n", "");
                    else
                        MessageBox.Show("Server responded with error code.");
                }
                if (html != "none")
                {
                    string pattern = "var sources = (.*?);";
                    Match match = Regex.Match(html, pattern);

                    string sources = match.Groups[1].Value;

                    dynamic data = JsonConvert.DeserializeObject(sources);
                    string link = "err";
                    try
                    {
                        dynamic element = data[1];

                        link = element.file;
                    }
                    catch (Exception)
                    {
                        dynamic element = data[0];

                        link = element.file;
                    }
                    Form2 g = new Form2();
                    g.url = link;
                    g.Show();
                }
            } catch (Exception)
            {
                MessageBox.Show("Please first select file to download!");
            }
        }
    }
}
