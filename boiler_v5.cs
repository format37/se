//---------
List<IMyGravityGenerator> gravs;
List<IMyArtificialMassBlock> mass;
List<IMyGyro> gyros;
List<IMyGyro> stab_gyros;
List<IMyTerminalBlock> temp;
IMyShipController controller;

bool Damp;
bool Test;

//alex++
bool rap_enabled; //Random Auto Pilot
int waypoint_treshold;
int distance_min;
int distance_max;
//alex--

Program()
{

//alex++
rap_enabled = false;
waypoint_treshold = 100;
distance_min = 150;
distance_max = 400;
//alex--

	gravs = new List<IMyGravityGenerator>();
	mass = new List<IMyArtificialMassBlock>();
	gyros = new List<IMyGyro>();
	stab_gyros = new List<IMyGyro>();
	temp = new List<IMyTerminalBlock>();

	GridTerminalSystem.GetBlocksOfType<IMyShipController>(temp, b => (b.IsSameConstructAs(Me)&&b.CustomName.Contains("MainCockpit")));
	if (temp.Count > 0) controller = temp[0] as IMyShipController;

	GridTerminalSystem.GetBlocksOfType<IMyGravityGenerator>(gravs, b=>(b.IsSameConstructAs(Me) && !b.CustomName.Contains("Stop")));
	GridTerminalSystem.GetBlocksOfType<IMyGyro>(stab_gyros, b => (b.IsSameConstructAs(Me) && b.CustomName.Contains("Stab")));
	GridTerminalSystem.GetBlocksOfType<IMyArtificialMassBlock>(mass, b => (b.IsSameConstructAs(Me)));
	GridTerminalSystem.GetBlocksOfType<IMyGyro>(gyros, b => (b.IsSameConstructAs(Me)));
}

void Main(string arg, UpdateType uType)
{

// Alex ++
IMyRemoteControl rcbroiler = GridTerminalSystem.GetBlockWithName("RCBroiler") as IMyRemoteControl;
if (arg == "timer_block")
    {
        var brs = GridTerminalSystem.GetBlockWithName("BuildAndRepairSystem") as IMyShipWelder;
        var lcd = GridTerminalSystem.GetBlockWithName("build_panel") as IMyTextPanel;
        lcd.WriteText((System.DateTime.Now).ToString()+"\n");
        lcd.WriteText((brs.CustomInfo).ToString(), true);

        //Random Auto Pilot
        var lcd_debug = GridTerminalSystem.GetBlockWithName("debug_panel") as IMyTextPanel;
        lcd_debug.WriteText((System.DateTime.Now).ToString()+"\n");
        lcd_debug.WriteText("rap_enabled: "+rap_enabled.ToString()+"\n", true);
        List<MyWaypointInfo> Waypoints = new List<MyWaypointInfo>();
        rcbroiler.GetWaypointInfo(Waypoints);
        if (rap_enabled)
            {            
            if (Waypoints.Count == 0) 
                {
                waypoint_add_current(rcbroiler);
                rcbroiler.SetCollisionAvoidance(true);
                rcbroiler.SetAutoPilotEnabled(true);
                }
            if (destination_distance(rcbroiler, Waypoints) < waypoint_treshold)
                {
                    waypoint_add_random(rcbroiler); 
                    rcbroiler.SetAutoPilotEnabled(true);
                }
            }    
        lcd_debug.WriteText("Waypoints.Count: "+Waypoints.Count.ToString()+"\n", true);
        if (Waypoints.Count>0) lcd_debug.WriteText("Distance: "+destination_distance(rcbroiler, Waypoints).ToString()+"\n", true);
        
        /*
        //random pilot 
        var lcd_debug = GridTerminalSystem.GetBlockWithName("debug_panel") as IMyTextPanel;
        lcd_debug.WriteText((System.DateTime.Now).ToString()+"\n");
        List<MyWaypointInfo> Waypoints = new List<MyWaypointInfo>();
        MyWaypointInfo CurrentWayPoint = rcbroiler.CurrentWaypoint;
        rcbroiler.GetWaypointInfo(Waypoints);
        
        //if Waypoints.Count &&  CurrentWayPoint.Name==last_wp_name rcbroiler.ClearWaypoints();        
        //if (rcbroiler.IsAutoPilotEnabled&&Waypoints.Count == 0)
        lcd_debug.WriteText("IsAutoPilotEnabled: "+rcbroiler.IsAutoPilotEnabled.ToString()+"\n", true);
        lcd_debug.WriteText("Waypoints.Count: "+Waypoints.Count.ToString()+"\n", true);
        if (Waypoints.Count == 0)
            {
            Vector3D player = new Vector3D(0, 0, 0);
            player = Me.GetPosition();
            rcbroiler.AddWaypoint(player, "0");
            }
        if (rcbroiler.IsAutoPilotEnabled)
            {        
            var last_wp_name = Waypoints[Waypoints.Count-1].Name;
            if (CurrentWayPoint.Name==last_wp_name)
                {
                rcbroiler.ClearWaypoints();
                for (int i=0;i<10;i++) random_ap_add_wp();
                rcbroiler.SetAutoPilotEnabled(true);
                lcd_debug.WriteText("reset\n", true);
                }
            lcd_debug.WriteText("WP: "+CurrentWayPoint.Name+" / "+last_wp_name+"\n", true);        
            }        
        lcd_debug.WriteText("count: "+ Waypoints.Count.ToString() +"\n", true);
        */
    }

//var remote = GridTerminalSystem.GetBlockWithName("RCBroiler") as IMyRemoteControl;
if (arg == "ap_state") ap_state();
if (arg == "rap_start") 
{
    rap_enabled = true;
    //rcbroiler.SetCollisionAvoidance(true);
    //rcbroiler.SetAutoPilotEnabled(true);
}
if (arg == "rap_stop") 
{
    rap_enabled = false;
    rcbroiler.ClearWaypoints();
    //rcbroiler.SetAutoPilotEnabled(false);
}
/*if (arg == "add_random") random_ap_add_wp();
if (arg == "test_left") bias_waypoint(0,-10,0);
if (arg == "test_right") bias_waypoint(0,10,0);
if (arg == "test_up") bias_waypoint(0,0,10);
if (arg == "test_down") bias_waypoint(0,0,-10);
if (arg == "test_forward") bias_waypoint(10,0,0);
if (arg == "test_back") bias_waypoint(-10,0,0);*/
//Alex --

	if (uType == UpdateType.Update1)
	{
		Update();
	}
	else
	{
		switch (arg)
		{
			case ("Start"):
				EngineON(true);
				Runtime.UpdateFrequency = UpdateFrequency.Update1;
				break;
			case ("Stop"):
				EngineON(false);
				Runtime.UpdateFrequency = UpdateFrequency.None;
				break;
			case ("Damp"):
				Damp = !Damp;
				break;
			case ("Test"):
				Test = !Test;
				break;
			default:
				break;
		}
	}

}

