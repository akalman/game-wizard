using System;

namespace GameWizard.Engine.State;

public class InvalidSceneTransitionException : Exception
{
    public InvalidSceneTransitionException() { }

    public InvalidSceneTransitionException(string message)
        : base(message) { }

    public InvalidSceneTransitionException(string message, Exception inner)
        : base(message, inner) { }
}