namespace GameWizard.Engine.Util;

public interface IConfigLoader
{
    public T Load<T>(string path);
}