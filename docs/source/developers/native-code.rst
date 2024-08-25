Native Code
###########

If you want to use a native code in the protection you must do the following:


.. code-block:: csharp

    [ConfigureForNativeCode] // Add this attribute on top of the protection class
    public class CustomProtection : Protection


A good example is ``UnmanagedString`` protection. It uses native code to encrypt strings. You can find the source code in the ``UnmanagedString`` file.

This thing is so important to do, before actually it was automatically done before the obfuscation without any attributes, however we found that this might break an app, because it changes the architecture of the app, so we decided to make it optional.