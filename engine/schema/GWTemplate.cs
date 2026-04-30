using System.Collections.Generic;

namespace GameWizard.Engine.Schema;

public class GWTemplate
{
    public string Id { get; set; }
    public string Scene { get; set; }
    public IList<string> Inputs { get; set; } = new List<string>();
    public IDictionary<string, GWEventLifecycle> Events { get; set; } = new Dictionary<string, GWEventLifecycle>();
}