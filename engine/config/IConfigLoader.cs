namespace GameWizard.Engine.Config;

public interface IConfigLoader
{
    public T Load<T>(string path);
    public T Convert<T>(string path);
}

public interface IConfigLoader<in T> : IConfigLoader
{
    public void RegisterDeserializer(T deserializer);
}