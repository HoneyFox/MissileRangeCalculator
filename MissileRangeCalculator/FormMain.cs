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

namespace MissileRangeCalculator
{
    public partial class FormMain : Form
    {
        Plotter plotter;
        internal Simulator simulator;

        internal bool isShiftDown;
        internal bool isCtrlDown;

        public FormMain()
        {
            InitializeComponent();
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
                float.Parse(txtTargetSpeed.Text), float.Parse(txtTargetDistance.Text), angleRateInfo, float.Parse(txtCutoffSpeed.Text));

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
            string[] lines = txtMotor.Text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            sb.AppendLine(lines.Length.ToString());
            foreach (string line in lines)
                sb.AppendLine(line);
            lines = txtPitch.Text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            sb.AppendLine(lines.Length.ToString());
            foreach (string line in lines)
                sb.AppendLine(line);
            return sb.ToString();
        }

        private void ParseInfo(string data)
        {
            string[] lines = data.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
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
    }

    public class MotorInfo
    {
        public static List<MotorInfo> AnalyzeMotorInfo(string text)
        {
            List<MotorInfo> motorInfo = new List<MotorInfo>();

            float timeElapsed = 0f;
            float totalPropellantMass = 0f;

            string[] lines = text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < lines.Length; ++i)
            {
                if (lines[i].Contains("//"))
                {
                    lines[i] = lines[i].Split(new string[] { "//" }, StringSplitOptions.None)[0].TrimEnd(' ');
                }
            }

            for (int i = 0; i < lines.Length; ++i)
            {
                string[] components = lines[i].Split(',');
                float propellantMass = float.Parse(components[1]);
                totalPropellantMass += propellantMass;
            }

            for (int i = 0; i < lines.Length; ++i)
            {
                string[] components = lines[i].Split(',');
                float time = float.Parse(components[0]);
                float propellantMass = float.Parse(components[1]);
                float thrustStart, thrustEnd;
                if (components[2].Contains('~'))
                {
                    string[] isps = components[2].Split('~');
                    float ispStart = float.Parse(isps[0]);
                    float ispEnd = float.Parse(isps[1]);
                    thrustStart = propellantMass / time * ispStart * 9.81f;
                    thrustEnd = propellantMass / time * ispEnd * 9.81f;
                }
                else
                {
                    float isp = float.Parse(components[2]);
                    thrustStart = thrustEnd = propellantMass / time * isp * 9.81f;
                }

                motorInfo.Add(new MotorInfo(timeElapsed, timeElapsed + time, thrustStart, thrustEnd, totalPropellantMass, totalPropellantMass - propellantMass));
                timeElapsed += time;
                totalPropellantMass -= propellantMass;
            }

            return motorInfo;
        }
        
        public float timeStart;
        public float timeEnd;
        public float thrustStart;
        public float thrustEnd;
        public float propellantMassStart;
        public float propellantMassEnd;

