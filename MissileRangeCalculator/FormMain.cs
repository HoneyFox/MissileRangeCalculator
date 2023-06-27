using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CSharpScriptExecutor;
using System.Reflection;

namespace MissileRangeCalculator
{
    public partial class FormMain : Form
    {
        public static FormMain singleton;
        public Plotter plotter;
        public Simulator simulator;

        string curScript = "";
        string curScriptInfo = "";
        ScriptModule curScriptModule = null;
        List<string> curScriptErrors = null;
        ScriptInstance curScriptInstance = null;

        internal bool isShiftDown;
        internal bool isCtrlDown;

        public FormMain()
        {
            InitializeComponent();
            singleton = this;
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            plotter = new Plotter(this, this.picMain, this.Font, this.picPlotData, this.picLegends);
            plotter.Clear();
            plotter.RenderLegends();

            string[] parameters = Environment.CommandLine.Split(new char[]{' '}, StringSplitOptions.RemoveEmptyEntries);
            if (parameters.Length == 2)
            {
                string filePath = parameters[1].Trim('"');
                ParseInfo(File.ReadAllText(filePath));
                openFileDialog.FileName = filePath;
                saveFileDialog.FileName = filePath;
                this.Text = "Missile Range Calculator - " + filePath;
            }
        }

        private void btnSimulate_Click(object sender, EventArgs e)
        {
            plotter.prevCheckFrame = -1;
            plotter.prevCheckFrameDownRangeX = -1;
            Simulate();
        }

        private void Simulate()
        {
            List<MotorInfo> motorInfo = MotorInfo.AnalyzeMotorInfo(txtMotor.Text);
            List<AeroInfo> aeroInfo = AeroInfo.AnalyzeAeroInfo(txtMotor.Text, new float[] { float.Parse(txtSubsonicDrag.Text), float.Parse(txtSupersonicDrag.Text), float.Parse(txtInducedDragFactor.Text), float.Parse(txtDiameter.Text), float.Parse(txtCLMax.Text) });
            List<AngleInfo> angleRateInfo = AngleInfo.AnalyzeAngleInfo(txtPitch.Text);
            List<ScriptInfo> scriptInfo = ScriptInfo.AnalyzeScriptInfo(curScriptInfo, curScriptModule);

            float deltaTime = 0.25f;
            float accuracy = 1f / 64f;
            if (txtDeltaTime.Text.Contains(","))
            {
                string[] components = txtDeltaTime.Text.Split(new char[] { ',' });
                deltaTime = float.Parse(components[0]);
                accuracy = 1f / float.Parse(components[1]);
            }
            else
            {
                deltaTime = float.Parse(txtDeltaTime.Text);
            }

            simulator = new Simulator(plotter, deltaTime, accuracy, float.Parse(txtSubsonicDrag.Text), float.Parse(txtSupersonicDrag.Text), float.Parse(txtInducedDragFactor.Text), float.Parse(txtCLMax.Text),
                motorInfo, aeroInfo, float.Parse(txtDryMass.Text), float.Parse(txtDiameter.Text), float.Parse(txtInitSpeed.Text), float.Parse(txtInitAngle.Text), float.Parse(txtInitAlt.Text),
                float.Parse(txtTargetSpeed.Text), float.Parse(txtTargetDistance.Text), angleRateInfo, scriptInfo, curScriptInstance, float.Parse(txtCutoffSpeed.Text));

            plotter.Clear();
            plotter.SetCutoffSpeed(float.Parse(txtCutoffSpeed.Text));
            plotter.SetRenderScale(float.Parse(txtDisplayScale.Text));
            simulator.Simulate();
        }

        private string AdjustTimeValue(string input, int delta, int adjustment)
        {
            string[] components = input.Split(new char[] { ',' });
            if (components[0].Contains("."))
            {
                double timeValue;
                if (double.TryParse(components[0], out timeValue))
                {
                    int digits = components[0].Split(new char[] { '.' })[1].Length;
                    double newValue = Math.Max(0.0, timeValue + Math.Sign(delta) * adjustment);
                    components[0] = newValue.ToString("f" + digits.ToString());
                }
            }
            else
            {
                int timeValue;
                if (int.TryParse(components[0], out timeValue))
                {
                    int newValue = Math.Max(0, timeValue + (int)Math.Sign(delta) * adjustment);
                    components[0] = newValue.ToString();
                }
            }
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < components.Length; ++i)
            {
                sb.Append(components[i]);
                if (i < components.Length - 1)
                    sb.Append(",");
            }
            return sb.ToString();
        }

