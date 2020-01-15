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
            List<MotorInfo> motorInfo = MotorInfo.AnalyzeMotorInfo(txtMotor.Text);
            List<AngleInfo> angleRateInfo = AngleInfo.AnalyzeAngleInfo(txtPitch.Text);
           
            simulator = new Simulator(plotter, float.Parse(txtDeltaTime.Text), float.Parse(txtSubsonicDrag.Text), float.Parse(txtSupersonicDrag.Text), float.Parse(txtInducedDragFactor.Text), float.Parse(txtCLMax.Text),
                motorInfo, float.Parse(txtDryMass.Text), float.Parse(txtDiameter.Text), float.Parse(txtInitSpeed.Text), float.Parse(txtInitAngle.Text), float.Parse(txtInitAlt.Text),
                float.Parse(txtTargetSpeed.Text), float.Parse(txtTargetDistance.Text), angleRateInfo, float.Parse(txtCutoffSpeed.Text));

            plotter.Clear();
            plotter.SetCutoffSpeed(float.Parse(txtCutoffSpeed.Text));
            plotter.SetRenderScale(float.Parse(txtDisplayScale.Text));
            simulator.Simulate();
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

                motorInfo.Add(new MotorInfo(timeElapsed, timeElapsed + time, thrust, totalPropellantMass, totalPropellantMass - propellantMass));
                timeElapsed += time;
                totalPropellantMass -= propellantMass;
            }

            return motorInfo;
        }


        public float timeStart;
        public float timeEnd;
        public float thrust;
        public float propellantMassStart;
        public float propellantMassEnd;

        public MotorInfo(float timeStart, float timeEnd, float thrust, float propellantMassStart, float propellantMassEnd)
        {
            this.timeStart = timeStart;
            this.timeEnd = timeEnd;
            this.thrust = thrust;
            this.propellantMassStart = propellantMassStart;
            this.propellantMassEnd = propellantMassEnd;
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
                string[] components = lines[i].Split(',');
                float time = float.Parse(components[0]);
                bool useLiftG = components[1].EndsWith("g", StringComparison.InvariantCultureIgnoreCase);
                if (components.Length == 2)
                {
                    if (useLiftG)
                        angleInfo.Add(new AngleInfo(timeElapsed, timeElapsed + time, 0, true, float.Parse(components[1].TrimEnd('g', 'G')), false, 0));
                    else
                        angleInfo.Add(new AngleInfo(timeElapsed, timeElapsed + time, float.Parse(components[1]), false, 0, false, 0));
                }
                else
                {
                    float targetAngle = float.Parse(components[2]);
                    if (useLiftG)
                        angleInfo.Add(new AngleInfo(timeElapsed, timeElapsed + time, 0, true, float.Parse(components[1].TrimStart('-').TrimEnd('g', 'G')), true, targetAngle));
                    else
                        angleInfo.Add(new AngleInfo(timeElapsed, timeElapsed + time, float.Parse(components[1].TrimStart('-')), false, 0, true, targetAngle));
                }
                timeElapsed += time;
            }

            return angleInfo;
        }

        public float timeStart;
        public float timeEnd;
        public float angleRate;
        public bool useLiftG;
        public float liftG;
        public bool useTargetAngle;
        public float targetAngle;

        public AngleInfo(float timeStart, float timeEnd, float angleRate, bool useLiftG, float liftG, bool useTargetAngle, float targetAngle)
        {
            this.timeStart = timeStart;
            this.timeEnd = timeEnd;
            this.angleRate = angleRate;
            this.useLiftG = useLiftG;
            this.liftG = liftG;
            this.useTargetAngle = useTargetAngle;
            this.targetAngle = targetAngle;
        }
    }

    public class Simulator
    {
        private Plotter plotter;

        float deltaTime;
        float cd0;
        float cd1;
        float idFactor;
        float maxLiftCoeff;
        List<MotorInfo> motorInfo;
        float dryMass;
        float refArea;
        float initSpeed;
        float initAngle;
        float initAlt;
        float targetSpeed;
        float targetDistance;
        List<AngleInfo> angleRateInfo;
        float cutoffSpeed;

        float maxThrustTime;

        public Simulator(Plotter plotter, float deltaTime, float cd0, float cd1, float idFactor, float maxLiftCoeff, List<MotorInfo> motorInfo, float dryMass, float diameter, 
            float initSpeed, float initAngle, float initAlt, float targetSpeed, float targetDistance, List<AngleInfo> angleRateInfo, float cutoffSpeed)
        {
            this.plotter = plotter;
            this.deltaTime = deltaTime;
            this.cd0 = cd0;
            this.cd1 = cd1;
            this.idFactor = idFactor;
            this.maxLiftCoeff = maxLiftCoeff;
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

            this.maxThrustTime = motorInfo[motorInfo.Count - 1].timeEnd;
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
            if ((target - value) * (target - (value + maxStep)) <= 0)
                return target;
            else
                return value + maxStep;
        }

        public float GetThrust(float time)
        {
            for(int i = 0; i < motorInfo.Count; ++i)
            {
                if (motorInfo[i].timeStart <= time && motorInfo[i].timeEnd > time)
                {
                    return motorInfo[i].thrust;
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
        
        public float GetMaxLiftForce()
        {
            float dynPressure = GetDynPressure(curSpeed, curAlt);
            return maxLiftCoeff * refArea * dynPressure;
        }

        public float UpdatePitchAngle(float time, float deltaTime, float mass)
        {
            float accForStraightFlight = (float)(Math.Cos(curAngle * Math.PI / 180f) * 9.81);
            for(int i = 0; i < angleRateInfo.Count; ++i)
            {
                if(angleRateInfo[i].timeStart <= time && angleRateInfo[i].timeEnd > time)
                {
                    if (curSpeed > 0)
                    {
                        float maxLiftG = GetMaxLiftForce() / mass;

                        float accRequired;
                        if (angleRateInfo[i].useLiftG == false)
                        {
                            if (angleRateInfo[i].useTargetAngle)
                                accRequired = (float)(angleRateInfo[i].angleRate * (angleRateInfo[i].targetAngle > curAngle ? 1 : -1) * Math.PI / 180) * curSpeed;
                            else
                                accRequired = (float)(angleRateInfo[i].angleRate * Math.PI / 180) * curSpeed;
                        }
                        else
                        {
                            if (angleRateInfo[i].useTargetAngle)
                                accRequired = angleRateInfo[i].liftG * 9.81f * (angleRateInfo[i].targetAngle > curAngle ? 1 : -1) - accForStraightFlight;
                            else
                                accRequired = angleRateInfo[i].liftG * 9.81f - accForStraightFlight;
                        }
                        float liftAccRequired = accRequired + accForStraightFlight;
                        float actualLiftAcc = Clamp(liftAccRequired, -maxLiftG, maxLiftG);
                        float angleRate = (actualLiftAcc - accForStraightFlight) / curSpeed;
                        if (angleRateInfo[i].useTargetAngle)
                            return MoveTowards(curAngle, angleRateInfo[i].targetAngle, (float)(angleRate * 180 / Math.PI) * deltaTime);
                        else
                            return curAngle + (float)(angleRate * 180 / Math.PI) * deltaTime;
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
            else if(alt <= 40000f)
            {
                float T = 216.65f + 0.001f * (alt - 20000f);
                return 0.088035f * (float)(Math.Pow(T / 216.65, -35.1632));
            }
            else if(alt <= 120000f)
            {
                return 0.003946607f * (float)(Math.Pow(0.5, (alt - 40000) / 6000));
            }
            else
            {
                return 0f;
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

        public float CalculateDrag(float angleRate, float mass, out float liftAcc, out float liftCoeff)
        {
            float dynPressure = GetDynPressure(curSpeed, curAlt);
            float drag0 = GetDragCoeff(TAStoMach(curSpeed, curAlt)) * refArea * dynPressure;
            float accForStraightFlight = (float)(Math.Cos(curAngle * Math.PI / 180f) * 9.81);
            float curAcc = angleRate * curSpeed;
            liftAcc = (curAcc + accForStraightFlight);
            float liftForce = liftAcc * mass;
            float dragL = 0f;
            if (dynPressure > 0)
            {
                liftCoeff = liftForce / refArea / dynPressure;
                float cdL = liftCoeff * liftCoeff * idFactor;
                dragL = cdL * refArea * dynPressure;
            }
            else
            {
                liftCoeff = 0f;
            }
            return drag0 + dragL;
        }

        int curFrame;
        float curTime;
        float curAlt;
        float curSpeed;
        float curAcc;
        float curAngle;
        float curLiftAcc;
        float curCLReq;
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
            float curMass = GetMass(curTime);
            float newAngle = UpdatePitchAngle(curTime, deltaTime, curMass);
            float deltaAngle = newAngle - curAngle;
            curAcc = (GetThrust(curTime) - CalculateDrag((float)(deltaAngle / deltaTime * Math.PI / 180f), curMass, out curLiftAcc, out curCLReq)) / curMass - 9.81f * (float)(Math.Sin(curAngle * Math.PI / 180f));
            curAngle = newAngle;
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
                    curAlt, curSpeed, TAStoIAS(curSpeed, curAlt), TAStoMach(curSpeed, curAlt), curAcc, curLiftAcc / 9.81f, curCLReq, curAngle,
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
            public float reqCL;
            public float angle;
            public float tgtDistance1;
            public float tgtDistance2;
            public float tgtDistance39;

            public PlotData(int frame, float time, float horDistance, float horDistance39, float alt, float TAS, float IAS, float mach, float acc, float liftG, float reqCL, float angle, float tgtDistance1, float tgtDistance2, float tgtDistance39)
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
                    .Append("Alt ").AppendLine(alt.ToString("F2"))
                    .Append("TAS ").AppendLine(TAS.ToString("F2"))
                    .Append("IAS ").AppendLine(IAS.ToString("F2"))
                    .Append("Mach ").AppendLine(mach.ToString("F2"))
                    .Append("Acc ").AppendLine(acc.ToString("F2"))
                    .Append("LiftG ").AppendLine(liftG.ToString("F2"))
                    .Append("ReqCL ").AppendLine(reqCL.ToString("F2"))
                    .Append("Angle ").AppendLine(angle.ToString("F2"))
                    .Append("HDist ").AppendLine(horDistance.ToString("F2"))
                    .Append("HDist39 ").AppendLine(horDistance39.ToString("F2"))
                    .Append("TgtDist ").Append(tgtDistance1.ToString("F2")).Append(" ").AppendLine(tgtDistance2.ToString("F2"))
                    .Append("TgtDist39 ").AppendLine(tgtDistance39.ToString("F2"));

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

        public void Render(int frame, float time, float horDistance, float horDistance39, float alt, float TAS, float IAS, float mach, float acc, float liftG, float reqCL, float angle, float tgtDistance1, float tgtDistance2, float tgtDistance39)
        {
            var data = new PlotData(frame, time, horDistance, horDistance39, alt, TAS, IAS, mach, acc, liftG, reqCL, angle, tgtDistance1, tgtDistance2, tgtDistance39);

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
