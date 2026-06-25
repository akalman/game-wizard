using System;
using System.Collections.Generic;
using GameWizard.Engine.Schema.Game;

namespace GameWizard.Engine.State;

public abstract class StateFacade
{
    private IDictionary<Type, Action<GameState, SaveState, IStateUpdate>> Updaters =
        new Dictionary<Type, Action<GameState, SaveState, IStateUpdate>>();

    public bool Update(GameState definition, SaveState state, IStateUpdate update)
    {
        if (!Updaters.ContainsKey(update.GetType()))
            return false;

        Updaters[update.GetType()](definition, state, update);
        return true;
    }

    protected void RegisterUpdater<T>(StateFacadeUpdateFunc<T> updateFn) where T : class, IStateUpdate
    {
        Updaters.Add(typeof(T), (game, state, update) => updateFn(game, state, update as T));
    }
}

public delegate void StateFacadeUpdateFunc<in T>(GameState definition, SaveState state, T update) where T : IStateUpdate;