MRCData
0.3
0.55
0.12
100
0.18
300
8000
0
320
400
45000
2.4
0.125
1
1
7.5,50,276
1
2,0
2
2,null,null
300,SetupAttMixer(),null
108
using MissileRangeCalculator;
using MissileRangeCalculator.ScriptUtils;

[DefaultClass]
public class ScriptFunctions
{
	Simulator simulator = null;
	float deltaTime = 1f / 64f;
	
	float tgtAlt = 7000f;
	Missile missile = null;
	PurePursuit pP = null;
	PNav pNav = null;
	AttitudeMixer attMixer = null;
	
	[StartMethod]
	void Start()
	{
		simulator = FormMain.singleton.simulator;
		deltaTime = simulator.accuracy;
		
		Utils.ClearLog();
		
		missile = new Missile(simulator);
		pP = new PurePursuit(simulator, missile, 1, AttitudeCombineMode.Additive, 0.8f, 7);
		pNav = new PNav(simulator, missile, 1, AttitudeCombineMode.Additive, 2.0f, 7);
		attMixer = new AttitudeMixer(simulator, missile, 1, AttitudeCombineMode.Replace);
		attMixer.AddSubAC(pP, 0.8f);
		attMixer.AddSubAC(pNav, 0.2f);
		
		Random r = new Random();
		simulator.targetDistance = simulator.curTargetDistance1 = simulator.curTargetDistance2 = simulator.curTargetDistance39 = (float)r.NextDouble() * 80000f + 20000f;
		simulator.targetSpeed = (float)r.NextDouble() * 150f + 250f;
		tgtAlt = (float)r.NextDouble() * 19000f + 1000f;
		
		Utils.Log("TGT(" + simulator.targetDistance.ToString() + "," + tgtAlt.ToString() + ")@" + simulator.targetSpeed.ToString());
	}
	
	[UpdateMethod]
	void Update()
	{
		float horDistance = simulator.curTargetDistance1 - simulator.curHorDistance;
		float loftBias = horDistance * horDistance / 400000f;
		loftBias = Math.Min(loftBias, horDistance * 0.25f);
		pP.SetPos(simulator.curHorDistance, simulator.curAlt, simulator.curTargetDistance1, tgtAlt + loftBias);
		pNav.SetPos(simulator.curHorDistance, simulator.curAlt, simulator.curTargetDistance1, tgtAlt);
		missile.Update();
	}
	
	[PostUpdateMethod]
	void PostUpdate()
	{
		
	}
	
	float lastErrorX, lastErrorY;
	void SetupAttMixer()
	{
		float t = simulator.curTime;
		float distance = simulator.curTargetDistance1 - simulator.curHorDistance;
		if (distance > 0)
		{
			if (attMixer.activated == false)
			{
				attMixer.Activate();
				SetupPNav(2.0f,4.0f,18000f,true);
			}
			SetupPNav(2.0f,4.0f,18000f);			
			attMixer.SetWeight(pP, Math.Max(0f, Math.Min((distance - 40000f) / 30000f * 0.8f, 0.8f)));
			attMixer.SetWeight(pNav, Math.Max(0.2f, Math.Min(0.2f + (70000f - distance) / 30000f * 0.8f, 1.0f)));

			lastErrorX = distance;
			lastErrorY = tgtAlt - simulator.curAlt;
		}
		else
		{
			if (attMixer.activated)
			{
				attMixer.Deactivate();
				simulator.cutoffSpeed = 99999f;
				float errorX = distance;
				float errorY = tgtAlt - simulator.curAlt;
				float unlerpY = errorY * (lastErrorX / (lastErrorX - errorX)) + lastErrorY * (-errorX / (lastErrorX - errorX));
				Utils.DLog("Encounter target, proximity range: " + unlerpY.ToString());
			}
		}		
	}
	
	float lastKNav = 0f;
	void SetupPNav(float kNavMid = 2.5f, float kNavTerm = 4.0f, float distTerm = 18000f, bool initialize = false)
	{
		float distance = simulator.curTargetDistance1 - simulator.curHorDistance;
		if (distance > 0)
		{
			if (initialize)
			{
				lastKNav = (distance > distTerm ? kNavMid : kNavTerm);
				if (distance <= distTerm)
					Utils.DLog("Go Terminal.");
			}
			pNav.SetKNav(distance > distTerm ? kNavMid : kNavTerm);
			if (pNav.kNav != lastKNav)
				Utils.DLog("Go Terminal.");
			lastKNav = pNav.kNav;
		}
	}
}

