namespace GameWizard.Engine.Schema;

public class GWStateChange
{
    public GWStateChangeType Type { get; set; }
    public string Target { get; set; }
    public string Value { get; set; }
}