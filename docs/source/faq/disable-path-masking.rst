How to disable path masking?
============================

.. note::

   Path masking was a feature in older versions of BitMono that used Serilog for logging.
   Since BitMono now uses a lightweight custom logger, path masking is no longer applied by default.

In current versions of BitMono, file paths are displayed in full without any masking. If you're seeing masked paths like ``(***\things)``, you may be using an older version of BitMono.

To get full path visibility, simply update to the latest version of BitMono from the `releases page <https://github.com/sunnamed434/BitMono/releases/latest>`_.
