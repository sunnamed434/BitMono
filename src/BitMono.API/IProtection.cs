namespace BitMono.API;

public interface IProtection
{
    Task ExecuteAsync(ProtectionParameters parameters);
}