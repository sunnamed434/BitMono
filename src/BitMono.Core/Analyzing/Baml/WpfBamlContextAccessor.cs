using BitMono.Core.Services;

namespace BitMono.Core.Analyzing.Baml;

/// <summary>
/// Builds and caches the <see cref="WpfBamlContext"/> for the module being obfuscated, so the
/// critical analyzer (which decides what to keep) and the renamer (which performs the BAML rewrite)
/// share one parse.
/// </summary>
public class WpfBamlContextAccessor
{
    private readonly IEngineContextAccessor _engineContextAccessor;
    private readonly ObfuscationSettings _obfuscationSettings;
    private ModuleDefinition? _module;
    private WpfBamlContext? _context;

    public WpfBamlContextAccessor(IEngineContextAccessor engineContextAccessor, ObfuscationSettings obfuscationSettings)
    {
        _engineContextAccessor = engineContextAccessor;
        _obfuscationSettings = obfuscationSettings;
    }

    public WpfBamlContext? GetContext()
    {
        var module = _engineContextAccessor.Instance?.Module;
        if (module == null)
        {
            return null;
        }
        if (!ReferenceEquals(_module, module) || _context == null)
        {
            _module = module;
            _context = WpfBamlContext.Build(module, _obfuscationSettings.WpfBamlRewrite);
        }
        return _context;
    }
}
