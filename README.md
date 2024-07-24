<p align="center">
  <img src="https://raw.githubusercontent.com/sunnamed434/BitMono/main/resources/logo/BitMonoLogo.png" alt="BitMono logo" width="180" /><br>
  Free open-source obfuscator that targetting Mono, and maybe whole .NET<br>
</p>

## BitMono

[![Codefactor][image_codefactor]][codefactor]
[![DeepSource][image_deepsource]][deepsource]
[![MIT License][image_license]][license]
[![BitMono Discord][image_bitmono_discord]][bitmono_discord]

BitMono is a free open-source C# obfuscator that in most cases works **only** with Mono - well known as a fork of .NET framework (which runs popular platforms such as Unity, etc), you can still use this for a whole .NET, but be careful that something working not as intentional, because the main target of the this project is Mono (actually some protections don't work with Mono but work with .NET Core). Which uses **[AsmResolver][asmresolver]** for assembly manipulation (not a dnlib as you might already think). If you have any questions/issues please let me know **[there][bitmono_issues]**. You can install the latest version of BitMono **[here][bitmono_releases]**.

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
* BitDateTimeStamp
* BitMono
* BillionNops
* and you can integrate existing/make own feature ;)

## Usage

### Pre-Require

Set one of setting from `protections.json` to `true`.

### Using CLI

`BitMono.CLI <path to file>/drag-and-drop`

Always drop dependencies in `libs` directory in the same path where `file` for obfuscation is located

Your obfuscation directory structure will look something like this:
```
specially_created_folder_for_obfuscation/
├─ your_app.exe
└─ libs/
  ├─ ImportantLibrary.dll
  ├─ SuperImportantLibrary.dll
  └─ ...
```

Copy all libraries (.dll) from the building application folder and paste them into the `libs` directory (if it doesn't exist yet create it), or even create the libs directory yourself with a custom name for example - `myLibs`, and then specify it in BitMono, however, if you will use `libs` then by default BitMono looking for a `libs` directory, so it will save your time.

### Using CLI Commands

```console
  -f, --file         Required. Set file path.

  -l, --libraries    Set libraries path.

  -o, --output       Set output path.

  --help             Display this help screen.

  --version          Display version information.
```

Basic example
```console
$ BitMono.CLI -f C:\specially_created_folder_for_obfuscation/your_app.exe -l specially_created_folder_for_obfuscation/libs
```

In case when you already have a directory with the name `libs` (specially_created_folder_for_obfuscation\libs) BitMono will catch it automatically, so, you don't need to specify it anymore, but you can in case if you made another directory with `libs` somewhere on the disk or even just for "visibility".
```console
$ BitMono.CLI -f C:\specially_created_folder_for_obfuscation/your_app.exe
```

Specify custom `libs` directory
```console
$ BitMono.CLI -f C:\specially_created_folder_for_obfuscation/your_app.exe -l C:\mythings\obfuscation\superLibsDirectory
```

Specify file, libs and output. If output directory doesn't exist BitMono will create it automatically and even open it on the top of the screen, if you want you can disable opening of the directory on the of top of the screen in `obfuscation.json` - and set `OpenFileDestinationInFileExplorer` to false.
```console
$ BitMono.CLI -f C:\specially_created_folder_for_obfuscation/your_app.exe -l C:\mythings\obfuscation\superLibsDirectory -o C:\specially_created_folder_for_obfuscation/output
```

Want more? Simply read the **[docs][bitmono_docs]**.

### Troubleshooting

Having issues? Get more help **[here][troubleshooting]**.

### Building

If you want to build the BitMono by your own - [click here for detailed info][build_info]

### Supported Frameworks

Feel free to use BitMono on frameworks which described below. Be careful using some protections because some might work on .NET Framework only, some on .NET (Core) only, some on all frameworks, some on Mono only - if the protection is unique to its platform/framework you will get a notification about that.

| Framework      | Version |
|----------------|---------|
| .NET (Core)    | 8.0     |
| .NET (Core)    | 7.0     |
| .NET (Core)    | 6.0     |
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

[codefactor]: https://www.codefactor.io/repository/github/sunnamed434/bitmono/overview/main
[deepsource]: https://deepsource.io/gh/sunnamed434/BitMono/?ref=repository-badge
[license]: https://github.com/sunnamed434/BitMono/blob/main/LICENSE
[previews]: https://github.com/sunnamed434/BitMono/blob/main/PREVIEWS.md
[asmresolver]: https://github.com/Washi1337/AsmResolver
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
[bitmono_discord]: https://discord.gg/sFDHd47St4

[troubleshooting]: https://github.com/sunnamed434/BitMono/blob/main/troubleshooting.md
[build_info]: https://github.com/sunnamed434/BitMono/blob/main/build.md
[image_codefactor]: https://www.codefactor.io/repository/github/sunnamed434/bitmono/badge/main
[image_deepsource]: https://deepsource.io/gh/sunnamed434/BitMono.svg/?label=active+issues&show_trend=true&token=_FJf25YbtCpPyX7SRveXCaGd
[image_license]: https://img.shields.io/github/license/sunnamed434/bitmono
[image_bitmono_discord]: https://img.shields.io/discord/1086240163321106523?label=discord&logo=discord
