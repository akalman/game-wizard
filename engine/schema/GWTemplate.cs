using System.Collections.Generic;

namespace GameWizard.Engine.Schema;

public class GWTemplate
{
    public string TemplateId { get; set; }
    public string ScenePath { get; set; }
    public IList<string> Inputs { get; set; }
    public IList<string> EdgeTypes { get; set; }
}