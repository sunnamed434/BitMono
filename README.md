<p align="center">
  <img src="https://raw.githubusercontent.com/sunnamed434/BitMono/main/resources/logo/BitMonoLogo.png" alt="BitMono logo" width="180" /><br>
  Free open-source obfuscator that targeting Mono and whole .NET<br>
</p>

## BitMono

[![MIT License][image_license]][license]
[![Documentation Status](https://readthedocs.org/projects/bitmono/badge/?version=latest)](https://bitmono.readthedocs.io/en/latest/?badge=latest)
[![Nuget feed][bitmono_nuget_shield]][bitmono_nuget_packages]
[![BitMono Discord][image_bitmono_discord]][bitmono_discord]

BitMono is a free, open-source C# obfuscator that was initially designed and intended mainly for [Mono][mono_mainpage], however, now you're feel free to use it for any .NET app, but, be careful some protections work on .NET Framework, some on .NET, some on [Mono][mono_mainpage], some on [Unity Engine][unityengine_mainpage] only.

BitMono uses [AsmResolver][asmresolver] instead of [dnlib][dnlib] (which we used in the past) for handling assemblies. If you have questions or issues, please let us know [here][bitmono_issues]. Download the latest version of BitMono [here][bitmono_releases].

You can also use BitMono as an engine to build custom obfuscators. It is built using dependency injection (DI) using [Autofac][autofac_repo] and follows the latest C# best practices.

<p align="center">
<img src="https://raw.githubusercontent.com/sunnamed434/BitMono/main/resources/images/preview/before-after.png"
  alt="Before and after obfuscation preview by BitMono">
</p>

<p align="center">
<img src="https://raw.githubusercontent.com/sunnamed434/BitMono/main/resources/images/preview/before-after-2.png"
  alt="Before and after obfuscation preview by BitMono 2">
</p>

<p align="center">
<img src="https://raw.githubusercontent.com/sunnamed434/BitMono/main/resources/images/preview/CLI.png"
  alt="CLI">
</p>

<p align="center">
<img src="https://raw.githubusercontent.com/sunnamed434/BitMono/main/resources/images/preview/configuration.png"
  alt="Configuration">
</p>

## Usability

BitMono breaks the most popular tools using just one packer, such as:
- dnSpy;
- dnlib;
- AsmResolver;
- MonoCecil;
- ILSpy;
- PEBear;
- DetectItEasy;
- CFF Explorer
- Perhaps, some dumpers?
- and many, many more...

So, if you will add more protection to the file, I think it would seem like total magic. :D

## Documentation

Read the **[docs][bitmono_docs]** to read protection, functionality, and more.

## How your app will look since BitMono obfuscation - just in a few words
* Looks like C++ application but is an actual C# application;
* Crash of decompilers when analyzing types;
* Broken decompilers;
* Broken IL Code;
* Invisible types;
* No code

## Features

* StringsEncryption
* **[UnmanagedString][unmanagedstring_source]** (based on existing protection)
* **[BitDotNet][bitdotnet_source]** (based and improved on existing protection)
* **[BitMethodDotnet][bitmethoddotnet_source]** (based and improved on existing protection)
* **[DotNetHook][dotnethook_source]** (based on existing protection)
* CallToCalli
* ObjectReturnType
* NoNamespaces
* FullRenamer
* AntiDebugBreakpoints
* AntiDecompiler
* BitDecompiler (fixed version of BitDotNet for newer Unity Versions)
* BitDateTimeStamp
* BitMono
* BillionNops
* AntiDe4dot
* AntiILdasm
* and you can integrate existing/make own feature ;)

## Documentation

Read the **[docs][bitmono_docs]** for comprehensive usage instructions, installation guides, and detailed documentation.

**Quick Start:**
1. Download from [releases][bitmono_releases]
2. Run `BitMono.CLI` and follow the prompts
3. (optional) See the [How To Use guide][bitmono_docs] for detailed instructions

**Installation Options:**
- **GitHub Releases**: Download pre-built executables
- **.NET Global Tool**: `dotnet tool install --global BitMono.GlobalTool`
- **NuGet Packages**: Integrate into your projects

For detailed installation and usage instructions, see the **[documentation][bitmono_docs]**.

### Troubleshooting

Having issues? See the [troubleshooting guide](https://bitmono.readthedocs.io/en/latest/usage/troubleshooting.html) in the documentation.

### Building

If you want to build BitMono yourself - see the [building guide](https://bitmono.readthedocs.io/en/latest/developers/building.html) in the documentation.

### Supported Frameworks

Feel free to use BitMono on frameworks which described below. Be careful using some protections because some might work on .NET Framework only, some on .NET (Core) only, some on all frameworks, some on Mono only - if the protection is unique to its platform/framework you will get a notification about that.

| Framework      | Version |
|----------------|---------|
| .NET           | 9.0     |
| .NET           | 8.0     |
| .NET           | 7.0     |
| .NET           | 6.0     |
| .NET Framework | 462     |
| netstandard    | 2.0     |
| netstandard    | 2.1     |

Credits
-------

**[JetBrains][jetbrains_rider]** has kindly provided licenses for their JetBrains Rider IDE to the contributors of BitMono. This top-tier tool greatly facilitates and enhances the process of software development.

**[0x59R11][author_0x59r11]** for his acquaintance in big part of **[BitDotNet][bitdotnet_source]** that breaks files for mono executables!

**[Gazzi][author_gazzi]** for his help that [me][author_sunnamed434] asked a lot!

**[Elliesaur][author_ellisaur]** for her acquaintance in **[DotNetHook][dotnethook_source]** that hooks methods.

**[Weka][author_naweka]** for his advices, help and motivation.

**[MrakDev][author_mrakdev]** for the acquaintance in **[UnmanagedString][unmanagedstring_source]**.

**[ConfuserEx and their Forks][confuserex_source]** for most things that I watched for the architecture of BitMono and the obfuscator engine as an application and solving plenty of User solutions which I would be knew in the very long future after much fail usage of BitMono and reports by other Users. Day-by-day I'm looking for something interesting there to improve myself in knowledge and BitMono also.

**[OpenMod][openmod_source]** Definitely, openmod inspired this project a lot with services and clean code, extensive similar things to openmod.

**[Kao and his blogs][author_kao_blog]** thanks a lot of these blogs.

**[drakonia][author_drakonia]** for her **[costura decompressor][simple_costura_decompressor_source]**.

[license]: https://github.com/sunnamed434/BitMono/blob/main/LICENSE
[previews]: https://github.com/sunnamed434/BitMono/blob/main/PREVIEWS.md
[asmresolver]: https://github.com/Washi1337/AsmResolver
[dnlib]: https://github.com/0xd4d/dnlib
[bitmono_issues]: https://github.com/sunnamed434/BitMono/issues
[bitmono_releases]: https://github.com/sunnamed434/BitMono/releases
[bitmono_docs]: https://bitmono.readthedocs.io/en/latest/
[bitdotnet_source]: https://github.com/0x59R11/BitDotNet
[bitmethoddotnet_source]: https://github.com/sunnamed434/BitMethodDotnet
[dotnethook_source]: https://github.com/Elliesaur/DotNetHook
[openmod_source]: https://github.com/openmod/openmod
[confuserex_source]: https://github.com/yck1509/ConfuserEx
[simple_costura_decompressor_source]: https://github.com/dr4k0nia/Simple-Costura-Decompressor
[unmanagedstring_source]: https://github.com/MrakDev/UnmanagedString
[jetbrains_rider]: https://www.jetbrains.com/rider/
[author_0x59r11]: https://github.com/0x59R11
[author_gazzi]: https://github.com/GazziFX
[author_ellisaur]: https://github.com/Elliesaur
[author_naweka]: https://github.com/naweka
[author_mrakdev]: https://github.com/MrakDev
[author_kao_blog]: https://lifeinhex.com/
[author_drakonia]: https://github.com/dr4k0nia
[author_sunnamed434]: https://github.com/sunnamed434
[bitmono_latest_release]: https://github.com/sunnamed434/BitMono/releases/latest
[bitmono_discord]: https://discord.gg/sFDHd47St4
[bitmono_nuget_packages]: https://www.nuget.org/profiles/BitMono
[bitmono_nuget_shield]: https://img.shields.io/nuget/v/BitMono.Core.svg
[autofac_repo]: https://github.com/autofac/Autofac
[unityengine_mainpage]: https://unity.com
[mono_mainpage]: https://www.mono-project.com

[troubleshooting]: https://github.com/sunnamed434/BitMono/blob/main/troubleshooting.md
[build_info]: https://github.com/sunnamed434/BitMono/blob/main/build.md
[image_codefactor]: https://www.codefactor.io/repository/github/sunnamed434/bitmono/badge/main
[image_deepsource]: https://deepsource.io/gh/sunnamed434/BitMono.svg/?label=active+issues&show_trend=true&token=_FJf25YbtCpPyX7SRveXCaGd
[image_license]: https://img.shields.io/github/license/sunnamed434/bitmono
[image_bitmono_discord]: https://img.shields.io/discord/1086240163321106523?label=discord&logo=discord
