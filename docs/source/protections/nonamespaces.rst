NoNamespaces
============

How it works?
-------------

Protection removes all namespaces.

Protection Type
---------------

The protection type is `Protection`.

IL2CPP
------

Works on IL2CPP builds: it runs before ``il2cpp.exe``, so the cleared namespaces are reflected in ``global-metadata.dat`` (see :doc:`../protection-list/unity`).