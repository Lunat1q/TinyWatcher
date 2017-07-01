using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace TinyWatcher
{
    static class Program
    {        
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Watcher wtc = new Watcher();
            if (Properties.Settings.Default.debug)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1(wtc, Properties.Settings.Default.debug));
            }
            else
            {
               //wtc.StartRecord();
               Application.Run(new NoForm(wtc));           
            }
        }
    }
}
