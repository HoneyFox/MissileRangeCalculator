MRCData
0.3
0.55
0.1
650
0.75
370
8000
0
300
0
1325000
1.8
1
0.5
4
50,1200,255
1,0,0
896,300,1015~1010
6,150,255
5
1,0g
5,2g,30
15,4g,35
880,-0.15g~5g,0
90,-5g,-60
4
50,null,null
1*,ActivateScramjet(300, 1000, 8000, 50, 5),null
896,null,null
1*,ActivateSRB(6, 150, 255)
262
using MissileRangeCalculator;

[DefaultClass]
public class Vessel
{
	static void Log(string log)
	{
		FormMain.singleton.AddScriptLog(log);
	}
	
	static void DLog(string log)
	{
		StringBuilder sb = new StringBuilder();
		sb.Append("Frame").Append(FormMain.singleton.simulator.curFrame).Append("@").Append(FormMain.singleton.simulator.curTime.ToString("F2")).Append("sec: ").Append(log);
		FormMain.singleton.AddScriptLog(sb.ToString());
	}
	
	static void ClearLog()
	{
		FormMain.singleton.ClearScriptLog();
	}
	
	public Simulator simulator = null;
	float deltaTime = 1f / 64f;
	
	List<Engine> activatedEngines = null;
	float vesselMass = 0;
	Scramjet scramjet = null;
	SRB terminalSRB = null;
	
	[StartMethod]
	void Start()
	{
		simulator = FormMain.singleton.simulator;
		deltaTime = simulator.accuracy;
		
		activatedEngines = new List<Engine>();
		vesselMass = simulator.curMass;
		scramjet = null;
		terminalSRB = null;
		
		ClearLog();
	}
	
	[UpdateMethod]
	void Update()
	{
		if (activatedEngines.Count > 0)
		{
			float totalThrust = 0f;
			foreach(Engine e in activatedEngines)
			{
				e.Update();
				totalThrust += e.curThrust;
			}
			simulator.UpdateFrame(
				deltaTime,
				true, false, false, true, false,
				vesselMass, 0, 0, totalThrust, 0
			);
			simulator.ignoreUpdateFrame = true;
		}
		else
		{
			simulator.UpdateFrame(deltaTime);
			simulator.ignoreUpdateFrame = true;
			vesselMass = simulator.curMass;
		}
	}
	
	[PostUpdateMethod]
	void PostUpdate()
	{
		
	}
	
	public class Engine
	{
		public Simulator simulator;
		public Vessel owner;
		public float deltaTime;
		public float propellantMass;
		public float consumedMass;
		public float isp;
		public float maxThrust;
		public float curThrust;
		public bool activated = false;
		public bool exhausted = false;
		
		public Engine(Simulator simulator, Vessel owner, float propellantMass, float isp, float maxThrust)
		{
			this.simulator = simulator;
			this.deltaTime = simulator.accuracy;
			this.owner = owner;
			this.propellantMass = propellantMass;
			this.consumedMass = 0f;
			this.isp = isp;
			this.maxThrust = maxThrust;
			this.curThrust = 0f;
		}
		
		public virtual bool Activate()
		{
			if (activated == false)
			{
				owner.activatedEngines.Add(this);
				activated = true;
				//DLog("Engine activated. CurMass: " + owner.vesselMass.ToString());
				return true;
			}
			return false;
		}
		
		public virtual void Update()
		{
			if (activated == false)
			{
				curThrust = 0f;
				return;
			}
			if (exhausted == false)
			{
				float requestedThrust = maxThrust;
				float massRate = requestedThrust / (isp * 9.81f);
				float massConsumption = massRate * deltaTime;
				float remainMass = propellantMass - consumedMass;
				if (massConsumption > remainMass)
				{
					requestedThrust = remainMass / massConsumption * requestedThrust;
					//DLog("\t" + requestedThrust.ToString() + "\t\t" + remainMass.ToString());
					consumedMass = propellantMass;
					owner.vesselMass -= remainMass;
					exhausted = true;
					//DLog("Engine exhausted. CurMass: " + owner.vesselMass.ToString());
				}
				else
				{			
					consumedMass += massConsumption;
					owner.vesselMass -= massConsumption;
					//DLog("\t" + requestedThrust.ToString() + "\t\t" + remainMass.ToString());
				}
				curThrust = requestedThrust;
			}
			else
			{
				curThrust = 0f;
			}
		}
	}
	
