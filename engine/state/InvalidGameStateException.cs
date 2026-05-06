using System;

namespace GameWizard.Engine.State;

public class InvalidGameStateException : Exception
{
    public InvalidGameStateException() { }

    public InvalidGameStateException(string message)
        : base(message) { }

    public InvalidGameStateException(string message, Exception inner)
        : base(message, inner) { }
}