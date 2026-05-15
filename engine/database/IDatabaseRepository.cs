using GameWizard.Engine.Schema.Game;

namespace GameWizard.Engine.Database;

public interface IDatabaseRepository
{
    public DatabaseEntry ReadDb(string databaseId, string entryId);
    public void RegisterDb(string databaseId, GameDb definition);
}