	public class SRB : Engine
	{
		public SRB(Simulator simulator, Vessel owner, float propellantMass, float isp, float maxThrust)
		: base(simulator, owner, propellantMass, isp, maxThrust)
		{
		}
		
		public override bool Activate()
		{
			if (base.Activate())
			{
				DLog("SRB activated. CurMass: " + owner.vesselMass.ToString());
				return true;
			}
			return false;
		}
	}
	
	public class Scramjet : Engine
	{
		public Scramjet(Simulator simulator, Vessel owner, float propellantMass, float isp, float maxThrust)
		: base(simulator, owner, propellantMass, isp, maxThrust)
		{
		}

		public float kP;
		public float expectedMach;
		
		public override bool Activate()
		{
			if (base.Activate())
			{
				DLog("Scramjet activated. CurMass: " + owner.vesselMass.ToString());
				DLog("\tThrust\t\tPropellantMass\tIsp");
				return true;
			}
			return false;
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
				this.isp = 1200f - simulator.curSpeed * 0.1f;
				this.maxThrust = simulator.curSpeed * 0.4f + 7500f;
				
				float curDragForce = simulator.curDragAcc * owner.vesselMass;
				float gravityCompensation = (float)Math.Sin(simulator.curAngle * Math.PI / 180f) * owner.vesselMass * 9.81f;
				float curMach = Simulator.TAStoMach(simulator.curSpeed, simulator.curAlt);
				float thrustDiff = (expectedMach - curMach) * owner.vesselMass * kP;
				float expectedThrust = thrustDiff + curDragForce + gravityCompensation;
				float requestedThrust = Math.Max(0f, Math.Min(expectedThrust, maxThrust));
				float massRate = requestedThrust / (isp * 9.81f);
				float massConsumption = massRate * deltaTime;
				float remainMass = propellantMass - consumedMass;
				if (massConsumption > remainMass)
				{
					requestedThrust = remainMass / massConsumption * requestedThrust;
					DLog("\t" + requestedThrust.ToString("F2") + "\t\t" + remainMass.ToString("F2") + "\t\t" + isp.ToString("F2"));
					consumedMass = propellantMass;
					owner.vesselMass -= remainMass;
					exhausted = true;
					DLog("Scramjet exhausted. CurMass: " + owner.vesselMass.ToString("F2"));
				}
				else
				{
					consumedMass += massConsumption;
					owner.vesselMass -= massConsumption;
					DLog("\t" + requestedThrust.ToString("F2") + "\t\t" + remainMass.ToString("F2") + "\t\t" + isp.ToString("F2"));
				}
				curThrust = requestedThrust;
			}
			else
			{
				curThrust = 0f;
			}
		}
	}
	
	void ActivateScramjet(float propellantMass, float isp, float maxThrust, float kP, float expectedMach)
	{
		if (scramjet == null)
		{
			scramjet = new Scramjet(simulator, this, propellantMass, isp, maxThrust);
		}
		UpdateScramjet(kP, expectedMach);
		scramjet.Activate();
	}
	
	void UpdateScramjet(float kP, float expectedMach)
	{
		scramjet.kP = kP;
		scramjet.expectedMach = expectedMach;
	}
	
	void ActivateSRB(float thrustTime, float propellantMass, float isp)
	{
		float massRate = propellantMass / thrustTime;
		float thrust = massRate * isp * 9.81f;
		if (terminalSRB == null)
		{
			terminalSRB = new SRB(simulator, this, propellantMass, isp, thrust);
		}
		terminalSRB.Activate();
	}
}

