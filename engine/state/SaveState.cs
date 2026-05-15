using System.Collections.Generic;

namespace GameWizard.Engine.State;

public class SaveState
{
    public IDictionary<string, string> Flags { get; set; } = new Dictionary<string, string>();
    public IDictionary<string, decimal> Attributes { get; set; } = new Dictionary<string, decimal>();
    public IDictionary<string, IDictionary<string, int>> Bags = new Dictionary<string, IDictionary<string, int>>();
}