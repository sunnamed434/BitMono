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
members by name. BitMono reads that BAML and automatically keeps the referenced types (and their
members - event handlers, bound properties, custom controls, the ``x:Class`` types) unrenamed, so
the app still loads its XAML after obfuscation. Everything not referenced by XAML is still renamed.

This is the safe baseline: BitMono does **not** rewrite BAML, so XAML-referenced types simply keep
their names. Renaming them *and* rewriting the BAML to match is not supported yet. One gap: a type
that XAML binds to only through a code-behind ``DataContext`` (so its name appears only as a binding
path string) is not detected - exclude it via ``criticals.json`` or ``[Obfuscation(Exclude = true)]``
if needed.

Protection Type
---------------

The protection type is `Protection`.