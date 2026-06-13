BillionNops
===========

History
-------

You might already know this protection because it's so popular and well known in many obfuscators, it is very simple protection but can cause a lot of headaches.

How it works?
-------------

Protection adds a new dummy method in the Module and then adds 100.000 nop instructions, and ret at the end.

As a result when someone will try to analyze this method will cause a crashed dnSpy, and it will lead a reverse engineer install old dnSpy or use a IDE/VS Code/LinqPad with installed AsmResolver or dnlib to remove this method.

Cons
----

Be careful because this protection will increase a file size a lot, and a bigger file size will cause more questions by users, most of us when see a big file size think that this file is obfuscated.

Protection Type
---------------

The protection type is `Protection`.

IL2CPP
------

Not supported on IL2CPP builds. It only pads the managed PE (discarded by IL2CPP) with a huge junk method while bloating the generated C++. BitMono skips it automatically when building for IL2CPP (see :doc:`../protection-list/unity`).