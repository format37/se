// https://steamcommunity.com/sharedfiles/filedetails/?id=2946608505
bool DoorOpened;

public Program()
{
    if (bool.TryParse(Storage, out DoorOpened) == false)
    {
        // Default value if storage is empty or contains invalid data
        DoorOpened = false;
    }
}

public void Main(string argument, UpdateType updateSource)
{
    // Get the door block
    IMyDoor door = GridTerminalSystem.GetBlockWithName("ElevatorDoor") as IMyDoor;
    IMyPistonBase piston = GridTerminalSystem.GetBlockWithName("ElevatorPiston") as IMyPistonBase;
    
    bool elevator_is_moving = false;

    // Check if the door is open
    if (door.OpenRatio > 0) {
        if (!DoorOpened) Echo("Door is open");
        DoorOpened = true;
    } else {
        if (DoorOpened) {
            Echo("Door is closed");
            piston.Reverse();
            elevator_is_moving = true;
        }
        DoorOpened = false;
    }

    if (DoorOpened == false && elevator_is_moving == false) {
        float currentDistance = piston.CurrentPosition;
        float targetDistance = 7f; // The target distance in meters
        if (currentDistance >= targetDistance || currentDistance <=0) {
            door.OpenDoor();
        }
    }

    // Save the boolean variable to storage
    Storage = DoorOpened.ToString();
}
