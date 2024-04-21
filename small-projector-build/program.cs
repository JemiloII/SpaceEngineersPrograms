string prefix = "Build 01";
List<string> additionalLCDNames = new List<string> {
    // "BlockName:SurfaceIndex"
    // "Four Button Panel:4" 
};

IMyProjector projector;
IMyShipConnector connector;
IMyShipWelder welder;
IMyTextSurfaceProvider buttonPanel;
List<IMyTextPanel> cautionLCDs = new List<IMyTextPanel>();

public Program()
{
    Initialize();
    Runtime.UpdateFrequency = UpdateFrequency.Update10;
}

public void Main(string argument, UpdateType updateSource)
{
    string buildPercentage = CalculateBuildPercentage();

    UpdateLCDsMe(buildPercentage);
    UpdateAdditionalLCDs(buildPercentage);

    UpdateButtonPanelLCDs();
    CheckProjectorStatus();
    CheckConnectorStatus();
    CheckWelderStatus();
    UpdateCautionLCDs();
}

string CalculateBuildPercentage()
{
    if (projector != null && projector.IsProjecting)
    {
        double percentage = projector.RemainingBlocks * 100.0 / projector.TotalBlocks;
        return $"Build {percentage:F0}%";
    }
    return $"Build 0%";
}

void UpdateLCDsMe(string text, bool multilines = false)
{
    var surface = Me.GetSurface(0);
    surface.WriteText(text.Replace(" ", "\n"));
}

void UpdateAdditionalLCDs(string text, bool multilines = false, bool usePrefix = true)
{
    string modifiedText = multilines ? text.Replace(" ", "\n") : text;
    foreach (string lcdName in additionalLCDNames)
    {
        var parts = lcdName.Split(':');
        string blockName = parts[0];
        int surfaceIndex = parts.Length > 1 ? int.Parse(parts[1]) : 0;
        
        if (usePrefix) {
            blockName = $"{prefix} {blockName}";
        }

        IMyTextSurfaceProvider block = GridTerminalSystem.GetBlockWithName(blockName) as IMyTextSurfaceProvider;
        if (block != null)
        {
            IMyTextSurface surface = block.GetSurface(surfaceIndex);
            surface.WriteText(modifiedText);
        }
        else
        {
            if (usePrefix) {
                UpdateAdditionalLCDs(text, multilines, false);
            } else {
                Echo($"ERROR: LCD block '{blockName}' not found!");
                break;
            }
        }
    }
}

void UpdateButtonPanelLCDs()
{
    // Update each LCD Background based on the corresponding system's status
    UpdateButtonPanelLCD(0, projector.Enabled ? Color.Green : Color.Red, "Projector");
    UpdateButtonPanelLCD(1, welder.Enabled ? Color.Green : Color.Red, "Welder");
    UpdateButtonPanelLCD(2, GetConnectorStatusColor(connector.Status), "Connector");
}

void UpdateButtonPanelLCD(int buttonIndex, Color color, string text)
{
    IMyTextSurface surface = buttonPanel.GetSurface(buttonIndex);
    surface.BackgroundColor = color;
    surface.WriteText(text);
}

void UpdateCautionLCDs()
{
    foreach (var lcd in cautionLCDs)
    {
        lcd.Enabled = welder.Enabled;
    }
}

Color GetConnectorStatusColor(MyShipConnectorStatus status)
{
    switch (status)
    {
        case MyShipConnectorStatus.Connected:
            return Color.Green;
        case MyShipConnectorStatus.Connectable:
            return Color.Yellow;
        default:
            return Color.Red;
    }
}

void CheckProjectorStatus()
{
    if (projector.IsProjecting && projector.RemainingBlocks == 0)
    {
        connector.Connect();
        welder.Enabled = false;
    }
    UpdateButtonPanelLCD(0, projector.Enabled ? Color.Green : Color.Red, "Projector");
}

void CheckConnectorStatus()
{
    UpdateButtonPanelLCD(2, GetConnectorStatusColor(connector.Status), "Connector");
}

void CheckWelderStatus()
{
    UpdateButtonPanelLCD(1, welder.Enabled ? Color.Green : Color.Red, "Welder");
}

void Initialize() 
{
    string output = "";

    projector = GridTerminalSystem.GetBlockWithName(prefix + " Projector") as IMyProjector;
    if (projector == null) output += "ERROR: Projector block '" + prefix + " Projector' not found!\n";
    else output += "Projector initialized: " + projector.CustomName + "\n";

    connector = GridTerminalSystem.GetBlockWithName(prefix + " Connector") as IMyShipConnector;
    if (connector == null) output += "ERROR: Connector block '" + prefix + " Connector' not found!\n";
    else output += "Connector initialized: " + connector.CustomName + "\n";

    welder = GridTerminalSystem.GetBlockWithName(prefix + " Welder") as IMyShipWelder;
    if (welder == null) output += "ERROR: Welder block '" + prefix + " Welder' not found!\n";
    else output += "Welder initialized: " + welder.CustomName + "\n";

    buttonPanel = GridTerminalSystem.GetBlockWithName(prefix + " Panel Buttons") as IMyTextSurfaceProvider;
    if (buttonPanel == null) output += "ERROR: Button panel block '" + prefix + " Panel Buttons' not found!\n";
    else output += "Button panel block initialized.\n";

    IMyBlockGroup lcdGroup = GridTerminalSystem.GetBlockGroupWithName(prefix + " Caution LCDs");
    if (lcdGroup == null) output += "ERROR: LCD group '" + prefix + " Caution LCDs' not found!\n";
    else {
        lcdGroup.GetBlocksOfType(cautionLCDs);
        output += "Caution LCD group initialized with " + cautionLCDs.Count + " LCD(s).\n";
    }

    Echo(output);
}
