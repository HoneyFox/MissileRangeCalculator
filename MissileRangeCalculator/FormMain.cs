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
    public partial class FormMain : Form
    {
        Plotter plotter;
        Simulator simulator;

        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            plotter = new Plotter(this.picMain, this.Font, this.picPlotData, this.picLegends);
            plotter.Clear();
            plotter.RenderLegends();
        }

        private void btnSimulate_Click(object sender, EventArgs e)
        {
            List<Tuple<float, float, float, float, float>> motorInfo = AnalyzeMotorInfo(txtMotor.Text);
            List<Tuple<float, float, float, float>> angleRateInfo = AnalyzeAngleInfo(txtPitch.Text);

            simulator = new Simulator(plotter, float.Parse(txtDeltaTime.Text), float.Parse(txtSubsonicDrag.Text), float.Parse(txtSupersonicDrag.Text), float.Parse(txtInducedDragFactor.Text),
                motorInfo, float.Parse(txtDryMass.Text), float.Parse(txtDiameter.Text), float.Parse(txtInitSpeed.Text), float.Parse(txtInitAngle.Text), float.Parse(txtInitAlt.Text),
                float.Parse(txtTargetSpeed.Text), float.Parse(txtTargetDistance.Text), angleRateInfo, float.Parse(txtCutoffSpeed.Text));

            plotter.Clear();
            plotter.SetCutoffSpeed(float.Parse(txtCutoffSpeed.Text));
            plotter.SetRenderScale(float.Parse(txtDisplayScale.Text));
            simulator.Simulate();
        }

        private List<Tuple<float, float, float, float, float>> AnalyzeMotorInfo(string text)
        {
            List<Tuple<float, float, float, float, float>> motorInfo = new List<Tuple<float, float, float, float, float>>();

            float timeElapsed = 0f;
            float totalPropellantMass = 0f;

            string[] lines = text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
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
                float isp = float.Parse(components[2]);
                float thrust = propellantMass / time * isp * 9.81f;

                motorInfo.Add(new Tuple<float, float, float, float, float>(timeElapsed, timeElapsed + time, thrust, totalPropellantMass, totalPropellantMass - propellantMass));
                timeElapsed += time;
                totalPropellantMass -= propellantMass;
            }

            return motorInfo;
        }

        private List<Tuple<float, float, float, float>> AnalyzeAngleInfo(string text)
        {
            List<Tuple<float, float, float, float>> angleInfo = new List<Tuple<float, float, float, float>>();

            float timeElapsed = 0f;
            float totalAngleRotated = 0f;

            string[] lines = text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < lines.Length; ++i)
            {
                string[] components = lines[i].Split(',');
                float time = float.Parse(components[0]);
                float angleRate = float.Parse(components[1]);

                angleInfo.Add(new Tuple<float, float, float, float>(timeElapsed, timeElapsed + time, totalAngleRotated, totalAngleRotated + angleRate * time));
                timeElapsed += time;
                totalAngleRotated += angleRate * time;
            }

            return angleInfo;
        }

        private void picMain_MouseDown(object sender, MouseEventArgs e)
        {
            if (simulator != null)
            {
                plotter.OnClick(e.X, e.Y);
                simulator.RenderStatistics();
                picMain.Refresh();
                picPlotData.Refresh();
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
            if (simulator != null && (e.KeyCode == Keys.Left || e.KeyCode == Keys.Right))
            {
                plotter.OnSlide(e.KeyCode == Keys.Left ? -1 : 1);
            }
        }
    }

    public class Simulator
    {
        private Plotter plotter;

        float deltaTime;
        float cd0;
        float cd1;
        float idFactor;
        List<Tuple<float, float, float, float, float>> motorInfo;
        float dryMass;
        float refArea;
        float initSpeed;
        float initAngle;
        float initAlt;
        float targetSpeed;
        float targetDistance;
        List<Tuple<float, float, float, float>> angleRateInfo;
        float cutoffSpeed;

        float maxThrustTime;

        public Simulator(Plotter plotter, float deltaTime, float cd0, float cd1, float idFactor, List<Tuple<float, float, float, float, float>> motorInfo, float dryMass, float diameter, 
            float initSpeed, float initAngle, float initAlt, float targetSpeed, float targetDistance, List<Tuple<float, float, float, float>> angleRateInfo, float cutoffSpeed)
        {
            this.plotter = plotter;
            this.deltaTime = deltaTime;
            this.cd0 = cd0;
            this.cd1 = cd1;
            this.idFactor = idFactor;
            this.motorInfo = motorInfo;
            this.dryMass = dryMass;
            this.refArea = (float)(diameter * diameter * Math.PI * 0.25) * 1.414f;
            this.initSpeed = initSpeed;
            this.initAngle = initAngle;
            this.initAlt = initAlt;
            this.targetSpeed = targetSpeed;
            this.targetDistance = targetDistance;
            this.angleRateInfo = angleRateInfo;
            this.cutoffSpeed = cutoffSpeed;

            this.maxThrustTime = motorInfo[motorInfo.Count - 1].Item2;
        }

        public static float Lerp(float start, float end, float t)
        {
            return start + (end - start) * t;
        }

        public static float Unlerp(float start, float end, float c)
        {
            return (c - start) / (end - start);
        }

        public float GetThrust(float time)
        {
            for(int i = 0; i < motorInfo.Count; ++i)
            {
                if (motorInfo[i].Item1 <= time && motorInfo[i].Item2 > time)
                {
                    return motorInfo[i].Item3;
                }
            }

            return 0f;
        }

        public float GetMass(float time)
        {
            for (int i = 0; i < motorInfo.Count; ++i)
            {
                if (motorInfo[i].Item1 <= time && motorInfo[i].Item2 > time)
                {
                    float t = Unlerp(motorInfo[i].Item1, motorInfo[i].Item2, time);
                    float currentPropellantMass = Lerp(motorInfo[i].Item4, motorInfo[i].Item5, t);
                    return dryMass + currentPropellantMass;
                }
            }

            return dryMass;
        }
        
        public float GetPitchAngle(float time)
        {
            for(int i = 0; i < angleRateInfo.Count; ++i)
            {
                if(angleRateInfo[i].Item1 <= time && angleRateInfo[i].Item2 > time)
                {
                    float t = Unlerp(angleRateInfo[i].Item1, angleRateInfo[i].Item2, time);
                    float currentRotatedAngle = Lerp(angleRateInfo[i].Item3, angleRateInfo[i].Item4, t);
                    return initAngle + currentRotatedAngle;
                }
            }

            return initAngle + angleRateInfo[angleRateInfo.Count - 1].Item4;
        }

        public float GetDragCoeff(float mach)
        {
            float Cd0 = 0f;
            if (mach <= 1)
            {
                Cd0 = (float)(Math.Sin(Math.Pow(mach, 10.0) * 3.1415926 + 1.5 * 3.1415926) * (cd1 - cd0) / 2.0 + cd0 + (cd1 - cd0) / 2.0);
            }
            else
            {
                Cd0 = (float)(Math.Sin(Math.Pow(mach, -0.75) * 3.1415926 - 0.5 * 3.1415926) * (cd1 - cd0) / 2 + cd0 + (cd1 - cd0) / 2.0);
            }

            return Cd0;
        }

        public static float TAStoIAS(float TAS, float alt)
        {
            float rho0 = 1.225f;
            float rho = GetAirDensity(alt);
            return TAS * (float)(Math.Sqrt(rho / rho0));
        }

        public static float GetAirDensity(float alt)
        {
            float rho0 = 1.225f;
            float T0 = 288.15f;
            if (alt <= 11000f)
            {
                float T = T0 - 0.0065f * alt;
                return rho0 * (float)(Math.Pow(T / T0, 4.25588));
            }
            else if (alt > 11000f && alt <= 20000f)
            {
                return 0.36392f * (float)(Math.Exp((11000 - alt) / 6341.62));
            }
            else
            {
                float T = 216.65f + 0.001f * (alt - 20000f);
                return 0.088035f * (float)(Math.Pow(T / 216.65, -35.1632));
            }
        }

        public static float GetSonicSpeed(float alt)
        {
            float T0 = 288.15f;
            float T;
            if (alt <= 11000f)
            {
                T = T0 - 0.0065f * alt;
            }
            else if (alt > 11000f && alt <= 20000f)
            {
                T = T0 - 0.0065f * 11000f;
            }
            else
            {
                T = T0 - 0.0065f * 11000f + 0.001f * (alt - 20000f);
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

        public float CalculateDrag(float deltaTime, float speed, float angle, float angleRate, float mass, out float liftAcc)
        {
            float dynPressure = GetDynPressure(curSpeed, curAlt);
            float drag0 = GetDragCoeff(TAStoMach(curSpeed, curAlt)) * refArea * dynPressure;
            float accForStraightFlight = (float)(Math.Cos(angle * Math.PI / 180f) * 9.81);
            float curAcc = angleRate * speed;
            liftAcc = (curAcc + accForStraightFlight);
            float liftForce = liftAcc * mass;
            float liftCoeff = liftForce / refArea / dynPressure;
            liftCoeff = Math.Min(2f, Math.Max(liftCoeff, -2f));
            float cdL = liftCoeff * liftCoeff * idFactor;
            float dragL = cdL * refArea * dynPressure;
            return drag0 + dragL;
        }

        int curFrame;
        float curTime;
        float curAlt;
        float curSpeed;
        float curAcc;
        float curAngle;
        float curLiftAcc;
        float curHorDistance;
        float curHorDistance39;

        float curTargetDistance1;
        float curTargetDistance2;
        float curTargetDistance39;
        
        float maxTAS;
        float maxMach;
        float maxAlt;

        public void UpdateFrame()
        {
            float newAngle = GetPitchAngle(curTime);
            float deltaAngle = newAngle - curAngle;
            curAngle = newAngle;
            float curMass = GetMass(curTime);
            curAcc = (GetThrust(curTime) - CalculateDrag(deltaTime, curSpeed, curAngle, (float)(deltaAngle / deltaTime * Math.PI / 180f), curMass, out curLiftAcc)) / curMass - 9.81f * (float)(Math.Sin(curAngle * Math.PI / 180f));
            curSpeed = curSpeed + curAcc * deltaTime;
            curHorDistance += curSpeed * (float)(Math.Cos(curAngle * Math.PI / 180f)) * deltaTime;
            curAlt += curSpeed * (float)(Math.Sin(curAngle * Math.PI / 180f)) * deltaTime;
            if (curSpeed > targetSpeed)
                curHorDistance39 += (float)(Math.Sqrt(curSpeed * curSpeed - targetSpeed * targetSpeed) * (float)(Math.Cos(curAngle * Math.PI / 180f)) * deltaTime);

            curTargetDistance1 -= targetSpeed * deltaTime;
            curTargetDistance2 += targetSpeed * deltaTime;
        }

        public void Simulate()
        {
            curFrame = 0;
            curTime = 0f;
            curAlt = initAlt;
            curSpeed = initSpeed;
            curAcc = 0f;
            curAngle = initAngle;
            curHorDistance = curHorDistance39 = 0f;

            curTargetDistance1 = curTargetDistance2 = curTargetDistance39 = targetDistance;
            
            while (true)
            {
                UpdateFrame();
                plotter.Render(curFrame, curTime, curHorDistance, curHorDistance39,
                    curAlt, curSpeed, TAStoIAS(curSpeed, curAlt), TAStoMach(curSpeed, curAlt), curAcc, curLiftAcc / 9.81f, curAngle,
                    curTargetDistance1, curTargetDistance2, curTargetDistance39);
                curTime += deltaTime;
                curFrame++;

                if (maxTAS < curSpeed) maxTAS = curSpeed;
                if (maxMach < TAStoMach(curSpeed, curAlt)) maxMach = TAStoMach(curSpeed, curAlt);
                if (maxAlt < curAlt) maxAlt = curAlt;

                if (curAlt < 0) break;
                if (curTime > maxThrustTime && curSpeed < cutoffSpeed) break;

                if (curFrame > 99999) break;
            }

            RenderStatistics();
        }

        public void RenderStatistics()
        {
            plotter.RenderStatistics(curFrame, curTime, curAlt, curAngle, TAStoMach(curSpeed, curAlt), curSpeed, curHorDistance, curHorDistance39, maxMach, maxTAS, maxAlt);
        }
    }

    public class Plotter
    {
        public class PlotData
        {
            public int frame;
            public float time;
            public float horDistance;
            public float horDistance39;
            public float alt;
            public float TAS;
            public float IAS;
            public float mach;
            public float acc;
            public float liftG;
            public float angle;
            public float tgtDistance1;
            public float tgtDistance2;
            public float tgtDistance39;

            public PlotData(int frame, float time, float horDistance, float horDistance39, float alt, float TAS, float IAS, float mach, float acc, float liftG, float angle, float tgtDistance1, float tgtDistance2, float tgtDistance39)
            {
                this.frame = frame;
                this.time = time;
                this.horDistance = horDistance;
                this.horDistance39 = horDistance39;
                this.alt = alt;
                this.TAS = TAS;
                this.IAS = IAS;
                this.mach = mach;
                this.acc = acc;
                this.liftG = liftG;
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
                    .Append("Time ").AppendLine(time.ToString())
                    .Append("Alt ").AppendLine(alt.ToString())
                    .Append("TAS ").AppendLine(TAS.ToString())
                    .Append("IAS ").AppendLine(IAS.ToString())
                    .Append("Mach ").AppendLine(mach.ToString())
                    .Append("Acc ").AppendLine(acc.ToString())
                    .Append("LiftG ").AppendLine(liftG.ToString())
                    .Append("Angle ").AppendLine(angle.ToString())
                    .Append("HDist ").AppendLine(horDistance.ToString())
                    .Append("HDist39 ").AppendLine(horDistance39.ToString())
                    .Append("TgtDist ").Append(tgtDistance1.ToString()).Append(" ").AppendLine(tgtDistance2.ToString())
                    .Append("TgtDist39 ").AppendLine(tgtDistance39.ToString());

                return sb.ToString();
            }
        }

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

        bool isFirstFrame = true;

        List<PlotData> plotData = new List<PlotData>();

        public Plotter(PictureBox target, Font font, PictureBox picPlotData, PictureBox picLegends)
        {
            this.target = target;
            this.font = font;
            this.picPlotData = picPlotData;
            this.picLegends = picLegends;
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

            isFirstFrame = true;
            plotData.Clear();
        }

        public void SetRenderScale(float s)
        {
            this.scale = s;
        }

        public void SetCutoffSpeed(float s)
        {
            this.cutoffSpeed = s;
        }

        public void Render(int frame, float time, float horDistance, float horDistance39, float alt, float TAS, float IAS, float mach, float acc, float liftG, float angle, float tgtDistance1, float tgtDistance2, float tgtDistance39)
        {
            var data = new PlotData(frame, time, horDistance, horDistance39, alt, TAS, IAS, mach, acc, liftG, angle, tgtDistance1, tgtDistance2, tgtDistance39);

            if (isFirstFrame == false)
            {
                RenderPlotData(data, plotData[plotData.Count - 1]);
            }
            else
            {
                RenderPlotData(data, null);
            }

            plotData.Add(data);

            isFirstFrame = false;
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
                graphics.DrawLine(Pens.White, data.frame - 1, prevData.alt * 0.01f * scale, data.frame, data.alt * 0.01f * scale);
                graphics.DrawLine(Pens.Orange, data.frame - 1, prevData.mach * 100 * scale, data.frame, data.mach * 100 * scale);

                graphics.DrawLine(Pens.Cyan, data.frame - 1, prevData.tgtDistance1 * 0.005f * scale, data.frame, data.tgtDistance1 * 0.005f * scale);
                graphics.DrawLine(Pens.Cyan, data.frame - 1, prevData.tgtDistance2 * 0.005f * scale, data.frame, data.tgtDistance2 * 0.005f * scale);
                graphics.DrawLine(Pens.Yellow, data.frame - 1, prevData.tgtDistance39 * 0.005f * scale, data.frame, data.tgtDistance39 * 0.005f * scale);

                graphics.DrawLine(Pens.Blue, data.frame - 1, prevData.horDistance * 0.005f * scale, data.frame, data.horDistance * 0.005f * scale);
                graphics.DrawLine(Pens.Yellow, data.frame - 1, prevData.horDistance39 * 0.005f * scale, data.frame, data.horDistance39 * 0.005f * scale);

                graphics.DrawLine(Pens.Magenta, data.frame - 1, target.Height * 0.667f + prevData.angle * 1f, data.frame, target.Height * 0.667f + data.angle * 1f);
                graphics.DrawLine(Pens.White, data.frame - 1, target.Height * 0.667f, data.frame, target.Height * 0.667f);
            }

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

        public void RenderStatistics(int frame, float time, float alt, float angle, float mach, float speed, float horDistance, float horDistance39, float maxMach, float maxSpeed, float maxAlt)
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

        int prevCheckFrame = -1;

        public void OnClick(int x, int y)
        {
            if (x >= 0 && x < plotData.Count)
            {
                plotDataGraphics.Clear(Color.DarkBlue);
                plotDataGraphics.DrawString(plotData[x].ToString(), font, Brushes.White, 10f, 10f);

                graphics.DrawLine(new Pen(target.BackColor), prevCheckFrame, 0, prevCheckFrame, target.Height);

                if (prevCheckFrame >= 0 && prevCheckFrame < plotData.Count)
                {
                    RenderPlotData(plotData[prevCheckFrame], (prevCheckFrame == 0) ? null : plotData[prevCheckFrame - 1]);
                }

                graphics.DrawLine(Pens.White, x, 0, x, target.Height);

                prevCheckFrame = x;
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
