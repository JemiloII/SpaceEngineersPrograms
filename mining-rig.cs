string LocalGridConnector = "Mining Connector";
string LocalGridDrillGroup = "Mining Elite Drills";
string LocalGridPistonGroup = "All Mining Pistons";
string LocalGridRotorGroup = "Mining Advanced Rotors";
string LocalGridConnectorPistonGroup = "Mining Connector Pistons";
string RemoteGridReceiver = "BaseMiningReceiver";
bool isRotorMovingUp = true;

const string WaitingForConnection = "WaitingForConnection";
const string NormalOperation = "Normal";

string currentState = NormalOperation;

public Program()
{
    Runtime.UpdateFrequency = UpdateFrequency.Update100;

    // Load existing settings
    var settings = LoadStorage();

    // Initialize or update rotor direction from storage
    string direction;
    if (settings.TryGetValue("RotorDirection", out direction))
    {
        isRotorMovingUp = direction == "up";
    }
    else
    {
        // Set default if not found
        isRotorMovingUp = true;
        settings["RotorDirection"] = "up"; // Default setting
        SaveStorage(settings);
    }
}

// Serialize key-value pairs into a storage string
void SaveStorage(Dictionary<string, string> data)
{
    Storage = string.Join(";", data.Select(kv => kv.Key + "=" + kv.Value));
}

// Deserialize the storage string back into key-value pairs
Dictionary<string, string> LoadStorage()
{
    return Storage.Split(';')
                  .Where(part => part.Contains('='))
                  .Select(part => part.Split('='))
                  .ToDictionary(split => split[0], split => split[1]);
}

public void Main(string argument, UpdateType updateSource)
{
    ChangeRotorDirection();

    if (argument == "reset")
    {
        currentState = NormalOperation;
    }

    // Get all cargo containers on the same grid as the programmable block
    var containers = new List<IMyCargoContainer>();
    GridTerminalSystem.GetBlocksOfType(containers, container => container.CubeGrid == Me.CubeGrid);

    double totalVolume = 0;
    double currentVolume = 0;

    foreach (var container in containers)
    {
        var inventory = container.GetInventory(0);
        totalVolume += (double)inventory.MaxVolume;
        currentVolume += (double)inventory.CurrentVolume;
    }

    bool isFull = currentVolume >= totalVolume;
    bool isEmpty = currentVolume == 0;

    if (currentState == WaitingForConnection)
    {
        CheckConnectorStatus();
    }
    else if (isFull)
    {
        HandleFullInventory();
    }
    else if (isEmpty)
    {
        HandleEmptyInventory();
    }
}

void HandleFullInventory()
{
    ControlBlockGroup<IMyPistonBase>(LocalGridPistonGroup, piston => piston.Enabled = false);
    ControlBlockGroup<IMyMotorStator>(LocalGridRotorGroup, rotor => rotor.RotorLock = true);
    ControlBlockGroup<IMyMotorStator>(LocalGridRotorGroup, rotor => rotor.Enabled = false);
    ControlBlockGroup<IMyShipDrill>(LocalGridDrillGroup, drill => drill.Enabled = false);
    ControlBlockGroup<IMyPistonBase>(LocalGridConnectorPistonGroup, piston => piston.Extend());
    IGC.SendBroadcastMessage(RemoteGridReceiver, "Extend", TransmissionDistance.TransmissionDistanceMax);

    var connector = GridTerminalSystem.GetBlockWithName(LocalGridConnector) as IMyShipConnector;
    if (connector != null)
    {
        connector.Connect();
        currentState = WaitingForConnection;
    }
}

void HandleEmptyInventory()
{
    var connector = GridTerminalSystem.GetBlockWithName(LocalGridConnector) as IMyShipConnector;
    if (connector != null)
    {
        connector.Disconnect();
    }
    IGC.SendBroadcastMessage(RemoteGridReceiver, "Retract", TransmissionDistance.TransmissionDistanceMax);
    ControlBlockGroup<IMyPistonBase>(LocalGridConnectorPistonGroup, piston => piston.Retract());
    ControlBlockGroup<IMyShipDrill>(LocalGridDrillGroup, drill => drill.Enabled = true);
    ControlBlockGroup<IMyMotorStator>(LocalGridRotorGroup, rotor => rotor.RotorLock = false);
    ControlBlockGroup<IMyMotorStator>(LocalGridRotorGroup, rotor => rotor.Enabled = true);
    ControlBlockGroup<IMyPistonBase>(LocalGridPistonGroup, piston => piston.Enabled = true);
    currentState = NormalOperation;
}

void CheckConnectorStatus()
{
    var connector = GridTerminalSystem.GetBlockWithName(LocalGridConnector) as IMyShipConnector;
    if (connector != null && connector.Status == MyShipConnectorStatus.Connected)
    {
        currentState = NormalOperation; // Change state back to normal after connection is confirmed
        // Here you might add further actions that should take place once the connection is confirmed
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

    var blocks = new List<T>();
    group.GetBlocksOfType(blocks);
    foreach (var block in blocks)
    {
        action(block);
    }
}

void ChangeRotorDirection()
{
    var rotor = GridTerminalSystem.GetBlockWithName("Mining Advanced Rotor Top") as IMyMotorStator;
    if (rotor == null)
    {
        Echo("Rotor not found!");
        return;
    }

    double currentAngle = MathHelper.ToDegrees(rotor.Angle);
    Echo($"Current Rotor Angle: {currentAngle} degrees");

    bool shouldMoveUp = currentAngle < 160;
    bool shouldMoveDown = currentAngle > 200;
    var settings = LoadStorage();

    if (shouldMoveUp && !isRotorMovingUp)
    {
        rotor.TargetVelocityRPM = Math.Abs(rotor.TargetVelocityRPM);
        isRotorMovingUp = true;
        settings["RotorDirection"] = "up"; // Update setting
        Echo("Rotor direction changed to move up.");
    }
    else if (shouldMoveDown && isRotorMovingUp)
    {
        rotor.TargetVelocityRPM = -Math.Abs(rotor.TargetVelocityRPM);
        isRotorMovingUp = false;
        settings["RotorDirection"] = "down"; // Update setting
        Echo("Rotor direction changed to move down.");
    }

    SaveStorage(settings);
}
