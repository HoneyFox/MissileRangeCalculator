using CSharpScriptExecutor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MissileRangeCalculator
{
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
                string[] components = lines[i].Split(new char[] { ',' }, StringSplitOptions.None);
                float time = float.Parse(components[0]);
                bool useLiftG = components[1].EndsWith("g", StringComparison.InvariantCultureIgnoreCase);

                bool useTargetAngle = false;
                float targetAngle = 0;
                if (components.Length >= 3)
                {
                    if (components[2] != "")
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

    public class ScriptInfo
    {
        public static List<ScriptInfo> AnalyzeScriptInfo(string text, ScriptModule scriptModule)
        {
            List<ScriptInfo> scriptInfo = new List<ScriptInfo>();

            if (scriptModule == null)
                return scriptInfo;

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
                string[] components = lines[i].Split(new char[] { ',' }, StringSplitOptions.None);
                bool onlyOnce = (components[0] == "*");
                bool timeRange = components[0].Contains("~");
                if (timeRange == false)
                {
                    if (onlyOnce == false)
                    {
                        float time = float.Parse(components[0]);
                        string script = lines[i].Substring(lines[i].IndexOf(',') + 1);
                        List<object> scriptComponents = SplitScriptInfo(script, scriptModule);
                        scriptInfo.Add(new ScriptInfo(timeElapsed, timeElapsed + time, (MethodInfo)scriptComponents[0], (object[])scriptComponents[1], (MethodInfo)scriptComponents[2], (object[])scriptComponents[3], onlyOnce));

                        timeElapsed += time;
                    }
                    else
                    {
                        string script = lines[i].Substring(lines[i].IndexOf(',') + 1);
                        List<object> scriptComponents = SplitScriptInfo(script, scriptModule);
                        scriptInfo.Add(new ScriptInfo(timeElapsed, timeElapsed + 0.125f, (MethodInfo)scriptComponents[0], (object[])scriptComponents[1], (MethodInfo)scriptComponents[2], (object[])scriptComponents[3], onlyOnce));
                    }
                }
                else
                {
                    string[] timeRangeComponents = components[0].Split('~');
                    float timeStart = float.Parse(timeRangeComponents[0]);
                    float timeEnd = float.Parse(timeRangeComponents[1]);
                    string script = lines[i].Substring(lines[i].IndexOf(',') + 1);
                    List<object> scriptComponents = SplitScriptInfo(script, scriptModule);
                    scriptInfo.Add(new ScriptInfo(timeStart, timeEnd, (MethodInfo)scriptComponents[0], (object[])scriptComponents[1], (MethodInfo)scriptComponents[2], (object[])scriptComponents[3], onlyOnce));
                }
            }
            return scriptInfo;
        }

        private static List<object> SplitScriptInfo(string scriptInfo, ScriptModule scriptModule)
        {
            List<object> result = new List<object>();

            string[] components = new string[2] { null, null };
            int bracketLevel = 0;
            int lastSplitPos = 0;
            int curPos = 0;
            int index = 0;
            var enumerator = scriptInfo.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var chr = enumerator.Current;
                if (chr == '(')
                    bracketLevel++;
                else if (chr == ')')
                    bracketLevel--;
                if (chr == ',' && bracketLevel == 0)
                {
                    components[index] = (scriptInfo.Substring(lastSplitPos, curPos - lastSplitPos).Trim());
                    lastSplitPos = curPos + 1;
                    index++;
                }
                curPos++;
            }
            if (bracketLevel == 0)
            {
                components[index] = (scriptInfo.Substring(lastSplitPos, curPos - lastSplitPos).Trim());
            }

            foreach (string script in components)
            {
                if (script == null || script == "" || script.ToLower() == "null")
                {
                    result.Add(null); result.Add(null);
                }
                else
                {
                    int bracketBegin = script.IndexOf('(');
                    int bracketEnd = script.LastIndexOf(')');
                    string scriptFuncName = script.Substring(0, bracketBegin).Trim();
                    string scriptParams = script.Substring(bracketBegin + 1, bracketEnd - bracketBegin - 1);
                    var methods = scriptModule.GetDefaultClassMethods();
                    if (methods.ContainsKey(scriptFuncName))
                    {
                        var mi = methods[scriptFuncName];
                        var pis = mi.GetParameters();
                        string[] scriptParameters = scriptParams.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        if (pis.Length == scriptParameters.Length)
                        {
                            var scriptParameterObjs = new object[pis.Length];
                            for (int j = 0; j < pis.Length; ++j)
                            {
                                Type paramType = pis[j].ParameterType;
                                string trimmed = scriptParameters[j].Trim();
                                if (paramType == typeof(int))
                                {
                                    scriptParameterObjs[j] = int.Parse(trimmed);
                                }
                                else if (paramType == typeof(float))
                                {
                                    scriptParameterObjs[j] = float.Parse(trimmed.TrimEnd('f'));
                                }
                                else if (paramType == typeof(double))
                                {
                                    scriptParameterObjs[j] = double.Parse(trimmed);
                                }
                                else if (paramType == typeof(bool))
                                {
                                    scriptParameterObjs[j] = bool.Parse(trimmed);
                                }
                                else if (paramType == typeof(string))
                                {
                                    scriptParameterObjs[j] = trimmed.Substring(1, trimmed.Length - 2);
                                }
                            }
                            result.Add(mi);
                            result.Add(scriptParameterObjs);
                        }
                        else
                        {
                            result.Add(null); result.Add(null);
                        }
                    }
                    else
                    {
                        result.Add(null); result.Add(null);
                    }
                }
            }

            return result;
        }

        public object ExecutePreUpdate(object instance)
        {
            if (invokeOnlyOnce == false || invoked == false)
            {
                if (instance == null) return null;
                return preUpdateScriptMethod?.Invoke(instance, preUpdateScriptMethodParams);
            }
            return null;
        }

        public object ExecutePostUpdate(object instance)
        {
            if (invokeOnlyOnce == false || invoked == false)
            {
                invoked = true;
                if (instance == null) return null;
                return postUpdateScriptMethod?.Invoke(instance, postUpdateScriptMethodParams);
            }
            return null;
        }

        public float timeStart;
        public float timeEnd;
        public bool invokeOnlyOnce;
        private bool invoked = false;
        public MethodInfo preUpdateScriptMethod;
        public object[] preUpdateScriptMethodParams;
        public MethodInfo postUpdateScriptMethod;
        public object[] postUpdateScriptMethodParams;

        public ScriptInfo(float timeStart, float timeEnd, MethodInfo preUpdateScriptMethod, object[] preUpdateScriptMethodParams, MethodInfo postUpdateScriptMethod, object[] postUpdateScriptMethodParams, bool invokeOnlyOnce = false)
        {
            this.timeStart = timeStart;
            this.timeEnd = timeEnd;
            this.invokeOnlyOnce = invokeOnlyOnce;
            this.preUpdateScriptMethod = preUpdateScriptMethod;
            this.preUpdateScriptMethodParams = preUpdateScriptMethodParams;
            this.postUpdateScriptMethod = postUpdateScriptMethod;
            this.postUpdateScriptMethodParams = postUpdateScriptMethodParams;
        }
    }
}
