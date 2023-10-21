CorLib not found
================

The problem
-----------
You're getting an error it says: `Could not load file or assembly CorLib Version=x.x.x.x, etc` 


Solution
--------
The thing that may cause this issue is that you're running on .NET Framework (the obfuscator project) and the actual obfuscator project is running on .NET Core (let's say - 6.0), and you're trying to run the obfuscated file on Mono, you might already catch the problem, you need to change the obfuscator project TargetFramework (i.e in this case to .NET Framework) to approximately the same version as target file for obfuscation.