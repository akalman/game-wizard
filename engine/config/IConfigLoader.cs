namespace GameWizard.Engine.Config;

public interface IConfigLoader
{
    public T Load<T>(string path);
}