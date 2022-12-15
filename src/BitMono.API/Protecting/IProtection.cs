namespace BitMono.API.Protecting;

public interface IProtection
{
    Task ExecuteAsync(ProtectionContext context, ProtectionParameters parameters, CancellationToken cancellationToken = default);
}