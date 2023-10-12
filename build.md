## Build the BitMono
First of all, if you just need the compiled binaries simply install them [right here][releases] - open the dropdown button `Assets`, and select the CLI/GUI.zip/7z file

### Build the solution
Recommended to install tools via Visual Studio installer, otherwise you can install those tools directly via provided links down below or just searching for it by yourself.

- [.NET Framework 462][net462]
- [Visual Studio 2022][vs2022]/[JetBrains Rider 2023][rider2023] or newer to build the solution
- [.NET 6.0 (Core)][net6]

### Artifacts

If you want to create a artifacts the same as BitMono do on release or you don't want to zip the output directory and set share it, then set `CreateBitMonoArtifacts` in SharedProjectProps.props to `true`, it will create an `.zip` archive file with build with currently selected one of the support/changed target frameworks.

Click build button, then go to `cloned repo path..\BitMono\src\BitMono.CLI` directory and you then you will see there `zip` archive artifacts. You can easily share them, just in case.

Archives examples (versions or naming can be a bit different):
- .NET 6.0: `BitMono-v0.18.0-alpha.33-CLI-net6.0.zip`
- .NET 462: `BitMono-v0.18.0-alpha.33-CLI-net462.zip`
- netstandard 2.1: `BitMono-v0.18.0-alpha.33-CLI-netstandard2.1.zip`
- netstandard 2.0: `BitMono-v0.18.0-alpha.33-CLI-netstandard2.0.zip`

### Help
If you have any issues/questions freely ask them [here][issues], or contact via:
- Email: sunnamed434@proton.me

[net462]: https://dotnet.microsoft.com/en-us/download/dotnet-framework/net462
[vs2022]: https://visualstudio.microsoft.com/downloads/
[rider2023]: https://www.jetbrains.com/rider/download/#section=windows
[net6]: https://dotnet.microsoft.com/en-us/download/dotnet/6.0
[releases]: https://github.com/sunnamed434/BitMono/releases
[issues]: https://github.com/sunnamed434/BitMono/issues