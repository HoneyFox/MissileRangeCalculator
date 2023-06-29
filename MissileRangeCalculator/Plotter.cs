using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MissileRangeCalculator
{
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
                graphics.DrawLine(Pens.Black, data.frame - 1, target.Height * 0.667f + Math.Abs(prevData.ldRatio) * 40f * scale, data.frame, target.Height * 0.667f + Math.Abs(data.ldRatio) * 40f * scale);
                graphics.DrawLine(Pens.DarkGray, data.frame - 1, target.Height * 0.667f + prevData.ldRatio * 40f * scale, data.frame, target.Height * 0.667f + data.ldRatio * 40f * scale);
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
