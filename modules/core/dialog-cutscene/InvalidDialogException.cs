using System;

namespace GameWizard.Core.DialogCutscene.State;

public class InvalidDialogException : Exception
{
    public InvalidDialogException()
    {
    }

    public InvalidDialogException(string message)
        : base(message)
    {
    }

    public InvalidDialogException(string message, Exception inner)
        : base(message, inner)
    {
    }
}