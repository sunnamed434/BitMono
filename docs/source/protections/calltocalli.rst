CallToCalli
===========

How it works?
-------------

Protection replaces call opcode to calli and calls method by its function pointer.

Protection Type
---------------

The protection type is `Protection`.


.. warning::

    This protection doesn't work the same way as ConfuserEx Bed's Mod, it works absolutely differently, as you may know in Confuser's mod their `CallToCalli` won't work on Mono, this version of protection works fine with both .NET and Mono.

IL2CPP
------

Not supported on IL2CPP builds. It emits ``calli`` against runtime-resolved function pointers, which IL2CPP's AOT compiler doesn't support. BitMono skips it automatically when building for IL2CPP (see :doc:`../protection-list/unity`).