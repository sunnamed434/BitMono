The module is built for ...
===========================

Understanding Compatibility
---------------------------

When using BitMono for obfuscation, it's critical to ensure that the version of BitMono matches the framework your application is built on. 

For example, if your application is built on .NET Core, you **must** use the version of BitMono that is also built for .NET Core. Using an incompatible version will result in your application not functioning after obfuscation.

.. note:: 
   A common error message you may encounter is:  
   ``The module is built for .NET (Core), but you're using a version of BitMono intended for .NET Framework.``
   This indicates a mismatch between your app's framework and BitMono's version.

Examples of Compatibility
--------------------------

Here are some examples of correct and incorrect configurations:

**✅ Good Configurations:**

- **BitMono for .NET Core** with an application built on **.NET Core**
- **BitMono for .NET Framework** with an application built on **.NET Framework**

**❌ Bad Configurations (These Won't Work!):**

- **BitMono for .NET Core** with an application built on **.NET Framework**
- **BitMono for .NET Framework** with an application built on **.NET Core**

Key Takeaways
-------------

- Always ensure that **BitMono's framework version** matches the **framework version** of your application.
- Incompatible configurations will break your app after obfuscation.
- Carefully check the framework version of both your app and the BitMono release you are using.

.. warning:: 
   Mixing framework versions (e.g., using BitMono for .NET Framework with a .NET Core app) will cause the app to fail after obfuscation.