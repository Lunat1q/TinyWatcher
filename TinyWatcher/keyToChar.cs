using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Globalization;
using System.ComponentModel;
using System.Windows;

namespace TinyWatcher
{
    public static class keyToChar
    {
        public static string toString(Keys key, bool Shift)
        {
            string k = "";
            switch (key)
            {
                case Keys.Oemcomma:
                    return "[Comma]";
                case Keys.OemPeriod:
                    return "[Period]";
                case Keys.LShiftKey:
                    return "";
                case Keys.RShiftKey:
                    return "";
                case Keys.Shift:
                    return "";               
                case Keys.ShiftKey:
                    return "";  
                case Keys.Alt:
                    return "";
                case Keys.Control:
                    return "";
                case Keys.ControlKey:
                    return "";
                case Keys.LMenu:
                    return "";
                default:
                    k = !Shift ? (new KeysConverter()).ConvertToString(key).ToLower() : (new KeysConverter()).ConvertToString(key).ToUpper();
                    break;
            }

            return k;
        }
        [DllImport("user32.dll")]
        public static extern int ToUnicode(uint virtualKeyCode, uint scanCode,
            byte[] keyboardState,
            [Out, MarshalAs(UnmanagedType.LPWStr, SizeConst = 64)]
    StringBuilder receivingBuffer,
            int bufferSize, uint flags);

        public static string GetCharsFromKeys(Keys keys, bool shift, bool altGr)
        {
            var buf = new StringBuilder(256);
            var keyboardState = new byte[256];
            if (shift)
                keyboardState[(int)Keys.ShiftKey] = 0xff;
            if (altGr)
            {
                keyboardState[(int)Keys.ControlKey] = 0xff;
                keyboardState[(int)Keys.Menu] = 0xff;
            }
            ToUnicode((uint)keys, 0, keyboardState, buf, 256, 0);
            return buf.ToString();
        }
    }
}
