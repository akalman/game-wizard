using System.Collections.Generic;
using GameWizard.Engine.Schema.Game;

namespace GameWizard.Engine.Database;

public class DatabaseRepository : IDatabaseRepository
{
    private IDictionary<string, GameDb> Databases { get; } = new Dictionary<string, GameDb>();
    private IDictionary<string, DatabaseEntry> EntryCache { get; } = new Dictionary<string, DatabaseEntry>();

    public DatabaseEntry ReadDb(string databaseId, string entryId)
    {
        var cacheKey = $"{databaseId}.{entryId}";

        if (!EntryCache.ContainsKey(cacheKey))
            EntryCache[cacheKey] = new DatabaseEntry(Databases[databaseId].Entries[entryId]);

        return EntryCache[cacheKey];
    }

    public void RegisterDb(string databaseId, GameDb definition)
    {
        Databases[databaseId] = definition;
    }
}