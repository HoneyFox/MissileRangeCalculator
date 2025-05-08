MRCData
0.3
0.55
0.12
250
0.75
300
0
90
320
7600
315000
2.4
0.5
0.1
6
25,1200,273~276
0.1,200,0,0.3,0.55,0.12,0.65
30,150,310
50,0,0
0.1,80,0,0.5,0.7,0.12
150,0,0
2
1,0
10,2g,65
2
2,null,null
300,SetupAttMixer(),null
47
using MissileRangeCalculator;

[DefaultClass]
public class ScriptFunctions
{
	static void Log(string log)
	{
		FormMain.singleton.AddScriptLog(log);
	}
	
	static void DLog(string log)
	{
		StringBuilder sb = new StringBuilder();
		sb.Append("Frame").Append(FormMain.singleton.simulator.curFrame).Append("@").Append(FormMain.singleton.simulator.curTime).Append("sec: ").Append(log);
		FormMain.singleton.AddScriptLog(sb.ToString());
	}
	
	static void ClearLog()
	{
		FormMain.singleton.ClearScriptLog();
	}
	
	Simulator simulator = null;
	float deltaTime = 1f / 64f;
	
	[StartMethod]
	void Start()
	{
		simulator = FormMain.singleton.simulator;
		deltaTime = simulator.accuracy;
	}
	
	[UpdateMethod]
	void Update()
	{
		simulator.UpdateFrame(deltaTime);
		simulator.ignoreUpdateFrame = true;
	}
	
	[PostUpdateMethod]
	void PostUpdate()
	{
		
	}
	
}

