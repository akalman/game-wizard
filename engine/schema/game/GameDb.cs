using System.Collections.Generic;

namespace GameWizard.Engine.Schema.Game;

public class GameDb
{
    public IList<string> Interfaces { get; set; } = new List<string>();
    public IDictionary<string, DbFieldTypes> Fields { get; set; } = new Dictionary<string, DbFieldTypes>();

    public IDictionary<string, DbEntry> Entries { get; set; } =
        new Dictionary<string, DbEntry>();
}

public enum DbFieldTypes
{
    String,
    Number,
    Flag,
    Attribute,
    Bag,
    Database,
    StringList,
    DatabaseList,
}

public class DbEntry
{
    public IDictionary<string, string> SimpleFields { get; set; } = new Dictionary<string, string>();
    public IDictionary<string, IList<string>> CollectionFields { get; set; } = new Dictionary<string, IList<string>>();
}