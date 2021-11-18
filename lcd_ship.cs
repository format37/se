public Program()
{
    Runtime.UpdateFrequency = UpdateFrequency.Update100;
}

public void Main(string argument, UpdateType updateSource)
{
    // Lcd init
    var cabin_panel = GridTerminalSystem.GetBlockWithName("cabin_panel") as IMyTextPanel;
    //var cabin_panel = GridTerminalSystem.GetBlockWithName("Flight Seat 5") as IMyTextPanel;
    cabin_panel.WriteText("Бройлер 747");

    // Time
    var Time = System.DateTime.Now;
    cabin_panel.WriteText("\n"+Time.ToString(),true);
    
    // Energy
    var bat_01 = GridTerminalSystem.GetBlockWithName("Amb_Battery_01") as IMyBatteryBlock;
    var bat_02 = GridTerminalSystem.GetBlockWithName("Amb_Battery_02") as IMyBatteryBlock;
    var stored = bat_01.CurrentStoredPower;
    stored += bat_02.CurrentStoredPower;
    var bat_p_max = bat_01.MaxStoredPower;
    bat_p_max += bat_02.MaxStoredPower;
    var energy = Math.Round(bat_p_max/stored*100);    
    cabin_panel.WriteText("\nЗаряд: "+(energy).ToString()+"%",true);

    //Hydrogen
    var hydrogen_a = GridTerminalSystem.GetBlockWithName("HydrogenTankA") as IMyGasTank;
    var hydrogen_b = GridTerminalSystem.GetBlockWithName("HydrogenTankB") as IMyGasTank;
    var hydrogen = Math.Round((hydrogen_a.FilledRatio+hydrogen_b.FilledRatio)/2*100);
    cabin_panel.WriteText("\nВодород: "+(hydrogen.ToString())+"%",true);

    // Oxygen
    var oxygenTank = GridTerminalSystem.GetBlockWithName("OxygenTank") as IMyGasTank;
    var oxygen = Math.Round(oxygenTank.FilledRatio*100);
    cabin_panel.WriteText("\nКислород: "+(oxygen.ToString())+"%",true);
    
}
