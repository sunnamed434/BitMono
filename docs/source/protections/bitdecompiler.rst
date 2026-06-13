BitDecompiler
=============

How it works?
-------------

This protection works the same as BitDotnet protection, but with some fixes. However, since after Unity 2021 and higher it stopped working correctly and since many of users asked to figure something out we made this protection as a solution =)

Protection Type
---------------

The protection type is `Packer`.


.. warning::

    This protection compatible only with Mono.

IL2CPP
------

Not supported on IL2CPP builds. It corrupts the managed PE's CLR metadata so only the Mono loader can read it, but ``il2cpp.exe`` must parse the assembly. BitMono skips it automatically when building for IL2CPP (see :doc:`../protection-list/unity`).