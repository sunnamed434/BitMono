AntiDecompiler
==============

How it works?
-------------

Protection looks for a nested type in <Module> and sets non-public accessibility attributes, according to ECMA CIL standard nested types should always have one of them applied, but Mono doesn't care about this standard.

That means if someone will try to analyze the protected nested type, dnSpy will crash, however in a newer version, this exploit was fixed.

Protection Type
---------------

The protection type is `Packer`.


.. warning::

    This protection compatible only with Mono.

IL2CPP
------

Not supported on IL2CPP builds. It applies Mono-specific ExplicitLayout metadata tricks that ``il2cpp.exe`` rejects. BitMono skips it automatically when building for IL2CPP (see :doc:`../protection-list/unity`).