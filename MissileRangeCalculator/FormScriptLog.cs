using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MissileRangeCalculator
{
    public partial class FormScriptLog : Form
    {
        List<string> logs = new List<string>(2048);

        public FormScriptLog()
        {
            InitializeComponent();
        }

        public void AddLog(string log)
        {
            logs.Add(log);
        }

        public void ClearLog()
        {
            logs.Clear();
            txtScriptLogs.Clear();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearLog();
        }

        private void FormScriptLog_FormClosing(object sender, FormClosingEventArgs e)
        {
            (this.Owner as FormMain).ScriptLogCallback(FormMain.ScriptLogOperation.Close);
        }

        private void timerFlushLog_Tick(object sender, EventArgs e)
        {
            if (logs.Count == 0) return;
            StringBuilder sb = new StringBuilder();
            foreach (string log in logs)
            {
                sb.AppendLine(log);
            }
            logs.Clear();
            txtScriptLogs.Text += sb.ToString();
            txtScriptLogs.Select(txtScriptLogs.Text.Length, 0);
        }
    }
}
