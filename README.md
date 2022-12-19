<p align="center">
  <img src="https://raw.githubusercontent.com/sunnamed434/BitMono/main/resources/logo/BitMonoLogo.png" alt="BitMono logo" width="180" /><br>
  Free open-source obfuscator for Mono<br>
</p>

## BitMono
[![Build status](https://ci.appveyor.com/api/projects/status/8jh35hfno6riq25j?svg=true)](https://ci.appveyor.com/project/sunnamed434/bitmono)
[![Join the chat at https://gitter.im/BitMonoSpeech/community](https://badges.gitter.im/BitMonoSpeech/community.svg)](https://gitter.im/BitMonoSpeech/community?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

Bit Mono is a free open-source C# obfuscator that in most cases works **only** with Mono - well known as fork of .NET Framework but with custom bugs or Unity. Which uses its own fork of **[dnlib](https://github.com/sunnamed434/dnlib)** for assembly manipulation. If you have any questions/issues please let me know **[there](https://github.com/sunnamed434/BitMono/issues)**. You can install the lastest version of BitMono **[here](https://github.com/sunnamed434/BitMono/releases)**.

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
Open the **[wiki](https://github.com/sunnamed434/BitMono/wiki)** to read protections functionnality and more.

## How your app will look since BitMono obfuscation - just in a few words
* Seems to C++ application but this is an actual C# application
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
* FieldsHiding (Deprecated)
* ObjectReturnType
* NoNamespaces
* FullRenamer
* AntiDebugBreakpoints
* BitDateTimeStamp

## Usage
`BitMono.CLI <path to file>/drag-and-drop or use BitMono.GUI (GUI Windows only)`

Always drop dependencies in `libs` directory in the same path where is obfuscation `file` located

Credits
-------
**[0x59R11](https://github.com/0x59R11)** for his acquaintance in big part of **[BitDotNet](https://github.com/0x59R11/BitDotNet)** that breaks files for mono executables!

**[Gazzi](https://github.com/GazziFX)** for his help that [me](https://github.com/sunnamed434) asked a lot!

**[Elliesaur](https://github.com/Elliesaur)** for his acquaintance in **[DotNetHook](https://github.com/Elliesaur/DotNetHook)** that hooks methods.

**[Weka](https://github.com/sasharumega)** for his advices, help and motivation. 

**[ConfuserEx and their Forks](https://github.com/yck1509/ConfuserEx)** for most things that I watched for the architecture of BitMono and the obfuscator engine as an application and solving plenty of User solutions which I would be knew in the very long future after much fail usage of BitMono and reports by other Users. Day-by-day I'm looking for something interesting there to improve myself in knowledge and BitMono also.

**[Kao and his blogs](https://lifeinhex.com/) thanks a lot of these blogs.
