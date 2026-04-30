using System.Collections.Generic;

namespace GameWizard.Engine.Schema;

public class GWStateFlag
{
    public string Id { get; set; }
    public IList<string> Values { get; set; }
    public string InitialValue { get; set; }
}