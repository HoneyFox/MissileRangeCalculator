using CSharpScriptExecutor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissileRangeCalculator
{
    public class Simulator
    {
        public Plotter plotter;

        public float deltaTime;
        public float accuracy;
        public float cd0(float time)
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
        public float cd1(float time)
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
        public float idFactor(float time)
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
        public float maxLiftCoeff(float time)
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
        public List<MotorInfo> motorInfo;
        public List<AeroInfo> aeroInfo;
        public float dryMass;
        public float refArea(float time)
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
        public float initSpeed;
        public float initAngle;
        public float initAlt;
        public float targetSpeed;
        public float targetDistance;
        public List<AngleInfo> angleRateInfo;
        public List<ScriptInfo> scriptInfo;
        public ScriptInstance scriptInstance;
        public float cutoffSpeed;

        public float maxThrustTime;

        public Simulator(Plotter plotter, float deltaTime, float accuracy, float cd0, float cd1, float idFactor, float maxLiftCoeff, List<MotorInfo> motorInfo, List<AeroInfo> aeroInfo, float dryMass, float diameter,
            float initSpeed, float initAngle, float initAlt, float targetSpeed, float targetDistance, List<AngleInfo> angleRateInfo, List<ScriptInfo> scriptInfo, ScriptInstance scriptInstance, float cutoffSpeed)
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
            this.scriptInfo = scriptInfo;
            this.scriptInstance = scriptInstance;
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

        public List<ScriptInfo> GetScriptInfo(float time)
        {
            List<ScriptInfo> result = new List<ScriptInfo>();
            for (int i = 0; i < scriptInfo.Count; ++i)
            {
                if (scriptInfo[i].timeStart <= time && scriptInfo[i].timeEnd > time)
                {
                    result.Add(scriptInfo[i]);
                }
            }

            return result;
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
            for (int i = 0; i < angleRateInfo.Count; ++i)
            {
                if (angleRateInfo[i].timeStart <= time && angleRateInfo[i].timeEnd > time)
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
            else if (alt <= 40000)
            {
                float T = 216.65f + (float)(0.001f * (alt - 20000));
                return 0.088035f * (float)(Math.Pow(T / 216.65, -35.1632));
            }
            else if (alt <= 120000)
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
            liftAcc = (curAcc + accForStraightFlight - (float)(Math.Sin(GetEngineAngle(time) * Math.PI / 180f) * GetThrust(time) / mass));
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

        public int curFrame;
        public float curTime;
        public float curMass;
        public float curAlt;
        public float curSpeed;
        public float curAcc;
        public float curAngle;
        public float curLiftAcc;
        public float curDragAcc;
        public float curCLReq;
        public float curHorDistance;
        public float curHorDistance39;

        public float curTargetDistance1;
        public float curTargetDistance2;
        public float curTargetDistance39;

        public float maxTAS;
        public float maxMach;
        public float maxAlt;

        public bool ignoreUpdateFrame = false;

        public void UpdateFrame(float deltaTime, bool overrideMass = false, bool overridePitchUpdate = false, bool overrideDrag = false, bool overrideThrust = false, bool overrideGravity = false, float mass = 0f, float pitch = 0f, float drag = 0f, float thrust = 0f, float gravity = 0f)
        {
            curMass = overrideMass ? mass : GetMass(curTime);
            float newAngle = UpdatePitchAngle(curTime, deltaTime, curMass);
            newAngle = (overridePitchUpdate ? pitch : newAngle);
            float deltaAngle = newAngle - curAngle;
            float engineAngle = GetEngineAngle(curTime);

            float curDrag = CalculateDrag((float)(deltaAngle / deltaTime * Math.PI / 180f), curMass, curTime, out curLiftAcc, out curCLReq);
            curDrag = (overrideDrag ? drag : curDrag);
            curDragAcc = curDrag / curMass;
            curAcc = ((overrideThrust ? thrust : GetThrust(curTime)) * (float)Math.Cos(engineAngle * Math.PI / 180f) - curDrag) / curMass - (float)((overrideGravity ? gravity : GetLocalG()) * Math.Sin(curAngle * Math.PI / 180f));
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

            scriptInstance?.ExecuteStart();

            while (true)
            {
                for (int i = 0; i < (int)(deltaTime / accuracy); ++i)
                {
                    ignoreUpdateFrame = false;
                    scriptInstance?.ExecuteUpdate();
                    var sis = GetScriptInfo(curTime);
                    foreach (var si in sis)
                        si.ExecutePreUpdate(scriptInstance?.GetDefaultClassInstance());
                    if(ignoreUpdateFrame == false)
                    {
                        UpdateFrame(accuracy);
                    }
                    foreach (var si in sis)
                        si.ExecutePostUpdate(scriptInstance?.GetDefaultClassInstance());
                    scriptInstance?.ExecutePostUpdate();
                    curTime += accuracy;
                }
                downRangeData.Add(new Tuple<double, double>(curHorDistance, curAlt));
                plotter.Record(curFrame, curTime, curMass, curHorDistance, curHorDistance39,
                    curAlt, curSpeed, TAStoIAS(curSpeed, curAlt), TAStoMach(curSpeed, curAlt), curAcc, curLiftAcc / 9.81f, (Math.Abs(curLiftAcc) > 0.005 && curDragAcc > 0 ? curLiftAcc / curDragAcc : 0.0f), curCLReq, curAngle,
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
            for (int i = 0; i < motorInfo.Count; ++i)
            {
                if (prevTime < motorInfo[i].timeStart && time >= motorInfo[i].timeStart)
                {
                    return true;
                }
                if (i == motorInfo.Count - 1)
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

}
