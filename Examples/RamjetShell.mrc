MRCData
0.40
0.55
0.1
700
0.406
700
0
50
150
0
2400000
3.2
2
0.3
3
5,0,0
15,150,252
2,0,0
3
5,0g
5,2g,50
2,0g
7
5,null,null
*,ActivateBooster(15,200,252),null
50,null,null
2,null,null
*,SetAltHold(0.008,0.4,28000,15),null
*,ActivateRamjet(130,3200,7200,4.0,80),null
80,null,null
155
using MissileRangeCalculator;
using MissileRangeCalculator.ScriptUtils;

public class PitchHold : AttitudeController
{
	public float expectedPitch;
	public float kP;
	
	public PitchHold(Simulator simulator, Missile owner, int priority, AttitudeCombineMode combineMode, float expectedPitch, float kP)
	: base(simulator, owner, priority, combineMode)
	{
		this.expectedPitch = expectedPitch;
		this.kP = kP;
	}
	
	public override void UpdateRawAngleCmd()
	{
		float pitchError = expectedPitch - simulator.curAngle;
		rawAngleCmd = pitchError * kP;
	}
}

[DefaultClass]
public class ScriptFunctions
{
	public Simulator simulator = null;
	float deltaTime = 1f / 64f;
	
	Missile missile = null;
	RocketEngine boosterSRB = null;
	AirbreathingEngine ramjet = null;
	ControlMachByEngine machController = null;
	AltHold altController = null;
	PitchHold pitchController = null;
	PurePursuit terminalController = null;
	
	[StartMethod]
	void Start()
	{
		simulator = FormMain.singleton.simulator;
		deltaTime = simulator.accuracy;
		
		missile = new Missile(simulator);
		boosterSRB = null;
		ramjet = null;
		machController = null;
		altController = null;
		pitchController = null;
		terminalController = null;
		
		Utils.ClearLog();
	}
	
	[UpdateMethod]
	void Update()
	{
		if (terminalController != null && terminalController.activated)
		{
			float ownX = simulator.curHorDistance39;
			float ownY = simulator.curAlt;
			float tgtX = terminalController.tgtX;
			float tgtY = 0;
			float horDistance = tgtX - ownX;
			float loftAlt = 0.000006f * horDistance * horDistance;
			Utils.DLog(loftAlt.ToString());
			terminalController.SetPos(ownX, ownY, tgtX, tgtY + loftAlt);
		}
		missile.Update();
		if (ramjet != null)
		{
			if (ramjet.exhausted)
			{
				if (ramjet.activated)
				{
					Utils.DLog("Ramjet exhausted.");
					ramjet.Deactivate();
				}
			}	
			else
			{
				Utils.DLog((ramjet.propellantMass - ramjet.consumedMass).ToString());
			}
			if (simulator.curTargetDistance39 - simulator.curHorDistance39 < 120000.0f)
			{
				if (terminalController == null)
				{
					ramjet.Deactivate();
					CancelAltHold();
					TerminalHome(0,0.8f);
				}
			}
		}
	}
	
	[PostUpdateMethod]
	void PostUpdate()
	{
		
	}
	
	void ActivateBooster(float thrustTime, float propellantMass, float isp)
	{
		if (boosterSRB == null)
		{
			float massRate = propellantMass / thrustTime;
			float thrust = massRate * isp * 9.81f;
			boosterSRB = new RocketEngine(simulator, missile, propellantMass, isp, thrust, thrust);
			boosterSRB.Activate();
			Utils.DLog("Booster SRB activated.");
		}
	}
	
	void ActivateRamjet(float propellantMass, float isp, float maxThrust, float expectedMach, float kP)
	{
		ramjet = new AirbreathingEngine(simulator, missile, propellantMass, isp, 0f, maxThrust);
		machController = new ControlMachByEngine(simulator, missile, ramjet, expectedMach, kP);
		ramjet.Activate();
		machController.Activate();
	}
	
	void SetAltHold(float kA, float kP, float expectedAlt, float maxPitch)
	{
		altController = new AltHold(simulator, missile, 1, AttitudeCombineMode.Additive, kA, kP, expectedAlt, -maxPitch, maxPitch);
		altController.Activate();
	}
	
	void UpdateAltHoldKA(float kA)
	{
		altController.SetKA(kA);
	}
	
	void CancelAltHold()
	{
		altController.Deactivate();
	}
	
	void SetPitchHold(float expectedPitch, float kP)
	{
		pitchController = new PitchHold(simulator, missile, 1, AttitudeCombineMode.Additive, expectedPitch, kP);
		pitchController.Activate();
	}
	
	void CancelPitchHold()
	{
		pitchController.Deactivate();
	}

	void TerminalHome(float targetAlt, float kNav)
	{
		terminalController = new PurePursuit(simulator, missile, 1, AttitudeCombineMode.Additive, kNav, 1);
		terminalController.SetPos(simulator.curHorDistance, simulator.curAlt, simulator.curTargetDistance39, targetAlt);
		terminalController.Activate();
	}
}

