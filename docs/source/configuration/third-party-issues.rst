Excluding third-party code (criticals.json)
===========================================

Some code can't be renamed without breaking, Unity message methods like ``Update()``, plugin base
classes, types a framework finds by name. You can't put an attribute on code you don't own, so instead you
list these things by name in ``criticals.json`` and BitMono leaves anything matching them alone.

.. note::

   The defaults already cover all the common cases: every Unity message method, and the base types and
   interfaces for **RocketMod**, **OpenMod**, and **uMod / Oxide**. For a plain Unity or plugin project you
   usually don't need to touch this file at all.

What you can match on
---------------------

.. list-table::
   :header-rows: 1
   :widths: 28 18 54

   * - List
     - Toggle
     - Excludes
   * - ``CriticalAttributes``
     - ``UseCriticalAttributes``
     - Members carrying an attribute you name (by ``Namespace`` + ``Name``), e.g. Unity's
       ``[SerializeField]``.
   * - ``CriticalModelAttributes``
     - ``UseCriticalModelAttributes``
     - Whole types (models) carrying a serialization attribute, ``[Serializable]``, ``[XmlAttribute]``,
       ``[JsonProperty]``, …
   * - ``CriticalInterfaces``
     - ``UseCriticalInterfaces``
     - Types that implement an interface, matched by its short name (e.g. ``IRocketPlugin``).
   * - ``CriticalBaseTypes``
     - ``UseCriticalBaseTypes``
     - Types that derive from a base type, matched against the full name. Supports ``*`` wildcards, so
       ``RocketPlugin*`` catches it regardless of generics or namespace tail.
   * - ``CriticalMethods``
     - ``UseCriticalMethods``
     - Methods with an exact name, e.g. Unity's ``Awake`` / ``Update`` / ``OnDestroy``.
   * - ``CriticalMethodsStartsWith``
     - ``UseCriticalMethodsStartsWith``
     - Methods whose name starts with a prefix, e.g. ``OV_``.

Each list has its own ``Use…`` toggle, set it ``false`` to switch that whole list off.

Example
-------

A trimmed ``criticals.json`` (the defaults ship with the full Unity method list):

.. code-block:: json

   {
     "UseCriticalAttributes": true,
     "CriticalAttributes": [
       { "Namespace": "UnityEngine", "Name": "SerializeField" }
     ],

     "UseCriticalModelAttributes": true,
     "CriticalModelAttributes": [
       { "Namespace": "System", "Name": "SerializableAttribute" },
       { "Namespace": "Newtonsoft.Json", "Name": "JsonPropertyAttribute" }
     ],

     "UseCriticalInterfaces": true,
     "CriticalInterfaces": [ "IRocketPlugin", "IOpenModPlugin" ],

     "UseCriticalBaseTypes": true,
     "CriticalBaseTypes": [ "RocketPlugin*", "OpenModUnturnedPlugin*", "RustPlugin*" ],

     "UseCriticalMethods": true,
     "CriticalMethods": [ "Awake", "Start", "Update", "OnDestroy" ],

     "UseCriticalMethodsStartsWith": true,
     "CriticalMethodsStartsWith": [ "OV_" ]
   }

.. warning::

   Don't dump every method in your own project here. This file is for the handful of *critical* names you
   genuinely can't rename (framework hooks and the like). For your own code, just rename it in your IDE, or
   use :doc:`[Obfuscation] <exclude-obfuscation>` when you want to keep a specific member readable.
