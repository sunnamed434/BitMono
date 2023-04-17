Which base Protection better to select and why?
===============================================

Since BitMono contains inside the heart 3 different types of protection, it's very necessary to understand what you're working on and what type of protection you need to select.

So, 3 different type of protections:

1. Protection
2. PipelineProtection
3. Packer

First of all, you need to understand what kinda type of work your protection is going to do
- You need something for fast testing and access to the Module then simply use ``Protection``
- If you need access to the Module and you want to modify it you can use ``Protection``
- You want to split your protection into different layers with access to the Module, e.g, populating of child protection then you can use ``PipelineProtection``
- You don't need access to the Module, but, if you want to change the actual file structure i.e modify ``PE``, then ``Packer`` is your choice, actually you can have an access to the Module, but you need to rewrite it again because at this point file is already written.