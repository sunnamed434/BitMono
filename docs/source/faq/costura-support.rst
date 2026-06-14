Does BitMono provide Costura-Fody Support by default?
=====================================================

Yes, out of the box. BitMono resolves Costura-Fody's embedded resources automatically, so you don't need
to keep separate copies of your ``.dll`` files or set ``DisableCleanup`` to ``true`` in
``FodyWeavers.xml``.