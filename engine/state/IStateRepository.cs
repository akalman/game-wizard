using GameWizard.Engine.Schema.Game;

namespace GameWizard.Engine.State;

public interface IStateRepository
{
    public void Initialize(GameState definition);

    public void Create();
    public void Load(string path);

    public string ReadFlag(string flagId);
    public decimal ReadAttribute(string statId);
    public int NumInBag(string bagId, string itemId);

    public void Update(IStateUpdate update);
}