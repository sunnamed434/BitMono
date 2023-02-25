namespace BitMono.API.Protecting;

public interface IProtection
{
    Task ExecuteAsync(ProtectionParameters parameters);
}