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
0.3
4
50,1200,255
1,0,0
800,300,1015~1010
6,150,270
5
1,0g
5,2g,30
14,4g,35
790,-0.15g~5g,0
90,-5g,-60
10
*,ActivateBooster(50,1200,255),null
50,null,null
*,SetAltHold(0.002,0.25,33000,15),null
*,ActivateScramjet(300,1250,7200,6.0,80),null
80,null,null
*,UpdateAltHoldKA(0.006),null
*,SetSineWaveStartTime(),null
680,SineWave(33000,800,80),null
*,CancelAltHold(),null
100,LowAltTerminalSRB(4000,6,150,270)
114
using MissileRangeCalculator;
using MissileRangeCalculator.ScriptUtils;

[DefaultClass]
public class ScriptFunctions
{
	public Simulator simulator = null;
	float deltaTime = 1f / 64f;
	
	Missile missile = null;
	RocketEngine boosterSRB = null;
	AirbreathingEngine scramjet = null;
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
		scramjet = null;
		machController = null;
		altController = null;
		terminalSRB = null;
		
		Utils.ClearLog();
	}
	
	[UpdateMethod]
	void Update()
	{
		missile.Update();
		if (scramjet != null)
			if (scramjet.exhausted)
				Utils.DLog("Scramjet exhausted.");
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
	
	void ActivateScramjet(float propellantMass, float isp, float maxThrust, float expectedMach, float kP)
	{
		scramjet = new AirbreathingEngine(simulator, missile, propellantMass, isp, 0f, maxThrust);
		machController = new ControlMachByEngine(simulator, missile, scramjet, expectedMach, kP);
		scramjet.Activate();
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
	
	float waveStartTime = -1;
	void SetSineWaveStartTime()
	{
		waveStartTime = simulator.curTime;
		Utils.DLog("Start sine wave maneuver.");
	}
	
	void SineWave(float baseAlt, float sinAmp, float period)
	{
		float expectedAlt = (float)Math.Sin((simulator.curTime - waveStartTime) / period * 2 * Math.PI) * sinAmp + baseAlt;
		altController.SetAlt(expectedAlt);
	}
	
	void CancelAltHold()
	{
		altController.Deactivate();
		Utils.DLog("Start terminal dive.");
	}
	
	void LowAltTerminalSRB(float triggerAlt, float thrustTime, float propellantMass, float isp)
	{
		if (simulator.curAlt <= triggerAlt)
		{
			if (terminalSRB == null)
			{
				float massRate = propellantMass / thrustTime;
				float thrust = massRate * isp * 9.81f;
				terminalSRB = new RocketEngine(simulator, missile, propellantMass, isp, thrust, thrust);
				terminalSRB.Activate();
				Utils.DLog("Alt reached(" + simulator.curAlt.ToString() + "), speed(" + simulator.curSpeed.ToString() + ")");
				Utils.DLog("Terminal SRB activated.");
			}
		}
	}
}

