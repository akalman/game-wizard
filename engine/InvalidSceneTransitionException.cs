using System;

namespace GameWizard.Engine;

public class InvalidSceneTransitionException : Exception
{
    public InvalidSceneTransitionException() { }

    public InvalidSceneTransitionException(string message)
        : base(message) { }

    public InvalidSceneTransitionException(string message, Exception inner)
        : base(message, inner) { }
}