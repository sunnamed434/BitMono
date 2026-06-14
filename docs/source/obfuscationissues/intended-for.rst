Intended for ... runtime
========================

Messages like ``Intended for Mono runtime`` or ``Intended for .NET Core runtime`` are normal, they're a
heads-up, not an error. BitMono is just telling you that a protection you enabled is built for one
specific runtime.

For example, ``BitDecompiler`` prints ``Intended for Mono runtime`` because it only works on Mono, it
leans on Mono-specific quirks to hide things from decompilers. Run that same app on regular .NET and the
protection simply won't do its job.

So if you know your target runtime matches, you can ignore the message. If it doesn't, drop that
protection or pick one that fits. This is the runtime moniker system, see
:doc:`../developers/protection-runtime-moniker` for how it works.
