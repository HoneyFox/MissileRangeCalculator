using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MissileRangeCalculator.ScriptUtils
{
    public class Utils
    {
        public static void Log(string log)
        {
            FormMain.singleton.AddScriptLog(log);
        }

        public static void DLog(string log)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Frame").Append(FormMain.singleton.simulator.curFrame).Append("@").Append(FormMain.singleton.simulator.curTime.ToString("F2")).Append("sec: ").Append(log);
            FormMain.singleton.AddScriptLog(sb.ToString());
        }

        public static void ClearLog()
        {
            FormMain.singleton.ClearScriptLog();
        }
    }

    public class Missile
    {
        public Simulator simulator;
        public List<Engine> activatedEngines = null;
        public List<EngineController> engineControllers = null;
        public List<AttitudeController> activatedAttitudeControllers = null;
        public float mass = 0f;

        public Missile(Simulator simulator)
        {
            this.simulator = simulator;
            this.activatedEngines = new List<Engine>();
            this.engineControllers = new List<EngineController>();
            this.activatedAttitudeControllers = new List<AttitudeController>();
            this.mass = simulator.curMass;
        }

        public virtual void Update()
        {
            bool customMass = false;
            bool customThrust = false;
            bool customAngle = false;

            float totalThrust = 0f;
            float finalAngle = 0f;
            
            if (engineControllers.Count > 0)
            {
                foreach (EngineController ec in engineControllers)
                {
                    ec.Update();
                }
            }

            if (activatedEngines.Count > 0)
            {
                customMass = true;
                foreach (Engine e in activatedEngines)
                {
                    e.Update();
                    totalThrust += e.curThrust;
                    customThrust |= (e.activated && !e.exhausted);
                }
            }

            var activeACs = new List<AttitudeController>();
            foreach (var ac in activatedAttitudeControllers)
            {
                if (ac.activated) activeACs.Add(ac);
            }
            if (activeACs.Count > 0)
            {
                activeACs.Sort();
                finalAngle = simulator.curAngle;
                var angleChange = 0f;
                foreach (AttitudeController ac in activeACs)
                {
                    ac.Update();
                    if (ac.combineMode == AttitudeCombineMode.Replace)
                        angleChange = ac.angleChange;
                    else if (ac.combineMode == AttitudeCombineMode.Additive)
                        angleChange += ac.angleChange;
                }
                finalAngle += angleChange;
                customAngle = true;
            }

            simulator.UpdateFrame(
                simulator.accuracy,
                customMass, customAngle, false, customThrust, false,
                mass, finalAngle, 0, totalThrust, 0
            );
            simulator.ignoreUpdateFrame = true;

            if (customMass == false)
                mass = simulator.curMass;
        }
    }

    public class Engine
    {
        public Simulator simulator;
        public Missile owner;
        public float deltaTime;
        public float propellantMass;
        public float consumedMass;
        public float isp;
        public float minThrust;
        public float maxThrust;
        public float requestedThrust;
        public float curThrust;
        public bool activated = false;
        public bool exhausted = false;

        public Engine(Simulator simulator, Missile owner, float propellantMass, float isp, float minThrust, float maxThrust)
        {
            this.simulator = simulator;
            this.deltaTime = simulator.accuracy;
            this.owner = owner;
            this.propellantMass = propellantMass;
            this.consumedMass = 0f;
            this.isp = isp;
            this.minThrust = minThrust;
            this.maxThrust = maxThrust;
            this.requestedThrust = maxThrust;
            this.curThrust = 0f;
        }

        public virtual bool Activate()
        {
            if (activated == false)
            {
                owner.activatedEngines.Add(this);
                activated = true;
                return true;
            }
            return false;
        }

        public virtual void RequestThrust(float thrust)
        {
            requestedThrust = thrust;
        }

        public virtual void Update()
        {
            if (activated == false)
            {
                curThrust = 0f;
                return;
            }
            UpdateThrust();
        }

        public virtual void UpdateThrust()
        {
            if (exhausted == false)
            {
                float reqThrust = Math.Max(minThrust, Math.Min(requestedThrust, maxThrust));
                float massRate = reqThrust / (isp * 9.81f);
                float massConsumption = massRate * deltaTime;
                float remainMass = propellantMass - consumedMass;
                if (massConsumption > remainMass)
                {
                    reqThrust = remainMass / massConsumption * reqThrust;
                    consumedMass = propellantMass;
                    owner.mass -= remainMass;
                    exhausted = true;
                }
                else
                {
                    consumedMass += massConsumption;
                    owner.mass -= massConsumption;
                }
                curThrust = reqThrust;
            }
            else
            {
                curThrust = 0f;
            }
        }
    }

    public class RocketEngine : Engine
    {
        public RocketEngine(Simulator simulator, Missile owner, float propellantMass, float isp, float minThrust, float maxThrust)
        : base(simulator, owner, propellantMass, isp, minThrust, maxThrust)
        {
        }
    }

    public class AirbreathingEngine : Engine
    {
        public float staticMinThrust;
        public float staticMaxThrust;
        public float staticIsp;

        public AirbreathingEngine(Simulator simulator, Missile owner, float propellantMass, float isp, float minThrust, float maxThrust)
        : base(simulator, owner, propellantMass, isp, minThrust, maxThrust)
        {
            this.staticIsp = isp;
            this.staticMinThrust = minThrust;
            this.staticMaxThrust = maxThrust;
        }

        public override void Update()
        {
            if (activated == false)
            {
                curThrust = 0f;
                return;
            }
            if (exhausted == false)
            {
                this.isp = staticIsp - simulator.curSpeed * 0.1f;
                this.minThrust = simulator.curSpeed * 0.1f + staticMinThrust;
                this.maxThrust = simulator.curSpeed * 0.4f + staticMaxThrust;
            }
            UpdateThrust();
        }
    }

    public abstract class EngineController
    {
        public Simulator simulator;
        public Missile owner;
        public Engine engine;
        public bool activated = false;

        public EngineController(Simulator simulator, Missile owner, Engine engine)
        {
            this.simulator = simulator;
            this.owner = owner;
            this.engine = engine;
        }

        public virtual void Activate()
        {
            owner.engineControllers.Add(this);
            activated = true;
        }

        public virtual void Deactivate()
        {
            owner.engineControllers.Remove(this);
            activated = false;
        }

        public virtual void Update()
        {
            if (activated)
                UpdateEngineControl();
        }

        public abstract void UpdateEngineControl();
    }

    public class ControlSpeedByEngine : EngineController
    {
        public float expectedSpeed;
        public float kP;
        public ControlSpeedByEngine(Simulator simulator, Missile owner, Engine engine, float expectedSpeed, float kP)
        : base(simulator, owner, engine)
        {
            this.expectedSpeed = expectedSpeed;
            this.kP = kP;
        }

        public override void UpdateEngineControl()
        {
            float curDragForce = simulator.curDragAcc * owner.mass;
            float gravityCompensation = (float)Math.Sin(simulator.curAngle * Math.PI / 180f) * owner.mass * 9.81f;
            float thrustDiff = (expectedSpeed - simulator.curSpeed) * owner.mass * kP;
            float expectedThrust = thrustDiff + curDragForce + gravityCompensation;
            engine.RequestThrust(expectedThrust);
        }
    }

    public class ControlMachByEngine : EngineController
    {
        public float expectedMach;
        public float kP;
        public ControlMachByEngine(Simulator simulator, Missile owner, Engine engine, float expectedMach, float kP)
        : base(simulator, owner, engine)
        {
            this.expectedMach = expectedMach;
            this.kP = kP;
        }

        public override void UpdateEngineControl()
        {
            float curDragForce = simulator.curDragAcc * owner.mass;
            float gravityCompensation = (float)Math.Sin(simulator.curAngle * Math.PI / 180f) * owner.mass * 9.81f;
            float curMach = Simulator.TAStoMach(simulator.curSpeed, simulator.curAlt);
            float thrustDiff = (expectedMach - curMach) * owner.mass * kP;
            float expectedThrust = thrustDiff + curDragForce + gravityCompensation;
            engine.RequestThrust(expectedThrust);
        }
    }

    public enum AttitudeCombineMode
    {
        Replace = 0,
        Additive = 1,
    }

    public abstract class AttitudeController : IComparable<AttitudeController>
    {
        public Simulator simulator;
        public Missile owner;
        public bool activated = false;
        public int priority;
        public AttitudeCombineMode combineMode = AttitudeCombineMode.Additive;
        public float rawAngleCmd { get; protected set; }
        public float angleChange { get; protected set; }

        public AttitudeController(Simulator simulator, Missile owner, int priority, AttitudeCombineMode combineMode)
        {
            this.simulator = simulator;
            this.owner = owner;
            this.priority = priority;
            this.combineMode = combineMode;
            rawAngleCmd = 0f;
            angleChange = 0f;
        }

        public int CompareTo(AttitudeController other)
        {
            return this.priority.CompareTo(other.priority);
        }

        public virtual void Activate()
        {
            owner.activatedAttitudeControllers.Add(this);
            activated = true;
        }
        public virtual void Deactivate()
        {
            owner.activatedAttitudeControllers.Remove(this);
            activated = false;
        }

        public virtual void Update()
        {
            if (activated)
            {
                UpdateRawAngleCmd();
                UpdateAttitude();
            }
        }

        public abstract void UpdateRawAngleCmd();

        public virtual void UpdateAttitude()
        {
            float t = simulator.curTime;
            float maxTurnRate = simulator.GetMaxLiftForce(t) / owner.mass / simulator.curSpeed * 180.0f / 3.1415926f;
            float gravityTurnRate = (float)simulator.GetNetG() / simulator.curSpeed * 180.0f / 3.1415926f;
            float clampAngleCmd = Math.Max(-maxTurnRate - gravityTurnRate, Math.Min(rawAngleCmd, maxTurnRate - gravityTurnRate));

            angleChange = clampAngleCmd * simulator.accuracy;
        }
    }

    public class AttitudeMixer : AttitudeController
    {
        protected Dictionary<AttitudeController, float> subACs = null;

        public AttitudeMixer(Simulator simulator, Missile owner, int priority, AttitudeCombineMode combineMode)
        : base(simulator, owner, priority, combineMode)
        {
            subACs = new Dictionary<AttitudeController, float>();
        }

        public void AddSubAC(AttitudeController ac, float weight)
        {
            subACs[ac] = weight;
            if (ac.activated)
                Utils.Log("AttitudeController added into ACMixer is activated, usually it should be inactive.");
        }

        public void RemoveSubAC(AttitudeController ac)
        {
            subACs.Remove(ac);
        }

        public void SetWeight(AttitudeController ac, float weight)
        {
            if (subACs.ContainsKey(ac))
                subACs[ac] = weight;
        }

        public override void UpdateRawAngleCmd()
        {
            float sum = 0f;
            foreach(var kv in subACs)
            {
                kv.Key.UpdateRawAngleCmd();
                sum += kv.Key.rawAngleCmd * kv.Value;
            }

            rawAngleCmd = sum;
        }
    }

    public class PurePursuit : AttitudeController
    {
        public float kNav = 0f;
        public float ownX = 0f;
        public float ownY = 0f;
        public float tgtX = 0f;
        public float tgtY = 0f;
        public int inputDelay = 0;

        protected Queue<float> delayedLOS = null;

        public PurePursuit(Simulator simulator, Missile owner, int priority, AttitudeCombineMode combineMode, float kNav, int inputDelay)
        : base(simulator, owner, priority, combineMode)
        {
            this.kNav = kNav;
            this.inputDelay = inputDelay;
            delayedLOS = new Queue<float>(inputDelay + 1);
        }

        public void SetKNav(float kNav)
        {
            this.kNav = kNav;
        }

        public void SetPos(float ownX, float ownY, float tgtX, float tgtY)
        {
            this.ownX = ownX;
            this.ownY = ownY;
            this.tgtX = tgtX;
            this.tgtY = tgtY;
        }

        public override void UpdateRawAngleCmd()
        {
            float t = simulator.curTime;

            float distance = tgtX - ownX;
            float LOS = (float)(Math.Atan2(tgtY - ownY, distance)) * 180.0f / 3.1415926f;
            delayedLOS.Enqueue(LOS);
            if (delayedLOS.Count == inputDelay + 1)
                LOS = delayedLOS.Dequeue();
            else
                LOS = simulator.curAngle;
            float PPCmd = (LOS - simulator.curAngle) * kNav;
            
            rawAngleCmd = PPCmd;
        }
    }

    public class PNav : AttitudeController
    {
        public float kNav = 0f;
        public float ownX = 0f;
        public float ownY = 0f;
        public float tgtX = 0f;
        public float tgtY = 0f;
        public float lastLOS = 0f;
        public float lastT = 0f;
        public int inputDelay = 0;

        protected Queue<float> delayedLOS = null;
        protected bool hasPastInitialInput = false;
        protected bool isInitialInput = false;
        
        public PNav(Simulator simulator, Missile owner, int priority, AttitudeCombineMode combineMode, float kNav, int inputDelay)
        : base(simulator, owner, priority, combineMode)
        {
            this.kNav = kNav;
            this.inputDelay = inputDelay;
            delayedLOS = new Queue<float>(inputDelay+1);
        }

        public void SetKNav(float kNav)
        {
            this.kNav = kNav;
        }

        public void SetPos(float ownX, float ownY, float tgtX, float tgtY)
        {
            this.ownX = ownX;
            this.ownY = ownY;
            this.tgtX = tgtX;
            this.tgtY = tgtY;
        }

        public override void UpdateRawAngleCmd()
        {
            float t = simulator.curTime;

            float distance = tgtX - ownX;
            float LOS = (float)(Math.Atan2(tgtY - ownY, distance)) * 180.0f / 3.1415926f;
            delayedLOS.Enqueue(LOS);
            if (delayedLOS.Count == inputDelay+1)
            {
                if (hasPastInitialInput == false)
                {
                    hasPastInitialInput = isInitialInput = true;
                }
                else
                {
                    isInitialInput = false;
                }
                LOS = delayedLOS.Dequeue();
            }
            else
                LOS = 0f;
            if(isInitialInput) lastLOS = LOS;
            float LOSRate = (LOS - lastLOS) / (t - lastT);
            float PNCmd = LOSRate * kNav;
            
            rawAngleCmd = PNCmd;
                
            lastLOS = LOS;
            lastT = t;
        }
    }

    public class AltHold : AttitudeController
    {
        public float kA;
        public float kP;
        public float expectedAlt;
        public float minPitch;
        public float maxPitch;
        
        public AltHold(Simulator simulator, Missile owner, int priority, AttitudeCombineMode combineMode, float kA, float kP, float expectedAlt, float minPitch, float maxPitch)
        : base(simulator, owner, priority, combineMode)
        {
            this.kA = kA;
            this.kP = kP;
            this.expectedAlt = expectedAlt;
            this.minPitch = minPitch;
            this.maxPitch = maxPitch;
        }

        public void SetKA(float kA)
        {
            this.kA = kA;
        }

        public void SetKP(float kP)
        {
            this.kP = kP;
        }

        public void SetAlt(float expectedAlt)
        {
            this.expectedAlt = expectedAlt;
        }

        public void SetMinPitch(float minPitch)
        {
            this.minPitch = minPitch;
        }

        public void SetMaxPitch(float maxPitch)
        {
            this.maxPitch = maxPitch;
        }

        public override void UpdateRawAngleCmd()
        {
            float altError = expectedAlt - simulator.curAlt;
            float expectedAngle = Math.Max(minPitch, Math.Min(kA * altError, maxPitch));
            float PitchCmd = (expectedAngle - simulator.curAngle) * kP;

            rawAngleCmd = PitchCmd;
        }
    }
}
