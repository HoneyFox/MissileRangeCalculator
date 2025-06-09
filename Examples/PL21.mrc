MRCData
0.55
0.8
0.12
100
0.24
370
8000
0
300
300
1000000
2.1
1
0.3
6
1,0,0 // Free-fall
5,50,0 // 1st Booster
605,100,0 // Ramjet
10,0,0
0,40,2,0.34,0.55,0.11,0.203,2.6
6,40,0
7
1,0
5,0
605,0
10,0
50,5g,20
100,0g
90,-1g,-40
10
1,null,null
5,ActivateBooster(5,50,260),null
*,SetAltHold(0.0015,4,28000,25),null
*,ActivateRamjet(100,1010,8000,4.5,10),null
100,null,null
*,UpdateAltHoldKA(0.004),null
505,null,null
10,null,null
*,CancelAltHold(),null
*,ActivateTerminalSRB(6,40,255)
102
using MissileRangeCalculator;
using MissileRangeCalculator.ScriptUtils;

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
	RocketEngine terminalSRB = null;
	
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
		terminalSRB = null;
		
		Utils.ClearLog();
	}
	
	[UpdateMethod]
	void Update()
	{
		missile.Update();
		if (ramjet != null)
		{
			if (ramjet.exhausted)
			{
				Utils.DLog("Ramjet exhausted.");
				ramjet= null;
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
		Utils.DLog("Start terminal stage.");
	}
	
	void ActivateTerminalSRB(float thrustTime, float propellantMass, float isp)
	{
		if (terminalSRB == null)
		{
			float massRate = propellantMass / thrustTime;
			float thrust = massRate * isp * 9.81f;
			terminalSRB = new RocketEngine(simulator, missile, propellantMass, isp, thrust, thrust);
			terminalSRB.Activate();
			Utils.DLog("Terminal SRB activated.");
		}
	}
}

