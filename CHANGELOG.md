| Versions:                       |
|---------------------------------|
| [v0.14.0-alpha](#v0140-alpha26) |
| [v0.13.1-alpha](#v0131-alpha25) |
| [v0.13.0-alpha](#v0130-alpha24) |
| [v0.12.2-alpha](#v0122-alpha23) |
| [v0.12.1-alpha](#v0121-alpha22) |
| [v0.12.0-alpha](#v0120-alpha21) |
| [v0.11.0-alpha](#v0110-alpha20) |
| [v0.10.0-alpha](#v0100-alpha19) |
| [v0.9.0-alpha](#v090-alpha18)   |
| [v0.8.0-alpha](#v080-alpha17)   |
| [v0.7.0-alpha](#v070-alpha16)   |
| [v0.6.0-alpha](#v060-alpha15)   |
| [v0.4.4-alpha](#v044-alpha13)   |
| [v0.4.3-alpha](#v043-alpha12)   |
| [v0.4.2-alpha](#v042-alpha11)   |
| [v0.4.1-alpha](#v041-alpha10)   |
| [v0.4.0-alpha](#v040-alpha9)    |
| [v0.3.3-alpha](#v033-alpha8)    |
| [v0.3.2-alpha](#v032-alpha7)    |
| [v0.3.1-alpha](#v031-alpha6)    |
| [v0.3.0-alpha](#v030-alpha5)    |
| [v0.2.2-alpha](#v022-alpha4)    |
| [v0.2.1-alpha](#v021-alpha3)    |
| [v0.1.3-alpha](#v013-alpha2)    |
| [v0.1.2-alpha](#v012-alpha1)    |
| [v0.1.0](#v010)                 |

---
### v0.14.0-alpha.26:
2023-04-24
#### Added:
* More docs for developers and users.
* New parameter in `criticals.json` which allows to ignore methods starts with name, i.e `CriticalMethodsStartsWith`.

#### Changed:
* Major changes in Protections APIs.

### v0.13.1-alpha.25:
2023-04-15
#### Fixed:
* Assembly Resolve, which caused problems with PE build.

### v0.13.0-alpha.24:
2023-04-05
#### Added:
* Return obfuscation success & failure status from BitMono.CLI (0 - Success, 1 - Failure), thanks to [techei](https://github.com/techei).
* More docs and answers to the questions.

#### Fixed:
* Assembly resolve [#113](https://github.com/sunnamed434/BitMono/issues/113), thanks to [techei](https://github.com/techei) for the bug report.

#### Changed:
* Docs information about protections.

### v0.12.2-alpha.23:
2023-03-08
#### Fixed:
* Error that caused issues with .NET 7.0 obfuscation (file not found, etc)

### v0.12.1-alpha.22:
2023-03-06
#### Added:
* Test cases for costura decompressor.

#### Fixed:
* Costura decompressor.

### v0.12.0-alpha.21:
2023-03-05
#### Added:
* [Costura-Fody](https://github.com/Fody/Costura) support, now references are resolved automatically, [#102](https://github.com/sunnamed434/BitMono/issues/102)
* Support when path contains quotes (for example: "path..."), [#104](https://github.com/sunnamed434/BitMono/issues/104)

#### Fixed:
* Hiding of the paths (before paths with .exe may cause an ignore)
* Now output directory path message shows normally (before ***\folder_before_output, now ***\output)

### v0.11.0-alpha.20:
2023-02-016
#### Added:
* BitMono ASCII Art in CLI.
* Hiding of the pathes (file path, directory path, etc).
* Documentation.

#### Fixed:
* Error when use mono BitMono.CLI.exe [#93](https://github.com/sunnamed434/BitMono/issues/93)

#### Changed:
* Major changes in whole API of BitMono.

### v0.10.0-alpha.19:
2023-02-013
#### Added:
* Command line arguments [#82](https://github.com/sunnamed434/BitMono/issues/82)

#### Fixed:
* DotNetHook protection

### v0.9.0-alpha.18:
2023-02-09
#### Changed:
* Bumped to the latest version of AsmResolver

#### Fixed
* Errors when launching the BitMono.CLI
* Bug fixes and other minor improvements

### v0.8.0-alpha.17:
2023-01-27
#### Added
* Reflection analysis such as in ConfuserEx [#41](https://github.com/sunnamed434/BitMono/issues/41)
* BitMono protection

#### Changed:
* Moved new protections from BitDotNet to BitMono protection and added support for PE32 and PE32+

#### New Contributors
* [Gibsol](https://github.com/Gibsol) made their first contribution in [#54](https://github.com/sunnamed434/BitMono/pull/54)
* [0x59R11](https://github.com/0x59R11) made their first contribution in [#81](https://github.com/sunnamed434/BitMono/pull/81)

### v0.7.0-alpha.16:
#### Added:
* Unit Tests
* Benchmarks
* Support of ObfuscateAssemblyAttribute
* New properties in obfuscation.json
* Ignore members with specific attribute, eg, [SerializeField], it can be edited in criticals.json

#### Changed:
* Obfuscation process
* ObfuscationAttribute support
* Moved from .NET Framework 461 to .NET Framework 462

#### Fixed:
* [SerializableAttribute] support
* [MethodImpl(MethodImplOptions.NoInlining)] support

### v0.6.0-alpha.15:
#### Added:
* New protection AntiDecompiler
* As more as possible errors logging

#### Changed:
* Migrate dnlib to AsmResolver or Mono.Cecil [#50](https://github.com/sunnamed434/BitMono/issues/50)

#### Fixed:
* BitMethodDotnet Protection
* StringsEncryption Protection
* CallToCalli Protection
* DotNetHook Protection
* Load Runtime Module via file instead of typeof(SomeRuntime).Module [#55](https://github.com/sunnamed434/BitMono/issues/55)
* Rewrite Custom Attributes Resolve as less as a possible reflection [#57](https://github.com/sunnamed434/BitMono/issues/57)
* System.BadImageFormatException: Invalid DOS signature [#45](https://github.com/sunnamed434/BitMono/issues/45)
* Whole Protections execution process
* Protections execution information
* Obfuscation
* Optimized file writing
* Ignoring of targets with [ObfuscationAttribute] and [MethodImpl(MethodImplOptions.NoInlining)]
* Runtime injection and even became better

### v0.5.0-alpha.14:
#### Added:
* Before obfuscation optmizes all method bodies (macros)

#### Fixed:
* BitMethodDotnet Protection
* StringsEncryption Protection

### v0.4.4-alpha.13:
#### Fixed:
* DotNetHook Protection
* StringsEncryption Protection
* CallToCalli Protection
* Obfuscation doesn't saves assembly information such as assembly attributes [#36](https://github.com/sunnamed434/BitMono/issues/36)
* No more module reloading (Module now loads once and writes once)

### v0.4.3-alpha.12:
#### Changed:
* Protections are looks more cleaner than before, no code duplication
* Now BitDotNet is not an protection but packer

#### Fixed:
* Obfuscation process
* Manipulations and saving of Module optimized, most of things store in memory then writes in file instead of every time writing in file and loading module