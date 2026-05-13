namespace GameWizard.Engine.Config;

public class ConfigRepository(IConfigLoader loader) : IConfigRepository
{
    private IConfigLoader Loader { get; } = loader;

    public T Read<T>(string path)
    {
        return Loader.Load<T>(path);
    }
}

