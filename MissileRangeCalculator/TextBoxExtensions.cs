using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MissileRangeCalculator
{
    public static class TextBoxExtensions
    {
        private const int EM_SETTABSTOPS = 0x00CB;

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr h, int msg, int wParam, int[] lParam);

        public static void SetTabStopWidth(this TextBox textbox, int width)
        {
            if (textbox.Multiline && textbox.AcceptsTab)
            {
                SendMessage(textbox.Handle, EM_SETTABSTOPS, 1, new int[] { width * 4 });
            }
        }
    }
}