//alex++
void waypoint_add_current(IMyRemoteControl rcbroiler)
{
    Vector3D player = new Vector3D(0, 0, 0);
    player = Me.GetPosition();
    rcbroiler.AddWaypoint(player, "0");
}

void waypoint_add_random(IMyRemoteControl rcbroiler)
{    
    //var remote = GridTerminalSystem.GetBlockWithName("RCBroiler") as IMyRemoteControl;
    Vector3D player = new Vector3D(0, 0, 0);
    player = Me.GetPosition();
    Random rnd = new Random();
    int x = rnd.Next(distance_min, distance_max);
    int y = rnd.Next(distance_min, distance_max);
    int z = rnd.Next(distance_min, distance_max);
    Vector3D bias_point = new Vector3D(x, y, z);
    bias_point = Vector3D.Add(player, bias_point);    
    List<MyWaypointInfo> Waypoints = new List<MyWaypointInfo>();
    MyWaypointInfo CurrentWayPoint = rcbroiler.CurrentWaypoint;
    rcbroiler.GetWaypointInfo(Waypoints);
    //add
    rcbroiler.AddWaypoint(bias_point, (Waypoints.Count).ToString());
}

double destination_distance(IMyRemoteControl rcbroiler, List<MyWaypointInfo> Waypoints)
{
    Vector3D current = new Vector3D(0, 0, 0);
    Vector3D destination = new Vector3D(0, 0, 0);
    current = Me.GetPosition();
    destination = Waypoints[Waypoints.Count - 1].Coords;
    return Vector3D.Distance(current, destination);
}

void bias_waypoint(IMyRemoteControl rcbroiler, int x, int y, int z)
{
        //var remote = GridTerminalSystem.GetBlockWithName("RCBroiler") as IMyRemoteControl;                
        rcbroiler.ClearWaypoints();
        Vector3D player = new Vector3D(0, 0, 0);
        Vector3D bias_point = new Vector3D(x, y, z);
        player = Me.GetPosition();        
        bias_point = Vector3D.Add(player, bias_point);

        /*var lcd = GridTerminalSystem.GetBlockWithName("debug_panel") as IMyTextPanel;        
        lcd.WriteText("Player:\n");
        lcd.WriteText("X: "+(player.X).ToString()+"\n", true);
        lcd.WriteText("Y: "+(player.Y).ToString()+"\n", true);
        lcd.WriteText("Z: "+(player.Z).ToString()+"\n", true);
        lcd.WriteText("Bias:\n", true);
        lcd.WriteText("X: "+(bias_point.X).ToString()+"\n", true);
        lcd.WriteText("Y: "+(bias_point.Y).ToString()+"\n", true);
        lcd.WriteText("Z: "+(bias_point.Z).ToString()+"\n", true);
        lcd.WriteText((remote.Orientation).ToString()+"\n", true);*/

        //waypoints count
        List<MyWaypointInfo> Waypoints = new List<MyWaypointInfo>();
        MyWaypointInfo CurrentWayPoint = rcbroiler.CurrentWaypoint;
        rcbroiler.GetWaypointInfo(Waypoints);

        //add
        rcbroiler.AddWaypoint(bias_point, (Waypoints.Count).ToString());
        rcbroiler.SetAutoPilotEnabled(true);
}

