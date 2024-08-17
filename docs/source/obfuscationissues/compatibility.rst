The module is built for ...
===========================

`The module is built for .NET (Core), but you're using a version of BitMono intended for .NET Framework.` - this message is very important because if your app is built on .NET Core but BitMono on .NET Framework after obfuscation your app just won't work.

Examples
--------

Good:

- BitMono on .NET Core and your app on .NET Core
- BitMono on .NET Framework and your app on .NET Framework

Bad (IT WON'T WORK!!!):

- BitMono on .NET Core and your app on .NET Framework
- BitMono on .NET Framework and your app on .NET Core