        private void txtPitch_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta != 0)
            {
                if (txtPitch.SelectedText != "" && (this.isShiftDown || this.isCtrlDown))
                {
                    txtPitch.SelectedText = AdjustTimeValue(txtPitch.SelectedText, e.Delta, this.isCtrlDown ? 1 : (this.isShiftDown ? 50 : 10));
                    Simulate();
                    picMain_MouseDown(this, new MouseEventArgs(MouseButtons.Left, 1, simulator.plotter.prevCheckFrame, 0, 0));
                }
            }
        }

        private void txtPitch_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                if (txtPitch.SelectedText != "")
                {
                    if (this.isCtrlDown)
                    {
                        // Split to two lines
                        Plotter.PlotData data = plotter.GetPlotData(plotter.prevCheckFrame);
                        if (data != null)
                        {
                            float time = data.time;
                            AngleInfo ai = simulator.GetAngleInfo(time);
                            float time1 = time - ai.timeStart;
                            float time2 = ai.timeEnd - time;
                            string txtToCopy = txtPitch.SelectedText;
                            string txtCopied = txtToCopy;
                            txtToCopy = time1.ToString() + txtToCopy.Substring(txtToCopy.IndexOf(','));
                            txtCopied = time2.ToString() + txtCopied.Substring(txtCopied.IndexOf(','));
                            txtPitch.Text = txtPitch.Text.Substring(0, txtPitch.SelectionStart) + txtToCopy + Environment.NewLine + txtCopied + txtPitch.Text.Substring(txtPitch.SelectionStart + txtPitch.SelectionLength);
                            Simulate();
                            picMain_MouseDown(this, new MouseEventArgs(MouseButtons.Left, 1, simulator.plotter.prevCheckFrame, 0, 0));
                        }
                    }
                    else if (this.isShiftDown)
                    {
                        // Merge to previous line
                        Plotter.PlotData data = plotter.GetPlotData(plotter.prevCheckFrame);
                        if (data != null)
                        {
                            float time = data.time;
                            AngleInfo ai = simulator.GetAngleInfo(time);
                            AngleInfo aiPrev = simulator.GetAngleInfo(ai.timeStart - float.Parse(txtDeltaTime.Text));
                            if (aiPrev != null && ai != null)
                            {
                                float totalTime = ai.timeEnd - ai.timeStart + aiPrev.timeEnd - aiPrev.timeStart;
                                string txtToModify = txtPitch.SelectedText;
                                txtToModify = totalTime.ToString() + txtToModify.Substring(txtToModify.IndexOf(','));
                                int prevLineEnd = txtPitch.SelectionStart - 1;
                                int prevLineStart = txtPitch.Text.LastIndexOf(Environment.NewLine, prevLineEnd - Environment.NewLine.Length) + Environment.NewLine.Length;
                                txtPitch.Text = txtPitch.Text.Substring(0, prevLineStart) + txtToModify + Environment.NewLine + txtPitch.Text.Substring(txtPitch.SelectionStart + txtPitch.SelectionLength + Environment.NewLine.Length);
                                Simulate();
                                picMain_MouseDown(this, new MouseEventArgs(MouseButtons.Left, 1, simulator.plotter.prevCheckFrame, 0, 0));
                            }
                        }
                    }
                }
            }
        }

        private void txtMotor_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta != 0)
            {
                if (txtMotor.SelectedText != "" && (this.isShiftDown || this.isCtrlDown))
                {
                    txtMotor.SelectedText = AdjustTimeValue(txtMotor.SelectedText, e.Delta, this.isCtrlDown ? 1 : (this.isShiftDown ? 50 : 10));
                    Simulate();
                    picMain_MouseDown(this, new MouseEventArgs(MouseButtons.Left, 1, simulator.plotter.prevCheckFrame, 0, 0));
                }
            }
        }

        private void txtMotor_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                if (txtMotor.SelectedText != "")
                {
                    if (this.isCtrlDown)
                    {
                        // Split to two lines
                        Plotter.PlotData data = plotter.GetPlotData(plotter.prevCheckFrame);
                        if (data != null)
                        {
                            float time = data.time;
                            MotorInfo mi = simulator.GetMotorInfo(time);
                            float time1 = time - mi.timeStart;
                            float time2 = mi.timeEnd - time;
                            string txtToCopy = txtMotor.SelectedText;
                            string txtCopied = txtToCopy;
                            txtToCopy = time1.ToString() + txtToCopy.Substring(txtToCopy.IndexOf(','));
                            txtCopied = time2.ToString() + txtCopied.Substring(txtCopied.IndexOf(','));
                            txtMotor.Text = txtMotor.Text.Substring(0, txtMotor.SelectionStart) + txtToCopy + Environment.NewLine + txtCopied + txtMotor.Text.Substring(txtMotor.SelectionStart + txtMotor.SelectionLength);
                            Simulate();
                            picMain_MouseDown(this, new MouseEventArgs(MouseButtons.Left, 1, simulator.plotter.prevCheckFrame, 0, 0));
                        }
                    }
                    else if (this.isShiftDown)
                    {
                        // Merge to previous line
                        Plotter.PlotData data = plotter.GetPlotData(plotter.prevCheckFrame);
                        if (data != null)
                        {
                            float time = data.time;
                            MotorInfo mi = simulator.GetMotorInfo(time);
                            MotorInfo miPrev = simulator.GetMotorInfo(mi.timeStart - float.Parse(txtDeltaTime.Text));
                            if (miPrev != null && mi != null)
                            {
                                float totalTime = mi.timeEnd - mi.timeStart + miPrev.timeEnd - miPrev.timeStart;
                                string txtToModify = txtMotor.SelectedText;
                                txtToModify = totalTime.ToString() + txtToModify.Substring(txtToModify.IndexOf(','));
                                int prevLineEnd = txtMotor.SelectionStart - 1;
                                int prevLineStart = txtMotor.Text.LastIndexOf(Environment.NewLine, prevLineEnd - Environment.NewLine.Length) + Environment.NewLine.Length;
                                txtMotor.Text = txtMotor.Text.Substring(0, prevLineStart) + txtToModify + Environment.NewLine + txtMotor.Text.Substring(txtMotor.SelectionStart + txtMotor.SelectionLength + Environment.NewLine.Length);
                                Simulate();
                                picMain_MouseDown(this, new MouseEventArgs(MouseButtons.Left, 1, simulator.plotter.prevCheckFrame, 0, 0));
                            }
                        }
                    }
                }
            }
        }

        private void picMain_MouseDown(object sender, MouseEventArgs e)
        {
            picMain.Focus();
            if (simulator != null)
            {
                plotter.OnClick(e.X, e.Y, e.Button);
                simulator.RenderStatistics();
                picMain.Refresh();
                picPlotData.Refresh();
            }
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(GenerateInfo());
        }

        private void btnPaste_Click(object sender, EventArgs e)
        {
            ParseInfo(Clipboard.GetText());
        }

        private string GenerateInfo()
        {
            StringBuilder sb = new StringBuilder();
            
            sb.AppendLine("MRCData");

            sb.AppendLine(txtSubsonicDrag.Text).AppendLine(txtSupersonicDrag.Text).AppendLine(txtInducedDragFactor.Text);
            sb.AppendLine(txtDryMass.Text).AppendLine(txtDiameter.Text);
            sb.AppendLine(txtInitSpeed.Text).AppendLine(txtInitAlt.Text).AppendLine(txtInitAngle.Text).AppendLine(txtCutoffSpeed.Text);
            sb.AppendLine(txtTargetSpeed.Text).AppendLine(txtTargetDistance.Text).AppendLine(txtCLMax.Text);
            sb.AppendLine(txtDeltaTime.Text).AppendLine(txtDisplayScale.Text);
            
            string[] lines = txtMotor.Text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            sb.AppendLine(lines.Length.ToString());
            foreach (string line in lines)
                sb.AppendLine(line);

            lines = txtPitch.Text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            sb.AppendLine(lines.Length.ToString());
            foreach (string line in lines)
                sb.AppendLine(line);
            
            lines = curScriptInfo.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            sb.AppendLine(lines.Length.ToString());
            foreach (string line in lines)
                sb.AppendLine(line);

            lines = curScript.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            sb.AppendLine(lines.Length.ToString());
            foreach (string line in lines)
                sb.AppendLine(line);
            
            return sb.ToString();
        }

        private void ParseInfo(string data)
        {
            string[] lines = data.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            if (lines[0] == "MRCData")
            {
                uint lineIndex = 1;
                txtSubsonicDrag.Text = lines[lineIndex++]; txtSupersonicDrag.Text = lines[lineIndex++]; txtInducedDragFactor.Text = lines[lineIndex++];
                txtDryMass.Text = lines[lineIndex++]; txtDiameter.Text = lines[lineIndex++];
                txtInitSpeed.Text = lines[lineIndex++]; txtInitAlt.Text = lines[lineIndex++]; txtInitAngle.Text = lines[lineIndex++]; txtCutoffSpeed.Text = lines[lineIndex++];
                txtTargetSpeed.Text = lines[lineIndex++]; txtTargetDistance.Text = lines[lineIndex++]; txtCLMax.Text = lines[lineIndex++];
                txtDeltaTime.Text = lines[lineIndex++]; txtDisplayScale.Text = lines[lineIndex++];
                txtMotor.Text = "";
                uint motorLineCount = uint.Parse(lines[lineIndex++]);
                for(uint i = 0; i < motorLineCount; ++i)
                {
                    txtMotor.Text += lines[lineIndex++] + (i < motorLineCount - 1 ? Environment.NewLine : "");
                }
                txtPitch.Text = "";
                uint pitchLineCount = uint.Parse(lines[lineIndex++]);
                for (uint i = 0; i < pitchLineCount; ++i)
                {
                    txtPitch.Text += lines[lineIndex++] + (i < pitchLineCount - 1 ? Environment.NewLine : "");
                }
                uint scriptInfoLineCount = uint.Parse(lines[lineIndex++]);
                curScriptInfo = "";
                for (uint i = 0; i < scriptInfoLineCount; ++i)
                {
                    curScriptInfo += lines[lineIndex++] + (i < scriptInfoLineCount - 1 ? Environment.NewLine : "");
                }
                uint scriptLineCount = uint.Parse(lines[lineIndex++]);
                curScript = "";
                for (uint i = 0; i < scriptLineCount; ++i)
                {
                    curScript += lines[lineIndex++] + (i < scriptLineCount - 1 ? Environment.NewLine : "");
                }
                CompileScript();
                if (formScriptEditor != null)
                {
                    formScriptEditor.SetScriptData(curScript, curScriptInfo, curScriptErrors);
                }
            }
        }

        private void CompileScript()
        {
            if (curScript == null || curScript == "")
            {
                curScriptModule = null;
                curScriptInstance = null;
                return;
            }

            curScriptModule = new ScriptModule(curScript, new string[] { "MissileRangeCalculator.exe" }, "MRCScript");
            if (curScriptModule.GetErrors() != null)
            {
                curScriptErrors = curScriptModule.GetErrors();
            }
            else
            {
                curScriptErrors = null;
                curScriptInstance = new ScriptInstance(curScriptModule, false);
            }

        }

        public void ShowMotorAndPitchStage(float time)
        {
            int motorIndex = -1;
            int angleRateIndex = -1;
            List<MotorInfo> motorInfo = MotorInfo.AnalyzeMotorInfo(txtMotor.Text);
            List<AngleInfo> angleRateInfo = AngleInfo.AnalyzeAngleInfo(txtPitch.Text);
            for (int i = 0; i < motorInfo.Count; ++i)
            {
                if (motorInfo[i].timeStart <= time && motorInfo[i].timeEnd > time)
                {
                    motorIndex = i;
                    break;
                }
            }
            for (int i = 0; i < angleRateInfo.Count; ++i)
            {
                if (angleRateInfo[i].timeStart <= time && angleRateInfo[i].timeEnd > time)
                {
                    angleRateIndex = i;
                    break;
                }
            }
            if (motorIndex >= 0)
            {
                int lineStart = txtMotor.GetFirstCharIndexFromLine(motorIndex);
                if (motorIndex < motorInfo.Count - 1)
                {
                    int lineEnd = txtMotor.GetFirstCharIndexFromLine(motorIndex + 1) - Environment.NewLine.Length;
                    txtMotor.Select(lineStart, lineEnd - lineStart);
                }
                else
                {
                    txtMotor.Select(lineStart, txtMotor.Text.Length - lineStart);
                }
            }
            else
            {
                txtMotor.DeselectAll();
            }
            if (angleRateIndex >= 0)
            {
                int lineStart = txtPitch.GetFirstCharIndexFromLine(angleRateIndex);
                if (angleRateIndex < angleRateInfo.Count - 1)
                {
                    int lineEnd = txtPitch.GetFirstCharIndexFromLine(angleRateIndex + 1) - Environment.NewLine.Length;
                    txtPitch.Select(lineStart, lineEnd - lineStart);
                }
                else
                {
                    txtPitch.Select(lineStart, txtPitch.Text.Length - lineStart);
                }
            }
            else
            {
                txtPitch.DeselectAll();
            }
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (simulator != null && (keyData == Keys.Left || keyData == Keys.Right))
            {
                plotter.OnSlide(keyData == Keys.Left ? -1 : 1);
                simulator.RenderStatistics();
                picMain.Refresh();
                picPlotData.Refresh();
                return false;
            }
            else if (simulator != null && (keyData == Keys.Up || keyData == Keys.Down))
            {
                plotter.OnSlide(keyData == Keys.Up ? -10 : 10);
                simulator.RenderStatistics();
                picMain.Refresh();
                picPlotData.Refresh();
                return false;
            }
            else
            {
                return base.ProcessDialogKey(keyData);
            }
        }

        private void FormMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (this.isShiftDown == false && e.KeyValue == 16)
                this.isShiftDown = true;
            if (this.isCtrlDown == false && e.KeyValue == 17)
                this.isCtrlDown = true;
        }

        private void FormMain_KeyUp(object sender, KeyEventArgs e)
        {
            if (this.isShiftDown == true && e.KeyValue == 16)
                this.isShiftDown = false;
            if (this.isCtrlDown == true && e.KeyValue == 17)
                this.isCtrlDown = false;
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            var dialogResult = openFileDialog.ShowDialog();
            if (dialogResult == DialogResult.OK)
            {
                saveFileDialog.FileName = openFileDialog.FileName;
                this.Text = "Missile Range Calculator - " + saveFileDialog.FileName;
                ParseInfo(File.ReadAllText(openFileDialog.FileName));
                this.Simulate();
            }
        }

        private void btnSaveAs_Click(object sender, EventArgs e)
        {
            var dialogResult = saveFileDialog.ShowDialog();
            if (dialogResult == DialogResult.OK)
            {
                File.WriteAllText(saveFileDialog.FileName, GenerateInfo());
                openFileDialog.FileName = saveFileDialog.FileName;
                this.Text = "Missile Range Calculator - " + saveFileDialog.FileName;
            }
        }

        private void FormMain_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length == 1)
                    e.Effect = DragDropEffects.Copy;
                else
                    e.Effect = DragDropEffects.None;
            }
        }

        private void FormMain_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length == 1)
            {
                ParseInfo(File.ReadAllText(files[0]));
                saveFileDialog.FileName = files[0];
                this.Text = "Missile Range Calculator - " + files[0];
            }
        }

        bool isResizing = false;
        bool isMoving = true;

        private void FormMain_ResizeBegin(object sender, EventArgs e)
        {
            isResizing = true;
            isMoving = true;
        }

        private void FormMain_ResizeEnd(object sender, EventArgs e)
        {
            isResizing = false;
            // Call OnResizeCompleted() when dragging window border is completed.
            if (isMoving == false)
                OnResizeCompleted();
        }

        private void FormMain_Resize(object sender, EventArgs e)
        {
            // If it's dragging window border, do not call OnResizeCompleted().
            isMoving = false;
            if (isResizing) return;
            if (this.WindowState == FormWindowState.Minimized) return;
            OnResizeCompleted();
        }

        private void OnResizeCompleted()
        {
            if (this.simulator != null)
            {
                int lastCheckFrame = -1;
                if (this.simulator.plotter != null)
                {
                    lastCheckFrame = this.simulator.plotter.prevCheckFrame;
                }
                Simulate();
                if (lastCheckFrame != -1)
                {
                    this.picMain_MouseDown(this, new MouseEventArgs(MouseButtons.Left, 1, lastCheckFrame, 1, 0));
                }
            }
        }


        FormScriptEditor formScriptEditor = null;
        private void btnOpenScript_Click(object sender, EventArgs e)
        {
            if (formScriptEditor == null)
            {
                formScriptEditor = new FormScriptEditor();
                formScriptEditor.SetScriptData(curScript, curScriptInfo, curScriptErrors);
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                formScriptEditor.SetAssemblyTreeView(curScriptModule == null ? null : curScriptModule.GetCompiledAssembly(), assemblies);
                formScriptEditor.Show(this);
            }
            else
            {
                formScriptEditor.Show(this);
            }
        }

        public enum ScriptEditorOperation
        {
            Apply = 1,
            Close = 2,
        }

        public void ScriptEditorCallback(ScriptEditorOperation operation)
        {
            switch(operation)
            {
                case ScriptEditorOperation.Apply:
                    bool scriptChanged = (curScript != formScriptEditor.GetScript());
                    if (scriptChanged)
                    {
                        curScript = formScriptEditor.GetScript();
                        CompileScript();
                        formScriptEditor.SetScriptErrors(curScriptErrors);

                        Assembly compiledAssembly = null;
                        if(curScriptErrors == null)
                            compiledAssembly = curScriptModule.GetCompiledAssembly();
                        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                        formScriptEditor.SetAssemblyTreeView(compiledAssembly, assemblies);
                    }
                    curScriptInfo = formScriptEditor.GetScriptInfo();
                    break;
                case ScriptEditorOperation.Close:
                    formScriptEditor.Dispose();
                    formScriptEditor = null;
                    break;
            }
        }
    }
}
