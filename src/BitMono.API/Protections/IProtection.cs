namespace BitMono.API.Protections;

public interface IProtection
{
    Task ExecuteAsync();
}