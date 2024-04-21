public Program()
{
    Runtime.UpdateFrequency = UpdateFrequency.Update100; // Update every 100 ticks
}

public void Main(string argument, UpdateType updateSource)
{
    Echo($"Me.CubeGrid: {Me.CubeGrid}");
    // ListAllInventoryBlocks();
    List<IMyCargoContainer> cargoContainers = new List<IMyCargoContainer>();
    GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(cargoContainers, container => IsTargetContainer(container) && container.CubeGrid == Me.CubeGrid);

    double totalVolume = 0;
    double currentVolume = 0;

    foreach (IMyCargoContainer container in cargoContainers)
    {
        var inventory = container.GetInventory(0);
        totalVolume += (double)inventory.MaxVolume;
        currentVolume += (double)inventory.CurrentVolume;
    }

    string output;
    if (totalVolume > 0)
    {
        double fillPercentage = (currentVolume / totalVolume) * 100;
        output = "Cargo Filled: " + Math.Round(fillPercentage, 2) + "%";
    }
    else
    {
        output = "No cargo containers found or total volume is zero.";
    }

    // Get rotor angle and append it to the output
    string rotorAngle = GetRotorAngle("Mining Advanced Rotor Top");
    output += $"\nRotor Angle: {rotorAngle}°";  // Append rotor angle info

    UpdateLCD("Mining Transparent LCD", output);
    UpdateTextSurface("Mining Flight Seat", output);
    UpdateTextSurface("Info Programmable Block", output);
}

private bool IsTargetContainer(IMyCargoContainer container)
{
    // Check container type by subtype ID using Contains for broader matching
    bool isTargetType = container.BlockDefinition.TypeIdString.Contains("CargoContainer");
    return isTargetType;
}

private void UpdateLCD(string blockName, string text)
{
    IMyTextPanel lcd = GridTerminalSystem.GetBlockWithName(blockName) as IMyTextPanel;
    if (lcd != null)
    {
        lcd.WriteText(text);
    }
    else
    {
        Echo("LCD Panel not found: " + blockName);
    }
}

private void UpdateTextSurface(string blockName, string text)
{
    IMyTextSurfaceProvider surfaceProvider = GridTerminalSystem.GetBlockWithName(blockName) as IMyTextSurfaceProvider;
    if (surfaceProvider != null)
    {
        IMyTextSurface textSurface = surfaceProvider.GetSurface(0); // Assuming the first LCD on the block
        textSurface.ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;
        textSurface.WriteText(text);
    }
    else
    {
        Echo("Text surface provider not found: " + blockName);
    }
}

private void ListAllInventoryBlocks()
{
    List<IMyTerminalBlock> inventoryBlocks = new List<IMyTerminalBlock>();
    GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(inventoryBlocks, block => block.HasInventory);

    StringBuilder output = new StringBuilder();
    foreach (var block in inventoryBlocks)
    {
        string TypeIdString = block.BlockDefinition.TypeIdString;
        bool IsCargoContainer = TypeIdString.Contains("CargoContainer");

        if (IsCargoContainer && Me.CubeGrid == block.CubeGrid)
        {
            output.AppendLine($"Type: {TypeIdString} | Cargo: {IsCargoContainer}");
            output.AppendLine($"ME: {Me.CubeGrid} | CE: {block.CubeGrid}");
        }
    }

    Echo(output.ToString()); // Outputs to the programmable block's console
}

private string GetRotorAngle(string rotorName)
{
    IMyMotorStator rotor = GridTerminalSystem.GetBlockWithName(rotorName) as IMyMotorStator;
    if (rotor != null)
    {
        float RotorAngle = MathHelper.ToDegrees(rotor.Angle);
        return Math.Round(RotorAngle).ToString();
    }
    else
    {
        Echo("Rotor not found: " + rotorName);
        return "N/A";
    }
}
