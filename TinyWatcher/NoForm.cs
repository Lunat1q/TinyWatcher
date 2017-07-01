using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Text;

namespace TinyWatcher
{
    class NoForm : ApplicationContext
    {
        Watcher wtc;
        public NoForm(Watcher _wtc)
        {
            this.ThreadExit += NoForm_ThreadExit;
            wtc = _wtc;
            wtc.StartRecord();
        }

        void NoForm_ThreadExit(object sender, EventArgs e)
        {
            wtc.WriteFile();
        }      
      
        
    }
}
