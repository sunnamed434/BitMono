AntiILdasm
==========

How it works?
-------------

Protection adds ``[SuppressIldasmAttribute]`` which prevents the Ildasm (IL Disassembler) from disassembling the protected file.

Protection Type
---------------

The protection type is `Protection`.

IL2CPP
------

Not supported on IL2CPP builds. It injects ``SuppressIldasmAttribute``, which only affects ildasm on the managed PE that IL2CPP discards. BitMono skips it automatically when building for IL2CPP (see :doc:`../protection-list/unity`).