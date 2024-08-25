namespace BitMono.Core.Attributes;

/// <summary>
/// This is necessary to make native code work inside the assembly.
/// See more here: https://docs.washi.dev/asmresolver/guides/dotnet/unmanaged-method-bodies.html
/// However, sometimes it causes issues with the assembly like <see cref="BadImageFormatException"/>,
/// that's why you need to manually mark your <see cref="Protection"/> with <see cref="ConfigureForNativeCodeAttribute"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class ConfigureForNativeCodeAttribute : Attribute;