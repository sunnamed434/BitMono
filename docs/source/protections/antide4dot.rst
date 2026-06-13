AntiDe4dot
==========

How it works?
-------------

Protection adds multiple attributes of known obfuscators/protectors and as a result fools de4dot.

Protection Type
---------------

The protection type is `Protection`.


.. warning::

    Due to protection is not really powerful and widely known by most reversers and skids, on GitHub you may see a lot of solutions to destroy this protection.

IL2CPP
------

Not supported on IL2CPP builds. It injects decoy obfuscator-marker types whose names aren't valid identifiers, and only confuses managed-PE tools (de4dot) that never see the IL2CPP build. BitMono skips it automatically when building for IL2CPP (see :doc:`../protection-list/unity`).