void ap_state()
{
        var remote = GridTerminalSystem.GetBlockWithName("RCBroiler") as IMyRemoteControl;                        
        Vector3D player = new Vector3D(0, 0, 0);        
        player = Me.GetPosition();        

        var lcd = GridTerminalSystem.GetBlockWithName("debug_panel") as IMyTextPanel;        
        //lcd.WriteText("Player:\n");
        //lcd.WriteText("X: "+(player.X).ToString()+"\n", true);
        //lcd.WriteText("Y: "+(player.Y).ToString()+"\n", true);
        //lcd.WriteText("Z: "+(player.Z).ToString()+"\n", true);
        List<MyWaypointInfo> Waypoints = new List<MyWaypointInfo>();
        MyWaypointInfo CurrentWayPoint = remote.CurrentWaypoint;
        remote.GetWaypointInfo(Waypoints);
        lcd.WriteText("Count: "+(Waypoints.Count).ToString()+"\n");
        //	foreach (MyWaypointInfo waypoint in Waypoints) lcd.WriteText((waypoint.Name).ToString()+"\n", true);
        lcd.WriteText("Current: "+(CurrentWayPoint).ToString()+"\n", true);
        var last_wp_name = Waypoints[Waypoints.Count-1].Name;
        lcd.WriteText("Last: "+last_wp_name+"\n", true);
        //lcd.WriteText("AP enabled: "+(remote.IsAutoPilotEnabled).ToString()+"\n", true);
        //lcd.WriteText("Last WP reached: "+(CurrentWayPoint.Name==((Waypoints.Count-1).ToString()).ToString()).ToString()+"\n", true);
        //lcd.WriteText(": "+(CurrentWayPoint.Name==((Waypoints.Count-1).ToString()).ToString()).ToString()+"\n", true);
        
}
//alex--

void Update()
{
	Vector3D input = Vector3D.Transform(controller.MoveIndicator, controller.WorldMatrix.GetOrientation());
	if (controller.DampenersOverride || Damp)
		input -= controller.GetShipVelocities().LinearVelocity * 0.005;
	if (Test) input += controller.WorldMatrix.Right;
	foreach (IMyGravityGenerator g in gravs)
	{
		g.GravityAcceleration = (float)input.Dot(g.WorldMatrix.Down) * 10;
	}

	Vector3D rot = controller.GetShipVelocities().AngularVelocity * 100 * controller.GetShipVelocities().AngularVelocity.LengthSquared();
	rot += controller.WorldMatrix.Right * controller.RotationIndicator.X * 10;
	rot += controller.WorldMatrix.Up * controller.RotationIndicator.Y * 10;
	rot += controller.WorldMatrix.Backward * controller.RollIndicator * 10;

	foreach (IMyGyro gyro in gyros)
	{
		gyro.Yaw = (float)rot.Dot(gyro.WorldMatrix.Up);
		gyro.Pitch = (float)rot.Dot(gyro.WorldMatrix.Right);
		gyro.Roll = (float)rot.Dot(gyro.WorldMatrix.Backward);
	   // (controller as IMyCockpit).GetSurface(0).WriteText("Y:" + (float)rot.Dot(gyro.WorldMatrix.Up));
	}

}

void EngineON(bool On)
{

	foreach (IMyArtificialMassBlock b in mass)
	{
		b.Enabled = On;
	}
	foreach (IMyGravityGenerator b in gravs)
	{
		b.Enabled = On;
		b.GravityAcceleration = 0f;
	}
	foreach (IMyGyro b in gyros)
	{
		b.GyroOverride = On;
		b.Yaw = 0f;
		b.Pitch = 0f;
		b.Roll = 0f;
	}
	foreach (IMyGyro b in stab_gyros)
	{
		b.GyroOverride = On;
		b.Yaw = 0f;
		b.Pitch = 0f;
		b.Roll = 0f;
	}
}

//---------
