using System.Threading.Tasks;

namespace BitMono.Obfuscation.API
{
    public interface IModuleDefMDCreator
    {
        Task<ModuleDefMDCreationResult> CreateAsync();
    }
}