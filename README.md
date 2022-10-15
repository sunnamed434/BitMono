<p align="center">
  <img src="https://raw.githubusercontent.com/sunnamed434/BitMono/main/BitMonoLogo.png" alt="BitMono" width="180" /><br>
  Free open-source protector for Mono, empty decompilers? bits? crashes?!<br>
  All this and even more is right here
</p>

## Documentation 
Click **[here](https://github.com/sunnamed434/BitMono/wiki)** to open wiki about protections functionnality and even more.

## Obfuscation Features
* Breaks decompilers (crash when analyzing types, no code, seems to C++ application)
* Strings encryption
* **[BitDotNet](https://github.com/0x59R11/BitDotNet)** (most part of bit took from there)
* **[BitMethodDotnet](https://github.com/sunnamed434/BitMethodDotnet)** 
* Invisible types
* Call to calli
* FieldsHiding
* ObjectReturnType
* NoNamespaces
* FullRenamer
* AntiDebugBreakpoints

## Quick Start
`BitMono.CLI <path to file>/drag-and-drop/first file in Base directory or use BitMono.GUI (GUI Windows only)`

## Configuring Protections
Open `protections.json`, by default all protections are configured as it should, if something works not as it intentional you always may disable something or enable or even remove.

_Executing of protections depends how they are located in `protections.json` (protections order is up-to-down, sometimes order may ignored with special protection executing order eg Calling Condition as well as `BitDotNet` and `FieldsHiding` they are executing always after all protections)._

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

## No required dependency (Deprecated file for obfuscation)
Failed to resolve dependency Assembly-CSharp-firstpass, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
<br>Open `config.json` and set `FailOnNoRequiredDependency` to false, be careful with this parameter, change it in cases when file truly deprecated
```json
{
  "FailOnNoRequiredDependency": false,
}
```

## Except from Protecting
Ignoring classes/properties
```cs
using System;
using System.Xml.Serialization;
using Newtonsoft.Json;

[Serializable] // Marking as serializable attribute is enough to ignore everything in this model
class ProductModel
{
    [XmlAttribute("Product Name")] // Marking as Xml attribute
    string Name { get; set; }
    [JsonProperty("Product Description")] // Or marking as Json Property
    string Description { get; set; }
    [XmlAttribute("Product Price")]
    double Price { get; set; }
}
```

Ignoring methods
```cs
using System.Runtime.CompilerServices;

class MyClass
{
    [MethodImpl(MethodImplOptions.NoInlining)] // Add this attribute to ignore renaming of method
    void MyMethod()
    {
        // potential critical code used to be here
    }
}
```

## Excluding of Having issues with third-parties (API/Libraries)
Open `criticals.json`

Add to `CriticalMethods`, `CriticalInterfaces` or `CriticalBaseTypes` your potential critical things if you have it. 
<br>There is already supporting all `Unity` methods and third-party frameworks (`RocketMod`, `rust-oxide-umod`, `OpenMod`)

```json
{
  "CriticalMethods": [
    // Unity
    "Awake",
    "OnEnable",
    "Start",
    "FixedUpdate",
    // .. and more tons there

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
**[0x59R11](https://github.com/0x59R11)** for his investigation in big part of **[BitDotNet](https://github.com/0x59R11/BitDotNet)** that breaks files for mono executables!