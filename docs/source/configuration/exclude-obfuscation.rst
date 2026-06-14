Exclude a member from being obfuscated
======================================

Got a method, class, or whole assembly you don't want touched? Mark it in your source with the standard
``[Obfuscation]`` attribute (``System.Reflection.ObfuscationAttribute``) and BitMono leaves it alone.

.. code-block:: csharp

    using System.Reflection;

    [Obfuscation(Feature = "FullRenamer")] // keep this class out of FullRenamer
    class MyClass
    {
        void MyMethod()
        {
        }
    }

``Feature`` is the name of the protection to skip (the same name you use in ``protections.json``). Leave
``Feature`` off and the member is excluded from **every** protection:

.. code-block:: csharp

    [Obfuscation] // skip this class entirely, all protections
    class KeepMeReadable
    {
    }

A few things worth knowing:

- **``Exclude`` defaults to ``true``**, so just adding the attribute excludes the member. Set
  ``Exclude = false`` if you ever want to *opt a member back in*.
- **It covers members too.** Put it on a class and its methods/fields are excluded as well
  (``ApplyToMembers`` is ``true`` by default, set it ``false`` to limit the attribute to the class itself).
- **It's stripped from the output** after obfuscation by default, so the attribute doesn't leak into your
  shipped assembly (``StripAfterObfuscation``).
- Stack several attributes to skip several protections on the same member:

  .. code-block:: csharp

      [Obfuscation(Feature = "FullRenamer")]
      [Obfuscation(Feature = "CallToCalli")]
      void MyMethod() { }

.. note::

    This whole mechanism is controlled by ``ObfuscationAttributeObfuscationExclude`` in
    ``obfuscation.json``. It's on by default, set it ``false`` if you want BitMono to ignore
    ``[Obfuscation]`` attributes entirely.

Skipping NoInlining methods
---------------------------

BitMono can also skip any method marked with ``[MethodImpl(MethodImplOptions.NoInlining)]``, handy when you
already use that attribute and don't want to add ``[Obfuscation]`` on top of it. Turn it on with
``NoInliningMethodObfuscationExclude`` in ``obfuscation.json``:

.. code-block:: csharp

    [MethodImpl(MethodImplOptions.NoInlining)]
    void MyMethod()
    {
    }

Data models and serialization
-----------------------------

Types that get serialized (``[Serializable]``, ``[XmlAttribute]``, ``[JsonProperty]``, …) usually need
their names left intact, or deserialization breaks. Marking the model is enough:

.. code-block:: csharp

    [Serializable] // keeps the whole model intact
    class ProductModel
    {
        public string Name { get; set; }
        public double Price { get; set; }
    }

Which attributes count as "a model" is configurable, and that list already covers the common serializers.
See :doc:`third-party-issues` for the ``criticals.json`` side of this.

Can't edit the source?
----------------------

Attributes only work on code you own. For third-party types, Unity messages, plugin base types, and
anything else you can't annotate, exclude it by name instead, see :doc:`third-party-issues`.
