Use Case of Bottles
===================

*more here*


Scenario #1
+++++++++++++++

So we start with a simple bottle, Towncrier which is a fancy zip file,
that contains three assemblies (Towncrier.dll, Topshelf.dll, log4net.dll),
a config file (towncrier.config) and a manifest file (.topshelf-manifest).

The manifest file tells us which assemblies are needed and some other things.
This is done to help figure out any conflicts ahead of time.

Once the manifest checks out, the bottles bootstrapper start the process
of loading all of the bottles into memory.