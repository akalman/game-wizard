namespace GameWizard.Engine.Config;

public interface IConfigRepository
{
    public T Read<T>(string path);
}