CorLib not found
================

The problem
-----------

You're getting an error like ``Could not load file or assembly CorLib Version=x.x.x.x, ...``.

Solution
--------

This usually means a framework mismatch. You're running a BitMono build that targets one runtime (say the
.NET / .NET Core build), but the file you're obfuscating is meant for another (say Mono). When the
runtimes don't line up, BitMono can't resolve the core library.

Fix it by using the BitMono build whose target framework matches the file you're protecting. For a Mono
target, use the .NET Framework build of BitMono. See :doc:`compatibility` for the full framework-matching
rundown.
