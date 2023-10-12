# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

| Versions:                     |
|-------------------------------|
| [0.19.0-alpha](#0190-alpha34) |
| [0.18.0-alpha](#0180-alpha33) |
| [0.16.2-alpha](#0162-alpha31) |
| [0.16.1-alpha](#0161-alpha30) |
| [0.16.0-alpha](#0160-alpha29) |
| [0.15.1-alpha](#0151-alpha28) |
| [0.15.0-alpha](#0150-alpha27) |
| [0.14.0-alpha](#0140-alpha26) |
| [0.13.1-alpha](#0131-alpha25) |
| [0.13.0-alpha](#0130-alpha24) |
| [0.12.2-alpha](#0122-alpha23) |
| [0.12.1-alpha](#0121-alpha22) |
| [0.12.0-alpha](#0120-alpha21) |
| [0.11.0-alpha](#0110-alpha20) |
| [0.10.0-alpha](#0100-alpha19) |
| [0.9.0-alpha](#090-alpha18)   |
| [0.8.0-alpha](#080-alpha17)   |
| [0.7.0-alpha](#070-alpha16)   |
| [0.6.0-alpha](#060-alpha15)   |
| [0.4.4-alpha](#044-alpha13)   |
| [0.4.3-alpha](#043-alpha12)   |

---

## 0.19.0-alpha.34
2023-10-12

### Added
- Artifacts creation.
- Automatic release (CI).
- .netstandard2.1 Support.
- More information in `build.md` how to build BitMono.

### Changed
- Removed unused and broken GUI version of BitMono from solution.
- Bumped to the stablest versions of NuGet Packages to don't break the compability with the target framework.
- .csproj style.

### Fixed
- .NET 6.0 Support.

## 0.18.0-alpha.33
2023-09-02

### Added
- Add ForceObfuscation, ReferencesDirectoryName, OutputDirectoryName, and ClearCLI options in the obfuscation.json.
- More detailed info how to build the solution.
- .NET 6.0 Support.

### Fixed
- BitMono drag and drop exception [#135](https://github.com/sunnamed434/BitMono/issues/135).
- First line of the CLI shows working directory [#137](https://github.com/sunnamed434/BitMono/issues/137).
- .NET 5 exceptions [#138](https://github.com/sunnamed434/BitMono/issues/138).

## 0.16.2-alpha.31
2023-07-19

### Fixed

- Exception on the BitMono run [#132](https://github.com/sunnamed434/BitMono/issues/132).

### Changed

- The GitHub Issue Templates are removed now.

## 0.16.1-alpha.30
2023-07-17

### Added

- New docs.

### Fixed

- (UnmanagedString) Add unicode support, fix strings with null characters [#130](https://github.com/sunnamed434/BitMono/pull/130), by [GazziFX](https://github.com/GazziFX)
- Other minor bug fixes.

### Changed

- Bumped to the latest AsmResolver 5.4.0 version.
- Code refactoring and possible bug fixes.

## 0.16.0-alpha.29
2023-05-14

### Added

- New logging info about loaded module (version, PE time date stamp, token, culture and target framework)

### Fixed

- PE Image build errors output

### Changed

- Bumped to latest AsmResolver 5.3.0 version.

## 0.15.1-alpha.28
2023-04-28

### Added

- New logging info with count of enabled/disabled protections.
- New logging info before execution of the protections.

### Fixed

- Bug that caused BitMono.CLI to be crashed.
- UnmanagedString Protection.
- CallToCalli Protection.

### Changed

- RuntimeMonikerAttribute now allowed to be multiple.

## 0.15.0-alpha.27
2023-04-27

### Added

- UnmanagedString Protection.
- More docs.

### Fixed

- DotNetHook Protection.

### Changed

- Major improvements and changes in Engine APIs.
- Removed PreserveAll flag, now obfuscation should be more stable.

## 0.14.0-alpha.26
2023-04-24

### Added

- More docs for developers and users.
- New parameter in `criticals.json` which allows to ignore methods starts with name, i.e `CriticalMethodsStartsWith`.

### Changed

- Major changes in Protections APIs.

## 0.13.1-alpha.25
2023-04-15

### Fixed

- Assembly Resolve, which caused problems with PE build.

## 0.13.0-alpha.24
2023-04-05

### Added

- Return obfuscation success & failure status from BitMono.CLI (0 - Success, 1 - Failure), by [techei](https://github.com/techei)
- More docs and answers to the questions.

#### Fixed

- Assembly resolve [#113](https://github.com/sunnamed434/BitMono/issues/113), by [techei](https://github.com/techei)

### Changed

- Docs information about protections.

## 0.12.2-alpha.23
2023-03-08

### Fixed

- Error that caused issues with .NET 7.0 obfuscation (file not found, etc)

## 0.12.1-alpha.22
2023-03-06

### Added

- Test cases for costura decompressor.

### Fixed

- Costura decompressor.

## 0.12.0-alpha.21
2023-03-05

### Added

- [Costura-Fody](https://github.com/Fody/Costura) support, now references are resolved automatically, [#102](https://github.com/sunnamed434/BitMono/issues/102).
- Support when path contains quotes (for example: "path..."), [#104](https://github.com/sunnamed434/BitMono/issues/104).

### Fixed

- Hiding of the paths (before paths with .exe may cause an ignore).
- Now output directory path message shows normally (before ***\folder_before_output, now ***\output).

## 0.11.0-alpha.20
2023-02-016

### Added

- BitMono ASCII Art in CLI.
- Hiding of the path (file path, directory path, etc).
- Documentation.

### Fixed

- Error when use mono BitMono.CLI.exe [#93](https://github.com/sunnamed434/BitMono/issues/93)

### Changed

- Major changes in whole API of BitMono.

## 0.10.0-alpha.19
2023-02-013

### Added

- Command line arguments [#82](https://github.com/sunnamed434/BitMono/issues/82).

### Fixed

- DotNetHook protection.

## 0.9.0-alpha.18
2023-02-09

### Changed

- Bumped to the latest version of AsmResolver.

### Fixed

- Errors when launching the BitMono.CLI.
- Bug fixes and other minor improvements.

## 0.8.0-alpha.17
2023-01-27

### Added

- Reflection analysis such as in ConfuserEx [#41](https://github.com/sunnamed434/BitMono/issues/41).
- BitMono protection.

### Changed

- Moved new protections from BitDotNet to BitMono protection and added support for PE32 and PE32+.

### Fixed

- README text spelling [#54](https://github.com/sunnamed434/BitMono/pull/54), by [Gibsol](https://github.com/Gibsol).
- .gitignore [#81](https://github.com/sunnamed434/BitMono/pull/81), by [0x59R11](https://github.com/0x59R11).

### 0.7.0-alpha.16

#### Added

- Unit Tests.
- Benchmarks.
- Support of ObfuscateAssemblyAttribute.
- New properties in obfuscation.json config.
- Ignore members with specific attribute, eg, [SerializeField], it can be edited in criticals.json now.

#### Changed

- Obfuscation process.
- ObfuscationAttribute support.
- Moved from .NET Framework 461 to .NET Framework 462

#### Fixed

- [SerializableAttribute] support.
- [MethodImpl(MethodImplOptions.NoInlining)] support.

### 0.6.0-alpha.15

#### Added

- New protection AntiDecompiler.
- As more as possible errors logging.

#### Changed

- Migrate dnlib to AsmResolver or Mono.Cecil [#50](https://github.com/sunnamed434/BitMono/issues/50).

#### Fixed

- BitMethodDotnet Protection.
- StringsEncryption Protection.
- CallToCalli Protection.
- DotNetHook Protection.
- Load Runtime Module via file instead of typeof(SomeRuntime).Module [#55](https://github.com/sunnamed434/BitMono/issues/55).
- Rewrite Custom Attributes Resolve as less as a possible reflection [#57](https://github.com/sunnamed434/BitMono/issues/57).
- System.BadImageFormatException: Invalid DOS signature [#45](https://github.com/sunnamed434/BitMono/issues/45).
- Whole Protections execution process.
- Protections execution information.
- Obfuscation.
- Optimized file writing.
- Ignoring of targets with [ObfuscationAttribute] and [MethodImpl(MethodImplOptions.NoInlining)].
- Runtime injection and even became better.

### 0.5.0-alpha.14

#### Added

- Before obfuscation optimizes all method bodies (macros)

#### Fixed

- BitMethodDotnet Protection.
- StringsEncryption Protection.

### 0.4.4-alpha.13

#### Fixed

- DotNetHook Protection.
- StringsEncryption Protection.
- CallToCalli Protection.
- Obfuscation doesn't saves assembly information such as assembly attributes [#36](https://github.com/sunnamed434/BitMono/issues/36).
- No more module reloading (Module now loads once and writes once).

### 0.4.3-alpha.12

#### Changed

- Protections are looks more cleaner than before, no code duplication.
- Now BitDotNet is not an protection but packer.

#### Fixed

- Obfuscation process.
- Manipulations and saving of Module optimized, most of things store in memory then writes in file instead of every time writing in file and loading module.

*Other versions were removed due to invalid usage of Semantic Versioning.*