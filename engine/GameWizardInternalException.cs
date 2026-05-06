using System;

namespace GameWizard.Engine;

public class GameWizardInternalException : Exception
{
    public GameWizardInternalException() { }

    public GameWizardInternalException(string message)
        : base(message) { }

    public GameWizardInternalException(string message, Exception inner)
        : base(message, inner) { }
}