using System.Collections.Generic;
using GameWizard.Engine.Config;
using GameWizard.Engine.Schema.Game;

namespace GameWizard.Engine.Database;

public class DatabaseRepository(IConfigRepository fieldDeserializer) : IDatabaseRepository
{
    private IConfigRepository FieldDeserializer { get; } = fieldDeserializer;

    private IDictionary<string, GameDb> Databases { get; } = new Dictionary<string, GameDb>();
    private IDictionary<string, DatabaseEntry> EntryCache { get; } = new Dictionary<string, DatabaseEntry>();

    public DatabaseEntry ReadDb(string databaseId, string entryId)
    {
        var cacheKey = $"{databaseId}.{entryId}";

        if (!EntryCache.ContainsKey(cacheKey))
        {
            var definition = Databases[databaseId];
            var entry = definition.Entries[entryId];
            EntryCache[cacheKey] = new DatabaseEntry(definition, entry, FieldDeserializer);
        }

        return EntryCache[cacheKey];
    }

    public void RegisterDb(string databaseId, GameDb definition)
    {
        Databases[databaseId] = definition;
    }
}