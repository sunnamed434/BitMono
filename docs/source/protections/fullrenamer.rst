FullRenamer
===========

How it works?
-------------

Protection renames types/methods/fields, however, ignores things such as reflection, Unity Methods (Update/FixedUpdate/LateUpdate, i.e all of them), overrides from Thanking (OV_methodName), and the most popular frameworks for plugin development in Unturned and Rust on GitHub - RocketMod, OpenMod, and rust-oxide-umod, you even could specify your methods/types to ignore.

If you want you can easily configure `criticals.json` to ignore strings and lot of stuff.

Be careful, because renamer is tricky protection, not always useful, and does not always work properly. But, if you configure BitMono correctly Renamer can be a great protection (I'm about big projects, not crackmes).

WPF / XAML (BAML)
-----------------

WPF compiles XAML into BAML (inside ``<assembly>.g.resources``) that refers to your types and
members by name. BitMono reads that BAML so renaming doesn't break the app at XAML load.

With ``WpfBamlRewrite`` enabled (the default) BitMono **renames the XAML-referenced type names and
rewrites the BAML to match**, so your WPF types get obfuscated too. To stay safe it deliberately:

- renames only type *names*; the **members are kept**, because binding paths and event-handler
  wiring reference members as plain strings in BAML and renaming them would break bindings silently;
- skips a type whose name appears anywhere as a BAML string value (an ``{x:Type}``/``TargetType``
  reference, which isn't rewritten);
- keeps namespaces, and uses dot-free names so BAML type resolution still works.

Set ``"WpfBamlRewrite": false`` in ``obfuscation.json`` to instead leave XAML-referenced types fully
untouched (the older keep-only behaviour). Either way, everything not referenced by XAML is renamed
as usual.

One gap: a type that XAML binds to only through a code-behind ``DataContext`` (its name appears only
as a binding-path string) is not detected - exclude it via ``criticals.json`` or
``[Obfuscation(Exclude = true)]`` if needed.

Protection Type
---------------

The protection type is `Protection`.

IL2CPP
------

Works on IL2CPP builds, and it's one of the most useful ones there: it runs before ``il2cpp.exe``, so the renamed names are written cloaked into ``global-metadata.dat`` - which is exactly what tools like Il2CppDumper read (see :doc:`../protection-list/unity`).