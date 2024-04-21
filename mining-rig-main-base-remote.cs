IMyBroadcastListener listener;

public Program()
{
    // Register a listener for the specific tag
    listener = IGC.RegisterBroadcastListener("BaseMiningReceiver");
    listener.SetMessageCallback("BaseMiningReceiver"); // Triggers 'Main' when a message is received
}

void Main(string argument, UpdateType updateSource)
{
    if ((updateSource & UpdateType.IGC) != 0) // Check if the trigger is an IGC message
    {
        while (listener.HasPendingMessage)
        {
            MyIGCMessage message = listener.AcceptMessage();
            if (message.Data.ToString() == "Extend")
            {
                // Handle extend action
                Echo("Received 'Extend' command.");
                ControlBlockGroup<IMyPistonBase>("Base Mining Connector Pistons", piston => piston.Extend());
            }
            else if (message.Data.ToString() == "Retract")
            {
                // Handle retract action
                Echo("Received 'Retract' command.");
                ControlBlockGroup<IMyPistonBase>("Base Mining Connector Pistons", piston => piston.Retract());
            }
        }
    }
}

void ControlBlockGroup<T>(string groupName, Action<T> action) where T : class, IMyTerminalBlock
{
    var group = GridTerminalSystem.GetBlockGroupWithName(groupName);
    if (group == null) 
    {
        Echo("Group not found: " + groupName);
        return;
    }
    
    Echo("Found Group:" + groupName);

    var blocks = new List<T>();
    group.GetBlocksOfType(blocks, block => true);
    foreach (var block in blocks)
    {
        action(block);
    }
}
