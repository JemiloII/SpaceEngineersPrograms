string prefix = "Build 01";

IMyProjector projector;
IMyShipConnector connector;
IMyShipWelder welder;
IMyButtonPanel buttonPanel;

public Program()
{
    Initialize();
    Runtime.UpdateFrequency = UpdateFrequency.Update10;
}

public void Main(string argument, UpdateType updateSource)
{
    if (projector == null || connector == null || welder == null || buttonPanel == null) {
        Echo("Initialization error, please check block names and group names.");
        return;
    }

    UpdateButtonPanelLCDs();
    CheckProjectorStatus();
    CheckConnectorStatus();
    CheckWelderStatus();
}

void UpdateButtonPanelLCDs()
{
    UpdateButtonPanelLCD(0, projector.Enabled ? Color.Green : Color.Red, "Projector");
    UpdateButtonPanelLCD(1, welder.Enabled ? Color.Green : Color.Red, "Welder");
    UpdateButtonPanelLCD(2, GetConnectorStatusColor(connector.Status), "Connector");
}

void UpdateButtonPanelLCD(int buttonIndex, Color color, string text)
{
    var surface = buttonPanel.GetSurface(buttonIndex);
    surface.BackgroundColor = color;
    surface.WriteText(text);
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

    buttonPanel = GridTerminalSystem.GetBlockWithName(prefix + " Panel Buttons") as IMyButtonPanel;
    if (buttonPanel == null) output += "ERROR: Button panel block '" + prefix + " Panel Buttons' not found!\n";
    else output += "Button panel block initialized.\n";

    Echo(output);
}