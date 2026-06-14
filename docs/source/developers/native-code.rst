Native Code
===========

If you want to use a native code in the protection you must do the following:


.. code-block:: csharp

    [ConfigureForNativeCode] // Add this attribute on top of the protection class
    public class CustomProtection : Protection


A good example is the ``UnmanagedString`` protection, it uses native code to encrypt strings. Have a look
at the ``UnmanagedString`` source if you want to see it in action.

Don't skip the attribute. BitMono used to switch to native code automatically, without any attribute, but
that changes the app's architecture and could break it. So now it's opt-in, and you have to ask for it
with ``[ConfigureForNativeCode]``.