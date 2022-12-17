| Versions: |
| - |
| [v0.5.0-alpha](#v050-alpha14) |
| [v0.4.4-alpha](#v044-alpha13) |
| [v0.4.3-alpha](#v043-alpha12) |
| [v0.4.2-alpha](#v042-alpha11) |
| [v0.4.1-alpha](#v041-alpha10) |
| [v0.4.0-alpha](#v040-alpha9) |
| [v0.3.3-alpha](#v033-alpha8) |
| [v0.3.2-alpha](#v032-alpha7) |
| [v0.3.1-alpha](#v031-alpha6) |
| [v0.3.0-alpha](#v030-alpha5) |
| [v0.2.2-alpha](#v022-alpha4) |
| [v0.2.1-alpha](#v021-alpha3) |
| [v0.1.3-alpha](#v013-alpha2) |
| [v0.1.2-alpha](#v012-alpha1) |
| [v0.1.0](#v010) |

---
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

### v0.4.2-alpha.11:
#### Fixed:
* Assembly resolving information and exceptions handling

### v0.4.1-alpha.10:
#### Changed:
* DotNetHook Protection workflow 
* StringsEncryption Protection workflow as well as data encrypting

#### Fixed:
* Obfuscation process

### v0.4.0-alpha.9:
#### Added:
* BitDateTimeStamp Protection

#### Fixed:
* Obfuscation process

### v0.3.3-alpha.8:
#### Changed:
* BitDotNet protection greatly improved

#### Fixed:
* Obfuscation process

### v0.3.2-alpha.7:
#### Fixed:
* Specifying file

### v0.3.1-alpha.6:
#### Changed:
* Guarantees loading libraries directly as bytes

### Fixed:
* Now BitMono.CLI catches errors and outputs it properly [#31](https://github.com/sunnamed434/BitMono/issues/31)
* Protections now outputs logs properly
* Protections bugs fixes

### v0.3.0-alpha.5:
### Added:
* Feature that allows to protect types/methods with the specified namespace in obfuscation.json configuration [#27](https://github.com/sunnamed434/BitMono/issues/27)

### Changed:
* FieldsHiding protection currently is deprecated and shouldn`t be used anymore

#### Fixed:
* AntiDebugBreakpoints Protection
* BitMethodDotnet Protection
* FullRenamer Protection
* CallToCalli Protection
* NoNamespaces Protection
* ObjectReturnType Protection
* StringsEncryption Protection

### v0.2.2-alpha.4:
#### Fixed:
* CLI + GUI
* Obfuscation process

### v0.2.1-alpha.3:
#### Changed:
* A lot of configurations moved from appsettings.json to separte configurations (obfuscation.json)
* Optimized obfuscation process
* Now libraries as well known as dependencies should be in directory `libs` instead of base

#### Fixed:
* DotNetHook Protection
* obfuscation.json configuration
* Module drag-and-dropping
* GUI version.

#### Removed:
* `base` directory

### v0.1.3-alpha.2:
#### Added:
* Now it is possible to obfuscate only specific namespace(s) [#27](https://github.com/sunnamed434/BitMono/issues/27).

#### Fixed:
* DotNetHook Protection

### v0.1.2-alpha.1:
#### Fixed:
* DotNetHook Protection

### v0.1.0:
* Pre-Release of BitMono.