        public MotorInfo(float timeStart, float timeEnd, float thrustStart, float thrustEnd, float propellantMassStart, float propellantMassEnd)
        {
            this.timeStart = timeStart;
            this.timeEnd = timeEnd;
            this.thrustStart = thrustStart;
            this.thrustEnd = thrustEnd;
            this.propellantMassStart = propellantMassStart;
            this.propellantMassEnd = propellantMassEnd;
        }
    }

    public class AeroInfo
    {
        public static List<AeroInfo> AnalyzeAeroInfo(string text, float[] defaultValues)
        {
            List<AeroInfo> aeroInfo = new List<AeroInfo>();

            float timeElapsed = 0f;

            float cdSubsonicOverride = defaultValues[0];
            float cdSupersonicOverride = defaultValues[1];
            float cdLOverride = defaultValues[2];
            float diameter = defaultValues[3];
            float clMaxOverride = defaultValues[4];

            string[] lines = text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < lines.Length; ++i)
            {
                if (lines[i].Contains("//"))
                {
                    lines[i] = lines[i].Split(new string[] { "//" }, StringSplitOptions.None)[0].TrimEnd(' ');
                }
            }

            for (int i = 0; i < lines.Length; ++i)
            {
                string[] components = lines[i].Split(',');
                float time = float.Parse(components[0]);

                if (components.Length > 3 && components[3] != "")
                    cdSubsonicOverride = float.Parse(components[3]);
                if (components.Length > 4 && components[4] != "")
                    cdSupersonicOverride = float.Parse(components[4]);
                if (components.Length > 5 && components[5] != "")
                    cdLOverride = float.Parse(components[5]);
                if (components.Length > 6 && components[6] != "")
                    diameter = float.Parse(components[6]);
                if (components.Length > 7 && components[7] != "")
                    clMaxOverride = float.Parse(components[7]);

                aeroInfo.Add(new AeroInfo(timeElapsed, timeElapsed + time, cdSubsonicOverride, cdSupersonicOverride, cdLOverride, diameter, clMaxOverride));
                timeElapsed += time;
            }

            if (aeroInfo.Count == 0)
            {
                aeroInfo.Add(new AeroInfo(0f, float.MaxValue, cdSubsonicOverride, cdSupersonicOverride, cdLOverride, diameter, clMaxOverride));
            }

            return aeroInfo;
        }

        public float timeStart;
        public float timeEnd;
        public float cdSubsonicOverride;
        public float cdSupersonicOverride;
        public float cdLOverride;
        public float diameter;
        public float clMaxOverride;

        public AeroInfo(float timeStart, float timeEnd, float cdSubsonicOverride, float cdSupersonicOverride, float cdLOverride, float diameter, float clMaxOverride)
        {
            this.timeStart = timeStart;
            this.timeEnd = timeEnd;
            this.cdSubsonicOverride = cdSubsonicOverride;
            this.cdSupersonicOverride = cdSupersonicOverride;
            this.cdLOverride = cdLOverride;
            this.diameter = diameter;
            this.clMaxOverride = clMaxOverride;
    }
    }

    public class AngleInfo
    {
        public static List<AngleInfo> AnalyzeAngleInfo(string text)
        {
            List<AngleInfo> angleInfo = new List<AngleInfo>();

            float timeElapsed = 0f;

            string[] lines = text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < lines.Length; ++i)
            {
                if (lines[i].Contains("//"))
                {
                    lines[i] = lines[i].Split(new string[] { "//" }, StringSplitOptions.None)[0].TrimEnd(' ');
                }
            }

            for (int i = 0; i < lines.Length; ++i)
            {
                string[] components = lines[i].Split( new char[] { ',' }, StringSplitOptions.None);
                float time = float.Parse(components[0]);
                bool useLiftG = components[1].EndsWith("g", StringComparison.InvariantCultureIgnoreCase);

                bool useTargetAngle = false;
                float targetAngle = 0;
                if (components.Length >= 3)
                {
                    if(components[2] != "")
                    {
                        useTargetAngle = true;
                        targetAngle = float.Parse(components[2]);
                    }
                }

                float engineStartAngle = 0;
                float engineEndAngle = 0;
                if (components.Length == 4)
                {
                    if (components[3].Contains('~'))
                {
                        var range = components[3].Split('~');
                        engineStartAngle = float.Parse(range[0]);
                        engineEndAngle = float.Parse(range[1]);
                    }
                    else
                    {
                        engineStartAngle = engineEndAngle = float.Parse(components[3]);
                    }
                }

                if (useLiftG)
                {
                    float liftGMin;
                    float liftGMax;
                    if (components[1].Contains('~'))
                    {
                        if (useTargetAngle == false) MessageBox.Show("G range is meanless when no target angle is provided.");
                        var range = components[1].Split('~');
                        liftGMin = float.Parse(range[0].TrimEnd('g', 'G'));
                        liftGMax = float.Parse(range[1].TrimEnd('g', 'G'));
                    }
                    else
                    {
                        liftGMax = Math.Abs(float.Parse(components[1].TrimEnd('g', 'G')));
                        liftGMin = -liftGMax;
                    }
                    angleInfo.Add(new AngleInfo(timeElapsed, timeElapsed + time, 0, true, liftGMin, liftGMax, useTargetAngle, targetAngle, engineStartAngle, engineEndAngle));
                }
                else
                {
                    angleInfo.Add(new AngleInfo(timeElapsed, timeElapsed + time, float.Parse(components[1]), false, 0, 0, useTargetAngle, targetAngle, engineStartAngle, engineEndAngle));
                }
                timeElapsed += time;
            }

            return angleInfo;
        }

        public float timeStart;
        public float timeEnd;
        public float angleRate;
        public bool useLiftG;
        public float liftGMin;
        public float liftGMax;
        public bool useTargetAngle;
        public float targetAngle;
        public float engineStartAngle;
        public float engineEndAngle;

        public AngleInfo(float timeStart, float timeEnd, float angleRate, bool useLiftG, float liftGMin, float liftGMax, bool useTargetAngle, float targetAngle, float engineStartAngle, float engineEndAngle)
        {
            this.timeStart = timeStart;
            this.timeEnd = timeEnd;
            this.angleRate = angleRate;
            this.useLiftG = useLiftG;
            this.liftGMin = liftGMin;
            this.liftGMax = liftGMax;
            this.useTargetAngle = useTargetAngle;
            this.targetAngle = targetAngle;
            this.engineStartAngle = engineStartAngle;
            this.engineEndAngle = engineEndAngle;
        }
    }

    public class Simulator
    {
        public Plotter plotter;

        float deltaTime;
        float accuracy;
        float cd0(float time)
        {
            for (int i = 0; i < aeroInfo.Count; ++i)
            {
                if (aeroInfo[i].timeStart <= time && aeroInfo[i].timeEnd > time)
                {
                    return aeroInfo[i].cdSubsonicOverride;
                }
            }
            return aeroInfo[aeroInfo.Count - 1].cdSubsonicOverride;
        }
        float cd1(float time)
        {
            for (int i = 0; i < aeroInfo.Count; ++i)
            {
                if (aeroInfo[i].timeStart <= time && aeroInfo[i].timeEnd > time)
                {
                    return aeroInfo[i].cdSupersonicOverride;
                }
            }
            return aeroInfo[aeroInfo.Count - 1].cdSupersonicOverride;
        }
        float idFactor(float time)
        {
            for (int i = 0; i < aeroInfo.Count; ++i)
            {
                if (aeroInfo[i].timeStart <= time && aeroInfo[i].timeEnd > time)
                {
                    return aeroInfo[i].cdLOverride;
                }
            }
            return aeroInfo[aeroInfo.Count - 1].cdLOverride;
        }
        float maxLiftCoeff(float time)
        {
            for (int i = 0; i < aeroInfo.Count; ++i)
            {
                if (aeroInfo[i].timeStart <= time && aeroInfo[i].timeEnd > time)
                {
                    return aeroInfo[i].clMaxOverride;
                }
            }
            return aeroInfo[aeroInfo.Count - 1].clMaxOverride;
        }
        List<MotorInfo> motorInfo;
        List<AeroInfo> aeroInfo;
        float dryMass;
        float refArea(float time)
        {
            for (int i = 0; i < aeroInfo.Count; ++i)
            {
                if (aeroInfo[i].timeStart <= time && aeroInfo[i].timeEnd > time)
                {
                    float diameter = aeroInfo[i].diameter;
                    return (float)(diameter * diameter * Math.PI * 0.25) * 1.414f;
                }
            }
            float finalDiameter = aeroInfo[aeroInfo.Count - 1].diameter;
            return (float)(finalDiameter * finalDiameter * Math.PI * 0.25) * 1.414f;
        }
        float initSpeed;
        float initAngle;
        float initAlt;
        float targetSpeed;
        float targetDistance;
        List<AngleInfo> angleRateInfo;
        float cutoffSpeed;

        float maxThrustTime;

        public Simulator(Plotter plotter, float deltaTime, float accuracy, float cd0, float cd1, float idFactor, float maxLiftCoeff, List<MotorInfo> motorInfo, List<AeroInfo> aeroInfo, float dryMass, float diameter,
            float initSpeed, float initAngle, float initAlt, float targetSpeed, float targetDistance, List<AngleInfo> angleRateInfo, float cutoffSpeed)
        {
            this.plotter = plotter;
            this.deltaTime = deltaTime;
            this.accuracy = accuracy;
            //this.cd0 = cd0;
            //this.cd1 = cd1;
            //this.idFactor = idFactor;
            //this.maxLiftCoeff = maxLiftCoeff;
            this.motorInfo = motorInfo;
            this.aeroInfo = aeroInfo;
            this.dryMass = dryMass;
            //this.refArea = (float)(diameter * diameter * Math.PI * 0.25) * 1.414f;
            this.initSpeed = initSpeed;
            this.initAngle = initAngle;
            this.initAlt = initAlt;
            this.targetSpeed = targetSpeed;
            this.targetDistance = targetDistance;
            this.angleRateInfo = angleRateInfo;
            this.cutoffSpeed = cutoffSpeed;

            this.maxThrustTime = motorInfo.Count > 0 ? motorInfo[motorInfo.Count - 1].timeEnd : 0f;
        }

        public static float Lerp(float start, float end, float t)
        {
            return start + (end - start) * t;
        }

        public static float Unlerp(float start, float end, float c)
        {
            return (c - start) / (end - start);
        }

        public static float Clamp(float value, float min, float max)
        {
            return (value < min ? min : (value > max ? max : value));
        }

        public static float MoveTowards(float value, float target, float maxStep)
        {
            if (target == value && maxStep != 0)
                throw new InvalidOperationException("Something goes wrong with the logic.");
            if ((target - value) * (target - (value + maxStep)) <= 0)
                return target;
            else
                return value + maxStep;
        }

        public MotorInfo GetMotorInfo(float time)
        {
            for (int i = 0; i < motorInfo.Count; ++i)
            {
                if (motorInfo[i].timeStart <= time && motorInfo[i].timeEnd > time)
                {
                    return motorInfo[i];
                }
            }

            return null;
        }

        public AngleInfo GetAngleInfo(float time)
        {
            for (int i = 0; i < angleRateInfo.Count; ++i)
            {
                if (angleRateInfo[i].timeStart <= time && angleRateInfo[i].timeEnd > time)
                {
                    return angleRateInfo[i];
                }
            }

            return null;
        }

        public float GetThrust(float time)
        {
            for (int i = 0; i < motorInfo.Count; ++i)
            {
                if (motorInfo[i].timeStart <= time && motorInfo[i].timeEnd > time)
                {
                    return motorInfo[i].thrustStart + (motorInfo[i].thrustEnd - motorInfo[i].thrustStart) * (time - motorInfo[i].timeStart) / (motorInfo[i].timeEnd - motorInfo[i].timeStart);
                }
            }

            return 0f;
        }

        public float GetEngineAngle(float time)
        {
            for (int i = 0; i < angleRateInfo.Count; ++i)
            {
                if (angleRateInfo[i].timeStart <= time && angleRateInfo[i].timeEnd > time)
                {
                    return angleRateInfo[i].engineStartAngle + (angleRateInfo[i].engineEndAngle - angleRateInfo[i].engineStartAngle) * (time - angleRateInfo[i].timeStart) / (angleRateInfo[i].timeEnd - angleRateInfo[i].timeStart);
                }
            }

            return 0f;
        }

        public float GetMass(float time)
        {
            for (int i = 0; i < motorInfo.Count; ++i)
            {
                if (motorInfo[i].timeStart <= time && motorInfo[i].timeEnd > time)
                {
                    float t = Unlerp(motorInfo[i].timeStart, motorInfo[i].timeEnd, time);
                    float currentPropellantMass = Lerp(motorInfo[i].propellantMassStart, motorInfo[i].propellantMassEnd, t);
                    return dryMass + currentPropellantMass;
                }
            }

            return dryMass;
        }

        public double GetLocalG()
        {
            const double earthRadius = 6341620f;
            return Math.Pow(earthRadius / (curAlt + earthRadius), 2) * 9.81;
        }

        public double GetNetG()
        {
            double horSpeed = curSpeed * Math.Cos(curAngle * Math.PI / 180f);
            const double earthRadius = 6341620f;
            double centrifugalAcc = horSpeed * horSpeed / earthRadius;
            return GetLocalG() - centrifugalAcc;
        }
        
        public float GetMaxLiftForce(float time)
        {
            float dynPressure = GetDynPressure(curSpeed, curAlt);
            return maxLiftCoeff(time) * refArea(time) * dynPressure;
        }

        public float UpdatePitchAngle(float time, float deltaTime, float mass)
        {
            float accForStraightFlight = (float)(Math.Cos(curAngle * Math.PI / 180f) * GetNetG());
            for(int i = 0; i < angleRateInfo.Count; ++i)
            {
                if(angleRateInfo[i].timeStart <= time && angleRateInfo[i].timeEnd > time)
                {
                    if (curSpeed > 0)
                    {
                        float originalAngle = curAngle;
                        float gravityAngleRate = (float)(-accForStraightFlight / curSpeed * 180 / Math.PI) + (float)(Math.Sin(GetEngineAngle(time) * Math.PI / 180f) * GetThrust(time) / mass / curSpeed * 180 / Math.PI);
                        curAngle += gravityAngleRate * deltaTime;
                                
                        float maxLiftAcc = GetMaxLiftForce(time) / mass;

                        float liftAccRequired;
                        if (angleRateInfo[i].useLiftG == false)
                        {
                            if (angleRateInfo[i].useTargetAngle)
                                liftAccRequired = (float)((angleRateInfo[i].angleRate - gravityAngleRate) * Math.Sign(angleRateInfo[i].targetAngle - curAngle) * Math.PI / 180 * curSpeed);
                            else
                                liftAccRequired = (float)((angleRateInfo[i].angleRate - gravityAngleRate) * Math.PI / 180) * curSpeed;
                        }
                        else
                        {
                            if (angleRateInfo[i].useTargetAngle)
                            {
                                if (angleRateInfo[i].targetAngle >= curAngle)
                                    liftAccRequired = angleRateInfo[i].liftGMax * 9.81f;
                                else
                                    liftAccRequired = angleRateInfo[i].liftGMin * 9.81f;
                            }
                            else
                            {
                                liftAccRequired = angleRateInfo[i].liftGMax * 9.81f;
                            }
                        }
                        float actualLiftAcc = Clamp(liftAccRequired, -maxLiftAcc, maxLiftAcc);
                        float angleRate = actualLiftAcc / curSpeed;
                        if (angleRateInfo[i].useTargetAngle)
                        {
                            float result = MoveTowards(curAngle, angleRateInfo[i].targetAngle, (float)(angleRate * 180 / Math.PI) * deltaTime);
                            curAngle = originalAngle;
                            return result;
                        }
                        else
                        {
                            float result = curAngle + (float)(angleRate * 180 / Math.PI) * deltaTime;
                            curAngle = originalAngle;
                            return result;
                        }
                    }
                    else
                    {
                        return curAngle;
                    }
                }
            }

            // Go ballistic if no pitch command exists.
            return curAngle + (float)(-accForStraightFlight / curSpeed * 180f / Math.PI) * deltaTime;
        }

        public float GetDragCoeff(float mach, float time)
        {
            float Cd0 = 0f;
            if (mach <= 1)
            {
                Cd0 = (float)(Math.Sin(Math.Pow(mach, 10.0) * 3.1415926 + 1.5 * 3.1415926) * (cd1(time) - cd0(time)) / 2.0 + cd0(time) + (cd1(time) - cd0(time)) / 2.0);
            }
            else
            {
                Cd0 = (float)(Math.Sin(Math.Pow(mach, -0.75) * 3.1415926 - 0.5 * 3.1415926) * (cd1(time) * 2.0 - cd0(time)) / 4.0 + cd0(time) / 2.0 + (cd1(time) * 2.0 - cd0(time)) / 4.0);
            }

            return Cd0;
        }

        public static float TAStoIAS(float TAS, double alt)
        {
            float rho0 = 1.225f;
            float rho = GetAirDensity(alt);
            return TAS * (float)(Math.Sqrt(rho / rho0));
        }

        public static float GetAirDensity(double alt)
        {
            float rho0 = 1.225f;
            float T0 = 288.15f;
            if (alt <= 11000)
            {
                float T = T0 - (float)(0.0065f * alt);
                return rho0 * (float)(Math.Pow(T / T0, 4.25588));
            }
            else if (alt > 11000 && alt <= 20000)
            {
                return 0.36392f * (float)(Math.Exp((11000 - alt) / 6341.62));
            }
            else if(alt <= 40000)
            {
                float T = 216.65f + (float)(0.001f * (alt - 20000));
                return 0.088035f * (float)(Math.Pow(T / 216.65, -35.1632));
            }
            else if(alt <= 120000)
            {
                return 0.003946607f * (float)(Math.Pow(0.5, (alt - 40000) / 6000));
            }
            else
            {
                return 0f;
            }
        }

        public static float GetSonicSpeed(double alt)
        {
            float T0 = 288.15f;
            float T;
            if (alt <= 11000)
            {
                T = T0 - (float)(0.0065f * alt);
            }
            else if (alt > 11000 && alt <= 20000)
            {
                T = T0 - 0.0065f * 11000f;
            }
            else
            {
                T = T0 - 0.0065f * 11000f + (float)(0.001f * (alt - 20000));
            }
            return 331.3f + 0.606f * (T - 273.15f);
        }

        public static float GetDynPressure(float TAS, float alt)
        {
            float airDensity = GetAirDensity(alt);
            if (float.IsNaN(airDensity)) throw new Exception("NAN!");
            return 0.5f * airDensity * TAS * TAS;
        }

        public static float TAStoMach(float TAS, float alt)
        {
            float sonicSpeed = GetSonicSpeed(alt);
            return TAS / sonicSpeed;
        }

        public static float TAStoGS(float TAS, float alt)
        {
            float earthRadius = 6341.62f;
            return TAS * (earthRadius / (alt * 0.001f + earthRadius));
        }

        public float CalculateDrag(float angleRate, float mass, float time, out float liftAcc, out float liftCoeff)
        {
            float dynPressure = GetDynPressure(curSpeed, curAlt);
            float drag0 = GetDragCoeff(TAStoMach(curSpeed, curAlt), time) * refArea(time) * dynPressure;
            float accForStraightFlight = (float)(Math.Cos(curAngle * Math.PI / 180f) * GetNetG());
            float curAcc = angleRate * curSpeed;
            liftAcc = (curAcc + accForStraightFlight - (float)(Math.Sin(GetEngineAngle(curTime) * Math.PI / 180f) * GetThrust(curTime) / mass));
            float liftForce = liftAcc * mass;
            float dragL = 0f;
            if (dynPressure > 0)
            {
                liftCoeff = liftForce / refArea(time) / dynPressure;
                float cdL = liftCoeff * liftCoeff * idFactor(time);
                dragL = cdL * refArea(time) * dynPressure;
            }
            else
            {
                liftCoeff = 0f;
            }
            return drag0 + dragL;
        }

        int curFrame;
        float curTime;
        float curMass;
        float curAlt;
        float curSpeed;
        float curAcc;
        float curAngle;
        float curLiftAcc;
        float curDragAcc;
        float curCLReq;
        float curHorDistance;
        float curHorDistance39;

        float curTargetDistance1;
        float curTargetDistance2;
        float curTargetDistance39;
        
        float maxTAS;
        float maxMach;
        float maxAlt;

        public void UpdateFrame(float deltaTime)
        {
            curMass = GetMass(curTime);
            float newAngle = UpdatePitchAngle(curTime, deltaTime, curMass);
            float deltaAngle = newAngle - curAngle;
            float engineAngle = GetEngineAngle(curTime);

            float curDrag = CalculateDrag((float)(deltaAngle / deltaTime * Math.PI / 180f), curMass, curTime, out curLiftAcc, out curCLReq);
            curDragAcc = curDrag / curMass;
            curAcc = (GetThrust(curTime) * (float)Math.Cos(engineAngle * Math.PI / 180f) - curDrag) / curMass - (float)(GetLocalG() * Math.Sin(curAngle * Math.PI / 180f));
            curAngle = newAngle;
            curSpeed = curSpeed + curAcc * deltaTime;
            float curHorSpeed = curSpeed * (float)(Math.Cos(curAngle * Math.PI / 180f));
            float curHorGS = TAStoGS(curHorSpeed, curAlt);
            //curSpeed *= (float)Math.Cos(curHorGS / 6341620.0 * deltaTime);
            curHorDistance += curHorGS * deltaTime;
            curAlt += curSpeed * (float)(Math.Sin(curAngle * Math.PI / 180f)) * deltaTime;
            if (curHorGS > targetSpeed)
                curHorDistance39 += (float)(Math.Sqrt(curHorGS * curHorGS - targetSpeed * targetSpeed) * deltaTime);

            curTargetDistance1 -= targetSpeed * deltaTime;
            curTargetDistance2 += targetSpeed * deltaTime;
        }

        public void Simulate()
        {
            curFrame = 0;
            curTime = 0f;
            curMass = GetMass(0);
            curAlt = initAlt;
            curSpeed = initSpeed;
            curAcc = 0f;
            curAngle = initAngle;
            curHorDistance = curHorDistance39 = 0f;

            curTargetDistance1 = curTargetDistance2 = curTargetDistance39 = targetDistance;

            List<Tuple<double, double>> downRangeData = new List<Tuple<double, double>>();

            while (true)
            {
                for (int i = 0; i < (int)(deltaTime / accuracy); ++i)
                {
                    UpdateFrame(accuracy);
                    curTime += accuracy;
                }
                downRangeData.Add(new Tuple<double, double>(curHorDistance, curAlt));
                plotter.Record(curFrame, curTime, curMass, curHorDistance, curHorDistance39,
                    curAlt, curSpeed, TAStoIAS(curSpeed, curAlt), TAStoMach(curSpeed, curAlt), curAcc, curLiftAcc / 9.81f, (Math.Abs(curLiftAcc) > 0.005 && curDragAcc > 0 ? Math.Abs(curLiftAcc) / curDragAcc : 0.0f), curCLReq, curAngle,
                    curTargetDistance1, curTargetDistance2, curTargetDistance39);
                //curTime += deltaTime;
                curFrame++;

                if (maxTAS < curSpeed) maxTAS = curSpeed;
                if (maxMach < TAStoMach(curSpeed, curAlt)) maxMach = TAStoMach(curSpeed, curAlt);
                if (maxAlt < curAlt) maxAlt = curAlt;

                if (curAlt < 0) break;
                if (curTime > maxThrustTime && curSpeed < cutoffSpeed) break;

                if (curFrame > 9999) break;
            }

            plotter.RenderAllFrames(maxAlt, maxTAS, Math.Max(curTargetDistance2, curHorDistance));

            RenderDownRange(downRangeData);

            RenderStatistics();
        }

        public void RenderDownRange(List<Tuple<double, double>> data)
        {
            if (data.Count <= 1) return;
            plotter.RenderDownRange(data);
        }

        public void RenderStatistics()
        {
            plotter.RenderStatistics(curFrame, curTime, curAlt, curAngle, TAStoMach(curSpeed, curAlt), curSpeed, curHorDistance, curHorDistance39, maxMach, maxTAS, maxAlt);
        }

        public bool IsStagingTime(float time, float prevTime, out bool hasThrust)
        {
            hasThrust = (GetThrust(time) > 0f);
            for(int i = 0; i < motorInfo.Count; ++i)
            {
                if(prevTime < motorInfo[i].timeStart && time >= motorInfo[i].timeStart)
                {
                    return true;
                }
                if(i == motorInfo.Count - 1)
                {
                    if (prevTime < motorInfo[i].timeEnd && time >= motorInfo[i].timeEnd)
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }

        public bool IsTurningTime(float time, float prevTime)
        {
            for (int i = 0; i < angleRateInfo.Count; ++i)
            {
                if (prevTime < angleRateInfo[i].timeStart && time >= angleRateInfo[i].timeStart)
                {
                    return true;
                }
                if (i == angleRateInfo.Count - 1)
                {
                    if (prevTime < angleRateInfo[i].timeEnd && time >= angleRateInfo[i].timeEnd)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }

    public class Plotter
    {
        public class PlotData
        {
            public int frame;
            public float time;
            public float mass;
            public double horDistance;
            public double horDistance39;
            public double alt;
            public float TAS;
            public float IAS;
            public float mach;
            public float acc;
            public float liftG;
            public float ldRatio;
            public float reqCL;
            public float angle;
            public double tgtDistance1;
            public double tgtDistance2;
            public double tgtDistance39;

            public PlotData(int frame, float time, float mass, double horDistance, double horDistance39, double alt, float TAS, float IAS, float mach, float acc, float liftG, float ldRatio, float reqCL, float angle, double tgtDistance1, double tgtDistance2, double tgtDistance39)
            {
                this.frame = frame;
                this.time = time;
                this.mass = mass;
                this.horDistance = horDistance;
                this.horDistance39 = horDistance39;
                this.alt = alt;
                this.TAS = TAS;
                this.IAS = IAS;
                this.mach = mach;
                this.acc = acc;
                this.liftG = liftG;
                this.ldRatio = ldRatio;
                this.reqCL = reqCL;
                this.angle = angle;
                this.tgtDistance1 = tgtDistance1;
                this.tgtDistance2 = tgtDistance2;
                this.tgtDistance39 = tgtDistance39;
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb
                    .Append("Frame ").AppendLine(frame.ToString())
                    .Append("Time ").AppendLine(time.ToString("F2"))
                    .Append("Mass ").AppendLine(mass.ToString("F2"))
                    .Append("Alt ").AppendLine(alt.ToString("F2"))
                    .Append("TAS ").AppendLine(TAS.ToString("F2"))
                    .Append("IAS ").AppendLine(IAS.ToString("F2"))
                    .Append("Mach ").AppendLine(mach.ToString("F2"))
                    .Append("Acc ").AppendLine(acc.ToString("F2"))
                    .Append("LiftG ").AppendLine(liftG.ToString("F2"))
                    .Append("L/D ").AppendLine(ldRatio.ToString("F2"))
                    .Append("ReqCL ").AppendLine(reqCL.ToString("F2"))
                    .Append("Angle ").AppendLine(angle.ToString("F2"))
                    .Append("HDist ").AppendLine(horDistance.ToString("F2"))
                    .Append("HDist39 ").AppendLine(horDistance39.ToString("F2"))
                    .Append("TgtDist ").Append(tgtDistance1.ToString("F2")).Append(" ").AppendLine(tgtDistance2.ToString("F2"))
                    .Append("TgtDist39 ").AppendLine(tgtDistance39.ToString("F2"));

                return sb.ToString();
            }
        }

        FormMain ownerWindow;

        PictureBox target;
        Graphics graphics;
        Graphics textGraphics;
        PictureBox picPlotData;
        Graphics plotDataGraphics;
        PictureBox picLegends;
        Graphics legendsGraphics;
        Font font;
        float scale;
        float cutoffSpeed;

        List<PlotData> plotData = new List<PlotData>();
        List<Tuple<double, double>> downRangeData = new List<Tuple<double, double>>();
        float maxTAS = 0f;
        float maxAlt = 0f;
        float maxDistance = 0f;

        public Plotter(FormMain ownerWindow, PictureBox target, Font font, PictureBox picPlotData, PictureBox picLegends)
        {
            this.ownerWindow = ownerWindow;
            this.target = target;
            this.font = font;
            this.picPlotData = picPlotData;
            this.picLegends = picLegends;
        }

        public PlotData GetPlotData(int frameIndex)
        {
            if (frameIndex >= 0 && frameIndex < plotData.Count)
                return plotData[frameIndex];
            else
                return null;
        }

        public void Clear()
        {
            Image image = new Bitmap(target.Width, target.Height);
            graphics = Graphics.FromImage(image);

            graphics.Clear(target.BackColor);
            graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
            graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;

            graphics.TranslateTransform(0f, target.Height * 1f);
            graphics.ScaleTransform(1f, -1f);

            textGraphics = Graphics.FromImage(image);

            target.Image = image;

            Image plotDataImage = new Bitmap(picPlotData.Width, picPlotData.Height);
            plotDataGraphics = Graphics.FromImage(plotDataImage);
            picPlotData.Image = plotDataImage;

            Image legendsImage = new Bitmap(picLegends.Width, picLegends.Height);
            legendsGraphics = Graphics.FromImage(legendsImage);
            picLegends.Image = legendsImage;

            RenderLegends();

            plotData.Clear();
            downRangeData.Clear();
        }

        public void SetRenderScale(float s)
        {
            this.scale = s;
        }

        public void SetCutoffSpeed(float s)
        {
            this.cutoffSpeed = s;
        }

        public void Record(int frame, float time, float mass, float horDistance, float horDistance39, float alt, float TAS, float IAS, float mach, float acc, float liftG, float ldRatio, float reqCL, float angle, float tgtDistance1, float tgtDistance2, float tgtDistance39)
        {
            var data = new PlotData(frame, time, mass, horDistance, horDistance39, alt, TAS, IAS, mach, acc, liftG, ldRatio, reqCL, angle, tgtDistance1, tgtDistance2, tgtDistance39);
            plotData.Add(data);
        }

        public void Render(int frame)
        {
            var data = plotData[frame];
            if (frame > 0)
            {
                RenderPlotData(data, plotData[frame - 1]);
            }
            else
            {
                RenderPlotData(data, null);
            }
        }

        public void RenderPlotData(PlotData data, PlotData prevData)
        {
            graphics.DrawLine(Pens.Red, data.frame, 0f, data.frame, data.TAS * 0.4f * scale);
            graphics.DrawLine(Pens.DarkRed, data.frame, 0f, data.frame, data.IAS * 0.4f * scale);
            graphics.DrawLine(Pens.Lime, data.frame, target.Height * 0.667f, data.frame, target.Height * 0.667f + data.acc * 2f * scale);
            graphics.DrawLine(Pens.BlueViolet, data.frame, target.Height * 0.667f, data.frame, target.Height * 0.667f + data.liftG * 20f * scale);
            graphics.DrawLine(Pens.White, data.frame - 1, target.Height * 0.667f, data.frame, target.Height * 0.667f);

            if (prevData != null)
            {
                graphics.DrawLine(Pens.White, data.frame - 1, (float)(prevData.alt * 0.01f * scale), data.frame, (float)(data.alt * 0.01f * scale));
                graphics.DrawLine(Pens.Orange, data.frame - 1, prevData.mach * 100 * scale, data.frame, data.mach * 100 * scale);

                graphics.DrawLine(Pens.Cyan, data.frame - 1, (float)(prevData.tgtDistance1 / maxDistance) * target.Height * 0.9f, data.frame, (float)(data.tgtDistance1 / maxDistance) * target.Height * 0.9f);
                graphics.DrawLine(Pens.Cyan, data.frame - 1, (float)(prevData.tgtDistance2 / maxDistance) * target.Height * 0.9f, data.frame, (float)(data.tgtDistance2 / maxDistance) * target.Height * 0.9f);
                graphics.DrawLine(Pens.Yellow, data.frame - 1, (float)(prevData.tgtDistance39 / maxDistance) * target.Height * 0.9f, data.frame, (float)(data.tgtDistance39 / maxDistance) * target.Height * 0.9f);
                
                graphics.DrawLine(Pens.Blue, data.frame - 1, (float)(prevData.horDistance / maxDistance) * target.Height * 0.9f, data.frame, (float)(data.horDistance / maxDistance) * target.Height * 0.9f);
                graphics.DrawLine(Pens.Yellow, data.frame - 1, (float)(prevData.horDistance39 / maxDistance) * target.Height * 0.9f, data.frame, (float)(data.horDistance39 / maxDistance) * target.Height * 0.9f);

                graphics.DrawLine(Pens.Magenta, data.frame - 1, target.Height * 0.667f + prevData.angle * 1f, data.frame, target.Height * 0.667f + data.angle * 1f);
                graphics.DrawLine(Pens.Black, data.frame - 1, target.Height * 0.667f + prevData.ldRatio * 40f * scale, data.frame, target.Height * 0.667f + data.ldRatio * 40f * scale);
                graphics.DrawLine(Pens.White, data.frame - 1, target.Height * 0.667f, data.frame, target.Height * 0.667f);
            }

            bool hasThrust = false;
            if (this.ownerWindow.simulator != null && this.ownerWindow.simulator.IsStagingTime(data.time, prevData != null ? prevData.time : 0f, out hasThrust))
                graphics.DrawLine(Pens.Orange, data.frame, target.Height * 0.667f + 80f, data.frame, target.Height * 0.667f + 90f);
            else
                if (hasThrust)
                    graphics.DrawLine(Pens.OrangeRed, data.frame, target.Height * 0.667f + 80f, data.frame, target.Height * 0.667f + 90f);
            if (this.ownerWindow.simulator != null && this.ownerWindow.simulator.IsTurningTime(data.time, prevData != null ? prevData.time : 0f))
                graphics.DrawLine(Pens.SkyBlue, data.frame, target.Height * 0.667f - 80f, data.frame, target.Height * 0.667f - 90f);
            graphics.DrawLine(Pens.Orange, data.frame - 1, target.Height * 0.667f + 90f, data.frame, target.Height * 0.667f + 90f);
            graphics.DrawLine(Pens.SkyBlue, data.frame - 1, target.Height * 0.667f - 90f, data.frame, target.Height * 0.667f - 90f);

            if (prevData == null || prevData.time % 10.0 > data.time % 10.0)
            {
                graphics.DrawLine(Pens.Black, data.frame, 0, data.frame, 10);
            }
            else if (prevData == null || prevData.time % 5.0 > data.time % 5.0)
            {
                graphics.DrawLine(Pens.Black, data.frame, 0, data.frame, 5);
            }
            else
            {
                graphics.DrawLine(Pens.Black, data.frame, 0, data.frame, 1);
            }

            if (prevData != null)
            {
                float prevSonicSpeed = Simulator.GetSonicSpeed(prevData.alt);
                float sonicSpeed = Simulator.GetSonicSpeed(data.alt);
                for (int i = 1; i < 6; ++i)
                    graphics.DrawLine(i == 1 ? Pens.DarkGray : Pens.DimGray, data.frame - 1, prevSonicSpeed * i * 0.4f * scale, data.frame, sonicSpeed * i * 0.4f * scale);
            }

            graphics.DrawLine(Pens.Black, data.frame - 1, cutoffSpeed * 0.4f * scale, data.frame, cutoffSpeed * 0.4f * scale);
        }

        public void RenderAllFrames(float maxTAS, float maxAlt, float maxDistance)
        {
            this.maxTAS = maxTAS;
            this.maxAlt = maxAlt;
            this.maxDistance = maxDistance;

            for (int i = 0; i < plotData.Count; ++i)
            {
                Render(i);
            }
        }

        public void RenderDownRange(List<Tuple<double, double>> data)
        {
            downRangeData = data;

            Pen p = new Pen(Color.White, 1.25f);
            p.DashPattern = new float[] { 4.0f, 6.0f };
            Pen p2 = new Pen(Color.FromArgb(128, 255, 255, 255), 1.25f);
            p2.DashPattern = new float[] { 4.0f, 6.0f };

            double maxRange = data[data.Count - 1].Item1;
            List<PointF> dataPoints = new List<PointF>();
            List<PointF> dataPointsUniform = new List<PointF>();
            for (int i = 0; i < data.Count; ++i)
            {
                dataPoints.Add(new PointF((float)(data[i].Item1 / maxRange * data.Count), (float)(data[i].Item2 * 0.01 * scale)));
                dataPointsUniform.Add(new PointF((float)(data[i].Item1 / maxRange * data.Count), (float)(data[i].Item2 / maxRange * data.Count)));
            }

            graphics.DrawCurve(p, dataPoints.ToArray());
            graphics.DrawCurve(p2, dataPointsUniform.ToArray(), 0.5f);
        }

        public void RenderStatistics(int frame, float time, double alt, float angle, float mach, float speed, double horDistance, double horDistance39, float maxMach, float maxSpeed, double maxAlt)
        {
            StringBuilder sb = new StringBuilder();
            sb
                .Append("Frames:").Append(frame.ToString()).Append(" Time:").AppendLine(time.ToString())
                .Append("Alt:").Append(alt.ToString()).Append(" Pitch:").AppendLine(angle.ToString())
                .Append("Mach:").Append(mach.ToString()).Append(" TAS:").AppendLine(speed.ToString())
                .Append("Distance:").Append(horDistance.ToString()).Append(" Distance39:").AppendLine(horDistance39.ToString())
                .Append("MaxMach:").Append(maxMach.ToString()).Append(" MaxTAS:").AppendLine(maxSpeed.ToString())
                .Append("MaxAlt:").AppendLine(maxAlt.ToString());
            string statistics = sb.ToString();

            textGraphics.DrawString(statistics, font, Brushes.White, 5f, 5f);
        }

        public void RenderLegends()
        {
            float y = 5f;
            legendsGraphics.FillRectangle(Brushes.Red, new RectangleF(5f, y, 12f, 12f));
            legendsGraphics.DrawRectangle(Pens.Black, new Rectangle(5, (int)y, 12, 12));
            legendsGraphics.DrawString("TAS", font, Brushes.Black, 20f, y);

            y += 20f;
            legendsGraphics.FillRectangle(Brushes.DarkRed, new RectangleF(5f, y, 12f, 12f));
            legendsGraphics.DrawRectangle(Pens.Black, new Rectangle(5, (int)y, 12, 12));
            legendsGraphics.DrawString("IAS", font, Brushes.Black, 20f, y);

            y += 20f;
            legendsGraphics.FillRectangle(Brushes.Orange, new RectangleF(5f, y, 12f, 12f));
            legendsGraphics.DrawRectangle(Pens.Black, new Rectangle(5, (int)y, 12, 12));
            legendsGraphics.DrawString("Mach", font, Brushes.Black, 20f, y);

            y += 20f;
            legendsGraphics.FillRectangle(Brushes.DarkGray, new RectangleF(5f, y, 12f, 12f));
            legendsGraphics.DrawRectangle(Pens.Black, new Rectangle(5, (int)y, 12, 12));
            legendsGraphics.DrawString("Mach 1", font, Brushes.Black, 20f, y);

            y += 20f;
            legendsGraphics.FillRectangle(Brushes.DimGray, new RectangleF(5f, y, 12f, 12f));
            legendsGraphics.DrawRectangle(Pens.Black, new Rectangle(5, (int)y, 12, 12));
            legendsGraphics.DrawString("Mach Lines", font, Brushes.Black, 20f, y);

            y += 20f;
            legendsGraphics.FillRectangle(Brushes.Lime, new RectangleF(5f, y, 12f, 12f));
            legendsGraphics.DrawRectangle(Pens.Black, new Rectangle(5, (int)y, 12, 12));
            legendsGraphics.DrawString("Acc", font, Brushes.Black, 20f, y);

            y += 20f;
            legendsGraphics.FillRectangle(Brushes.BlueViolet, new RectangleF(5f, y, 12f, 12f));
            legendsGraphics.DrawRectangle(Pens.Black, new Rectangle(5, (int)y, 12, 12));
            legendsGraphics.DrawString("LiftG", font, Brushes.Black, 20f, y);

            y += 20f;
            legendsGraphics.FillRectangle(Brushes.Black, new RectangleF(5f, y, 12f, 12f));
            legendsGraphics.DrawRectangle(Pens.Black, new Rectangle(5, (int)y, 12, 12));
            legendsGraphics.DrawString("Cutoff", font, Brushes.Black, 20f, y);

            y += 20f;
            legendsGraphics.FillRectangle(Brushes.White, new RectangleF(5f, y, 12f, 12f));
            legendsGraphics.DrawRectangle(Pens.Black, new Rectangle(5, (int)y, 12, 12));
            legendsGraphics.DrawString("Alt", font, Brushes.Black, 20f, y);

            y += 20f;
            legendsGraphics.FillRectangle(Brushes.Magenta, new RectangleF(5f, y, 12f, 12f));
            legendsGraphics.DrawRectangle(Pens.Black, new Rectangle(5, (int)y, 12, 12));
            legendsGraphics.DrawString("Pitch Angle", font, Brushes.Black, 20f, y);

            y += 20f;
            legendsGraphics.FillRectangle(Brushes.Blue, new RectangleF(5f, y, 12f, 12f));
            legendsGraphics.DrawRectangle(Pens.Black, new Rectangle(5, (int)y, 12, 12));
            legendsGraphics.DrawString("Hor Distance", font, Brushes.Black, 20f, y);

            y += 20f;
            legendsGraphics.FillRectangle(Brushes.Yellow, new RectangleF(5f, y, 12f, 12f));
            legendsGraphics.DrawRectangle(Pens.Black, new Rectangle(5, (int)y, 12, 12));
            legendsGraphics.DrawString("39 Distance", font, Brushes.Black, 20f, y);

            y += 20f;
            legendsGraphics.FillRectangle(Brushes.Cyan, new RectangleF(5f, y, 12f, 12f));
            legendsGraphics.DrawRectangle(Pens.Black, new Rectangle(5, (int)y, 12, 12));
            legendsGraphics.DrawString("Tgt Distance", font, Brushes.Black, 20f, y);
        }

        internal int prevCheckFrame = -1;
        internal int prevCheckFrameDownRangeX = -1;

        public int OnClick(int x, int y, MouseButtons button = MouseButtons.Left)
        {
            if (x >= 0 && x < plotData.Count)
            {
                if (button == MouseButtons.Right)
                {
                    double horDistance = x * plotData[plotData.Count - 1].horDistance / (plotData.Count - 1);
                    int closestIndex = -1;
                    double closestDistance = double.MaxValue;
                    for (int i = 0; i < plotData.Count; ++i)
                    {
                        double distanceError = Math.Abs(plotData[i].horDistance - horDistance);
                        if (distanceError < closestDistance)
                        {
                            closestIndex = i;
                            closestDistance = distanceError;
                        }
                    }
                    x = closestIndex;
                }

                plotDataGraphics.Clear(Color.DarkBlue);
                plotDataGraphics.DrawString(plotData[x].ToString(), font, Brushes.White, 10f, 10f);

                graphics.DrawLine(new Pen(target.BackColor), prevCheckFrame, 0, prevCheckFrame, target.Height);
                graphics.DrawLine(new Pen(target.BackColor), prevCheckFrameDownRangeX, 0, prevCheckFrameDownRangeX, target.Height);

                if (prevCheckFrame >= 0 && prevCheckFrame < plotData.Count)
                {
                    RenderPlotData(plotData[prevCheckFrame], (prevCheckFrame == 0) ? null : plotData[prevCheckFrame - 1]);
                    if (prevCheckFrame < plotData.Count - 1)
                        RenderPlotData(plotData[prevCheckFrame + 1], (prevCheckFrame == 0) ? null : plotData[prevCheckFrame]);
                }
                if (prevCheckFrameDownRangeX >= 0 && prevCheckFrameDownRangeX < plotData.Count)
                {
                    RenderPlotData(plotData[prevCheckFrameDownRangeX], (prevCheckFrameDownRangeX == 0) ? null : plotData[prevCheckFrameDownRangeX - 1]);
                    if (prevCheckFrameDownRangeX < plotData.Count - 1)
                        RenderPlotData(plotData[prevCheckFrameDownRangeX + 1], (prevCheckFrameDownRangeX == 0) ? null : plotData[prevCheckFrameDownRangeX]);
                }

                graphics.DrawLine(Pens.White, x, 0, x, target.Height);

                RenderDownRange(downRangeData);
                int downRangeX = Math.Min((int)(plotData[x].horDistance / plotData[plotData.Count - 1].horDistance * plotData.Count), plotData.Count - 1);
                int downRangeY = (int)(plotData[x].alt * 0.01f * scale);
                graphics.DrawLine(Pens.Blue, downRangeX, downRangeY - 10, downRangeX, downRangeY + 10);

                prevCheckFrame = x;
                prevCheckFrameDownRangeX = downRangeX;

                ownerWindow.ShowMotorAndPitchStage(plotData[x].time);

                return x;
            }
            else
            {
                ownerWindow.ShowMotorAndPitchStage(-1);
                return -1;
            }
        }

        public void OnSlide(int direction)
        {
            if (prevCheckFrame == -1) return;
            int newFrame = prevCheckFrame + direction;
            newFrame = Math.Min(Math.Max(0, newFrame), plotData.Count - 1);
            OnClick(newFrame, 0);
        }
    }
}
