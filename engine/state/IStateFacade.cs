using System.Collections.Generic;
using GameWizard.Engine.Schema.Game;

namespace GameWizard.Engine.State;

public abstract class StateFacade
{
    private IDictionary<StateUpdateType, StateFacadeUpdateFunc> _updaters;
    private IDictionary<StateUpdateType, StateFacadeUpdateFunc> Updaters
    {
        get => _updaters ??= GetUpdaters();
        set => _updaters = value;
    }

    protected abstract IDictionary<StateUpdateType, StateFacadeUpdateFunc> GetUpdaters();

    public bool Accepts(StateUpdateType type) => Updaters.ContainsKey(type);

    public void Update(GameState definition, SaveState state, StateUpdate update)
    {
        if (!Updaters.ContainsKey(update.Type))
            throw new GameWizardInternalException();

        Updaters[update.Type](definition, state, update);
    }
}

public delegate void StateFacadeUpdateFunc(GameState definition, SaveState state, StateUpdate update);