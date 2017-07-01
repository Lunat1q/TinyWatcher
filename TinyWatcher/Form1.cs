using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace TinyWatcher
{
    public partial class Form1 : Form
    {
        public Watcher wtc;
        public Form1(Watcher _wtc, bool debug)
        {
            //if (!debug)
            //{
                InitializeComponent();

            //}
            //else
            //{
            //    InitializeComponent();
            //    this.Visible = false;
            //}
            wtc = _wtc;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (wtc.Recording)
                wtc.StopRecord();
            else
            {
                wtc.StartRecord();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = "";
            foreach (ProcList item in wtc.procList)
            {
                richTextBox1.Text += string.Format("Name: {0} - Title: {1} - Usage: {2} seconds \r\n", item.procName, item.titleName, item.usage);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            wtc.WriteFile();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            richTextBox2.Text = wtc.sbText.ToString();
        }       
    }
}
