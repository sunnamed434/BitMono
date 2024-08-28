## Binaries

If you just need the compiled binaries simply install them [from releases][releases] - open the dropdown button `Assets`, and select the preferred archive, these binaries were made automatically via CI/CD pipeline. 

### Compiling

Recommended to install tools via Visual Studio installer, otherwise you can install those tools directly via provided links down below or just searching for it by yourself.

- [.NET Framework 462][net462]
- [Visual Studio 2022][vs2022]/[JetBrains Rider][rider] or newer to build the solution
- [.NET 8.0][net8]
- [.NET 7.0][net7]
- [.NET 6.0][net6]

To build the solution from command line, use: 

```bash
$ dotnet build 
```

Otherwise do that via IDE `Build` button if you have.

To run tests, use:

```bash
$ dotnet test
```

### Release On GitHub details

Archives examples (versions and/or naming can be a bit different):
- .NET 8.0: `BitMono-v0.24.2+7aaeceac-CLI-net8.0-linux-x64.zip`
- .NET 7.0: `BitMono-v0.24.2+7aaeceac-CLI-net7.0-win-x64.zip`
- .NET 6.0: `BitMono-v0.24.2+7aaeceac-CLI-net6.0-linux-x64.zip`
- .NET 462: `BitMono-v0.24.2+7aaeceac-CLI-net462-win-x64.zip`
- netstandard 2.1: `BitMono-v0.24.2+7aaeceac-CLI-netstandard2.1-linux-x64.zip`
- netstandard 2.0: `BitMono-v0.24.2+7aaeceac-CLI-netstandard2.0-win-x64.zip`

To be more clear:
- `v0.24.2` is the version and the value `+7aaeceac` after the version is the hash of the commit.
- `CLI` is the `command line interface` meaning, currently BitMono only have CLI for usage.
- `net8.0`, `net7.0`, `net6.0`, `net462`, `netstandard2.1`, `netstandard2.0` is the target framework that BitMono was built on.

### Help

If you have any issues/questions freely, ask them [here][issues], or contact via:
- Email: sunnamed434 (at) proton.me

[net462]: https://dotnet.microsoft.com/en-us/download/dotnet-framework/net462
[vs2022]: https://visualstudio.microsoft.com/downloads
[rider]: https://www.jetbrains.com/rider/download
[net6]: https://dotnet.microsoft.com/en-us/download/dotnet/6.0
[net7]: https://dotnet.microsoft.com/en-us/download/dotnet/7.0
[net8]: https://dotnet.microsoft.com/en-us/download/dotnet/8.0
[releases]: https://github.com/sunnamed434/BitMono/releases
[issues]: https://github.com/sunnamed434/BitMono/issues