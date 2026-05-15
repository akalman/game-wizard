using System.Collections.Generic;

namespace GameWizard.Engine.Schema.Game;

public class GameDb
{
    public IList<string> Interfaces { get; set; } = new List<string>();
    public IDictionary<string, DbFieldTypes> Fields { get; set; } = new Dictionary<string, DbFieldTypes>();

    public IDictionary<string, IDictionary<string, string>> Entries { get; set; } =
        new Dictionary<string, IDictionary<string, string>>();
}

public enum DbFieldTypes
{
    String,
    Number,
    Flag,
    Attribute,
    Bag,
    Database,
}