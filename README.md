<p align="center">
  <img src="https://raw.githubusercontent.com/sunnamed434/BitMono/main/resources/logo/BitMonoLogo.png" alt="BitMono logo" width="180" /><br>
  Free open-source protector for Mono, empty decompilers? bits? crashes?!<br>
  All this and even more is right here
</p>

<p align="center">
<img src="https://raw.githubusercontent.com/sunnamed434/BitMono/main/resources/images/preview/before-after.png"
  alt="Before and after obfuscation preview by BitMono"
</p>

<p align="center">
<img src="https://raw.githubusercontent.com/sunnamed434/BitMono/main/resources/images/preview/GUI.png"
  alt="GUI"
</p>

<p align="center">
<img src="https://raw.githubusercontent.com/sunnamed434/BitMono/main/resources/images/preview/CLI.png"
  alt="CLI"
</p>

<p align="center">
<img src="https://raw.githubusercontent.com/sunnamed434/BitMono/main/resources/images/preview/configuration.png"
  alt="Configuration"
</p>

## Documentation 
Open **[wiki](https://github.com/sunnamed434/BitMono/wiki)** to read protections functionnality and more.

## How your app will look since BitMono obfuscation - just in a few-words
* Seems to C++ application but this is actual C# application
* Crash of decompilers when analyzing types
* Broken decompilers
* Broken IL Code
* Invisible types
* No code 

## Obfuscation Features
* StringsEncryption
* **[BitDotNet](https://github.com/0x59R11/BitDotNet)** (based and improved on existing protection)
* **[BitMethodDotnet](https://github.com/sunnamed434/BitMethodDotnet)** (based and improved on existing protection)
* **[DotNetHook](https://github.com/Elliesaur/DotNetHook)** (based on existing protection)
* Call to calli
* FieldsHiding
* ObjectReturnType
* NoNamespaces
* FullRenamer
* AntiDebugBreakpoints

## Quick Start
`BitMono.CLI <path to file>/drag-and-drop/first file in Base directory or use BitMono.GUI (GUI Windows only)`

## Configuring Protections
Use `protections.json` - by default all protections are configured as it should, if something works not as it intentional you always may disable something or enable or even remove.

_Executing of protections depends how they are located in `protections.json` (protections order is up-to-down, sometimes order may ignored by special protection executing order eg `IStageProtection` as well as `BitDotNet`, `FieldsHiding`, `CallToCalli` and `DotNetHook` - they are executing always at their own order)._

Lets look at this example, first will be executed `AntiILdasm` then `AntiDe4dot` and `ControlFlow` and `BitDotNet` and `FieldsHiding`.
Always you could write in `protections.json` - protections which are doesnt mentioned here or if you create protection by yourself.
```json
{
  "Protections": [
    {
      "Name": "BitDotNet", // Executing always after all protections because of Calling Condition
      "Enabled": true,
    }
    {
      "Name": "AntiILdasm",
      "Enabled": true
    },
    {
      "Name": "AntiDe4dot",
      "Enabled": true
    },
    {
      "Name": "ControlFlow",
      "Enabled": true
    },
    {
      "Name": "FieldsHiding", // Executing always after all protections because of Calling Condition
      "Enabled": true
    }
  ]
}

```

## Excluding Protection(s)
More info about excluding - ctor, cctor, interfaces etc to read is **[here](https://github.com/sunnamed434/BitMono/wiki/Excluding-Protection(s))**
```cs
[assembly: Obfuscation(Feature = "Name")] // Ignoring whole assembly or in the AssemblyInfo.cs (sometimes it would not exist in your project)
namespace Project
{
}

[Obfuscation(Feature = "CallToCalli")]
public class ProductModel
{
    [Obfuscation(Feature = "Renamer")]
    public string Name { get; set; }

    [MethodImpl(MethodImplOptions.NoInlining)] // Add this attribute to ignore renaming of method
    void Method()
    {
    }
}
```

## No required dependency (Deprecated file for obfuscation)
Failed to resolve dependency Assembly-CSharp-firstpass, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
<br>Use `appsettings.json` and set `FailOnNoRequiredDependency` to false, be careful with this parameter, change it in cases when file truly deprecated
```json
{
  // Adding visible things that shows you have been used BitMono to protect your app
  "Watermark": true,

  // Excluding from obfuscation if method has [MethodImpl(MethodImplOptions.NoInlining)] attribute
  "NoInliningMethodObfuscationExcluding": true,

  // Excluding from obfuscation if it is a type/method and etc 
  // should has an [Obfuscation(Feature = "Name")] attribute with Protection name (Feature) and Excluding set to true
  "ObfuscationAttributeObfuscationExcluding": true,

  // Sometimes when you don`t have needed dependency for your app, a tons of reasons could be for that, 
  // if you got error that says you don`t have needed dependency first of all atleast try to add this dependency
  // otherwise if this is deprecated - you can set this to false to ignore error and continue obfuscation
  // NB! but be aware of kind a weird errors and issues that breaking you app and it stop working after that
  "FailOnNoRequiredDependency": false,

  "Logging": {
    "LogsFile": "logs.txt"
  },
}
```

## Excluding of Having issues with third-parties (API/Libraries)
Use `criticals.json`

Add to `CriticalMethods`, `CriticalInterfaces` or `CriticalBaseTypes` your potential critical things if you have it. 
<br>There is already supported all `Unity` methods and third-party frameworks as `RocketMod`, `rust-oxide-umod`, `OpenMod`.

```json
{
  "CriticalMethods": [
    // Unity
    "Awake",
    "OnEnable",
    "Start",
    "FixedUpdate",
    // .. etc

  ],
  "CriticalInterfaces": [
    // RocketMod
    "IRocketPlugin",
    "IRocketCommand",
    "IRocketPluginConfiguration",
    "IDefaultable",

    // OpenMod
    "IOpenModPlugin"
  ],
  "CriticalBaseTypes": [
    // RocketMod
    "RocketPlugin",

    // OpenMod
    "OpenModUnturnedPlugin",
    "OpenModUniversalPlugin",
    "Command",

    // rust-oxide-umod
    "RustPlugin"
  ]
}
```

Credits
-------
**[0x59R11](https://github.com/0x59R11)** for his acquaintance in big part of **[BitDotNet](https://github.com/0x59R11/BitDotNet)** that breaks files for mono executables!

**[Gazzi](https://github.com/GazziFX)** for his help that [me](https://github.com/sunnamed434) asked a lot!

**[Elliesaur](https://github.com/Elliesaur)** for his acquaintance in **[DotNetHook](https://github.com/Elliesaur/DotNetHook)** that hides methods execution.