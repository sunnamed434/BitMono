Which base Protection to pick, and why
======================================

BitMono has three kinds of protection at its core, and picking the right base for what you're building
matters. They are:

1. ``Protection``
2. ``PipelineProtection``
3. ``PackerProtection``

Start by asking what your protection actually needs to do:

- **Just need the Module?** Reading or modifying types, methods, IL, anything in the managed metadata,
  use ``Protection``. It's also the simplest one to start with for quick testing.
- **Want to split the work into layers?** If your protection naturally breaks into stages, or it spawns
  child protections, use ``PipelineProtection``. You still get full access to the Module.
- **Changing the file itself (the PE)?** Use ``PackerProtection``. Packers run *last*, after the module
  has already been written to disk, so this is where you rewrite the actual file structure. You can still
  reach the Module from here, but you'd have to write it out again, so only do this if you really need to
  touch the PE.
