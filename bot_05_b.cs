public void Main(string argument, UpdateType updateSource)
{
            // Params
            int RotorSpeed = 8;
            int Rotor_lower_limit = -45;
            int Rotor_upper_limit = 45;
            int DisplacementMin = 7; //leg high
            int DisplacementMax = 11;
            int direction = 0; //-1 back; 0 twin; 1 front;
            int minutes_rotation = 2; //direction change time interval
            
            // Lcd init
            var lcd = GridTerminalSystem.GetBlockWithName("BotPanel") as IMyTextPanel;
            lcd.WriteText("#"+argument);

            // Mech init
            var RotorL = GridTerminalSystem.GetBlockWithName("RotorL") as IMyMotorStator;
            var RotorR = GridTerminalSystem.GetBlockWithName("RotorR") as IMyMotorStator;
            var ChasisL = GridTerminalSystem.GetBlockWithName("ChasisL") as IMyLandingGear;
            var ChasisR = GridTerminalSystem.GetBlockWithName("ChasisR") as IMyLandingGear;

            ChasisL.AutoLock = false;
            ChasisR.AutoLock = false;

            RotorL.UpperLimitDeg = Rotor_upper_limit;
            RotorL.LowerLimitDeg = Rotor_lower_limit;
            RotorR.UpperLimitDeg = Rotor_upper_limit;
            RotorR.LowerLimitDeg = Rotor_lower_limit;

            if (argument=="ButtonFrontL")
            {
                RotorL.Displacement = DisplacementMax;
                RotorR.Displacement = DisplacementMax;
                RotorL.TargetVelocityRPM = RotorSpeed;
                RotorR.TargetVelocityRPM = RotorSpeed;
            }
            if (argument=="ButtonFrontR")
            {
                RotorL.Displacement = DisplacementMax;
                RotorR.Displacement = DisplacementMax;
                RotorL.TargetVelocityRPM = RotorSpeed * (-1);
                RotorR.TargetVelocityRPM = RotorSpeed * (-1);
            }       
            if (argument=="ButtonL")
            {
                if (ChasisL.IsLocked) ChasisL.GetActionWithName("Unlock").Apply(ChasisL);
                else ChasisL.GetActionWithName("Lock").Apply(ChasisL);
            } 
            if (argument=="ButtonR")
            {
                if (ChasisR.IsLocked) ChasisR.GetActionWithName("Unlock").Apply(ChasisR);
                else ChasisR.GetActionWithName("Lock").Apply(ChasisR);
            } 
            if (argument=="timer")
            {
            
            // Time
            var Time = System.DateTime.Now;

            // Counters
            int t_minutes = Time.Minute % minutes_rotation;
            int t10 = Time.Second % 10;
            int t20 = Time.Second % 20;
            int t60 = Time.Second;

            // direction
            int speed_multipler_a = 1;
            int speed_multipler_b = 1;//-1;

            // Lcd print
            lcd.WriteText("10: " + t10.ToString() + "  20: " + t20.ToString(), true);
            lcd.WriteText("\n60: " + t60.ToString() + "\nminutes: " + t_minutes.ToString(), true);


            
            if (direction==0)
            {
            if (t_minutes==0)
            {
                lcd.WriteText("\nforward", true);
                speed_multipler_a = 1;
                speed_multipler_b = -1;
            }
            else
            {
                lcd.WriteText("\nbackward", true);
                speed_multipler_a = -1;
                speed_multipler_b = 1;
            } 
            }
            if (direction==-1) {
               lcd.WriteText("\nbackward", true);
                speed_multipler_a = -1;
                speed_multipler_b = 1;
             }
            if (direction==1) {
               lcd.WriteText("\nforward", true);
                speed_multipler_a = 1;
                speed_multipler_b = -1;
             }
            
            if (t20 == 0 || t20 == 1) // A.0
            {
                RotorR.Displacement = DisplacementMax;
            }
            if (t20 == 2 || t20 == 3) // A.1
            {
                ChasisR.GetActionWithName("Lock").Apply(ChasisR);
                ChasisL.GetActionWithName("Unlock").Apply(ChasisL);
                RotorL.Displacement = DisplacementMin;
                RotorL.TargetVelocityRPM = RotorSpeed * speed_multipler_a;
                RotorR.TargetVelocityRPM = RotorSpeed * speed_multipler_a;
            }
            if (t20 == 10 || t20 == 11) // B.0
            {
                RotorL.Displacement = DisplacementMax;
            }
            if (t20 == 12 || t20 == 13) // B.1
            {
                ChasisL.GetActionWithName("Lock").Apply(ChasisL);
                ChasisR.GetActionWithName("Unlock").Apply(ChasisR);
                RotorR.Displacement = DisplacementMin;
                RotorL.TargetVelocityRPM = RotorSpeed * speed_multipler_b;
                RotorR.TargetVelocityRPM = RotorSpeed * speed_multipler_b;
            }
       
         

    }
}
