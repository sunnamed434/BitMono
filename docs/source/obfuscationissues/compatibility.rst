The module is built for ...
===========================

If you see something like this:

.. code-block:: text

   The module is built for .NET (Core), but you're using a version of BitMono intended for .NET Framework.

it means BitMono and your app don't target the same framework. Grab the BitMono build that matches and
you're good:

- .NET (Core) app → .NET build of BitMono
- .NET Framework app → .NET Framework build
- Unity/Mono → .NET Framework build

Mix them up and the app will break after obfuscation, so this part isn't optional. Not sure which build
you're on? Check the framework of both your app and the BitMono release you downloaded, they have to
line up.
