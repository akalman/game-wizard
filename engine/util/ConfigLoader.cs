namespace GameWizard.Engine.Util;

public interface ConfigLoader
{
    public T Load<T>(string path);
}