namespace GameWizard.Engine.Schema;

public class GWState
{
    public string StateId { get; set; }
    public string Template { get; set; }
    public string Config { get; set; }
    public bool IsOverlay { get; set; }
    public bool HaltsInputs { get; set; }
}