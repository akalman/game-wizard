using System;

namespace GameWizard.Core.Menu;

public class InvalidMenuException : Exception
{
    public InvalidMenuException()
    {
    }

    public InvalidMenuException(string message)
        : base(message)
    {
    }

    public InvalidMenuException(string message, Exception inner)
        : base(message, inner)
    {
    }
}