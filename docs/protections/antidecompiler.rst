AntiDecompiler
==============

How it works?
-------------
Protection looks for a nested type in <Module> and sets non-public accessibility attributes, according to ECMA CIL standard nested types should always have one of them applied, but Mono doesn't care about this standard.

That means if someone will try to analyze the protected nested type, dnSpy will crash, however in a newer version, this exploit was fixed.


.. warning::

    This protection is only compatible with Mono.