<p align="center">
  <img src="https://raw.githubusercontent.com/sunnamed434/BitMono/main/BitMonoLogo.png" alt="BitMono" width="180" /><br>
  Free open-source obfuscator for Mono, empty decompilers? bits? crashes?!<br>
  All this, right here
</p>

## Features
* Breaks decompilers (crash when analyzing types, no code, seems to C++ application)
* Strings encryption
* **[BitDotNet](https://github.com/0x59R11/BitDotNet)** (most of bit took from there)
* **[BitMethodDotnet](https://github.com/sunnamed434/BitMethodDotnet)** 
* Invisible types
* Call to calli

## Quick Start
`BitMono.CLI <path to PE file> or BitMono.GUI`

## Ignoring protections
To make sure your method is in ignore you shall to make as shown in example here
```cs
using System.Runtime.CompilerServices;

class MyClass
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    void MyMethod()
    {
        // potential critical code used to be here
    }
}
```

## Excluding of Having issues with third-parties (API/Libraries)
Open `config.json`
```json
{
  "FileWatermark": true,
  "Logging": {
    "LogsFile": "logs.txt"
  },
  "Protections": [
    {
      "Name": "StringsEncryption",
      "Enabled": false
    },
    {
      "Name": "FieldsHiding",
      "Enabled": true
    },
    {
      "Name": "CallToCalli",
      "Enabled": false
    },
    {
      "Name": "ObjectReturnType",
      "Enabled": false
    },
    {
      "Name": "MethodsBreak",
      "Enabled": false
    },
    {
      "Name": "BitDotNet",
      "Enabled": false
    },
  ],
  "CriticalMethods": [
    // Unity, here is only a few in config you will see all methods that supports Unity
    "Awake",
    "OnEnable",
    "Reset",
    "Start",
    "FixedUpdate",
  ],
  "CriticalInterfaces": [
    // RocketMod
    "IRocketPlugin",
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

    // rust-oxide-umod
    "RustPlugin"
  ],
  "Tips": [
    "[Tip]: Mark your method with attribute [MethodImpl(MethodImplOptions.NoInlining)] to ignore obfuscation of your method!",
    "[Tip]: Open config.json and set FileWatermark to 'true', to disable watermarking of your file!"
  ]
}
```

Credits
-------
**[0x59R11](https://github.com/0x59R11)** for his **[BitDotNet](https://github.com/0x59R11/BitDotNet)** that breaks files for mono executables!