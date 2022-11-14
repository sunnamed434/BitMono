| Versions: |
| - |
| [v0.3.0-alpha](#v030-alpha5) |
| [v0.2.2-alpha](#v022-alpha4) |
| [v0.2.1-alpha](#v021-alpha3) |
| [v0.1.3-alpha](#v013-alpha2) |
| [v0.1.2-alpha](#v012-alpha1) |
| [v0.1.0](#v010) |

---

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