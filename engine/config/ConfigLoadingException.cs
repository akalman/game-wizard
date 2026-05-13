using System;

namespace GameWizard.Engine.Config;

public class ConfigLoadingException : Exception
{
    public ConfigLoadingException() { }

    public ConfigLoadingException(string message)
        : base(message) { }

    public ConfigLoadingException(string message, Exception inner)
        : base(message, inner) { }
}