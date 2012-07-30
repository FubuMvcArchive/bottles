Key Ideas
=====================

Bottles is the spawn of a random conversation between Jeremy Miller and I. 
It started out with me asking "Is wanting something like Jars, but for .net,
crazy?" and after some back and forth we both agreed that it wasn't completely
stupid. So the idea continued to stew in my head, and to my surprise the next
time we talked Miller had actually started working on it for FubuMVC. Now that
I am at Dovetail I am working to have a bottle-aware Windows Service host 
that runs on Topshelf. This host will live in the Bottles repository and 
should provide a convenient way to host these types of services.

The Players in the Bottles Ecosystem
--------------------------------------

The entry point for integrating Bottles into your framework
 (Topshelf, FubuMVC, etc) is to look at the BottleRegistry class.

The BottleRegistry.LoadBottles static method allows
your framework to go and find all of the necessary pieces
of the puzzle to put your application together, including
Loaders, Activators, Assemblies, Bootstrappers and Facilities.
So what are all of these things?

**IBottleLoaders**: An IBottleLoader discovers bottles and
loads them into the system. It does this by converting the
physical on-disk format of the bottle to the in-memory model
as IBottleInfo.

**IBottleInfo**: This is the in-memory representation the contents
of a bottle. There are two sides to this guy, the providing side
where you register everything you find and a consumer side where
you can do things for each item found in it. 

**IActivator**: This interface gives you a chance to execute any thing
that is needed to start your application up, so if your bottle
wants to prime a cache before you start up this is how you do it.
During this activation process you will have access to all of the
IBottleInfo's (so that you can copy all of those images to the
correct folder maybe) and you will have access to the built in
log to leverage the bottle diagnostics.

**Assembly**: This is just the standard issue .net assembly. 
Bottles recognizes this as a first class item in the framework, 
Bottles can load these into the AppDomain for you, so we can 
extend the AppDomain loader to support things like loading dlls 
from a zip file, or if they exist outside of the 'bin' directory 
they can be loaded in from the sidelines.

**IBootstrapper**: Typically this is where the user of your
framework will interact most, this is where Bottles expects 
the user to do things like IoC registration and return all 
of the activators that need to be called, you can think of 
it as a Application_Start for your app. If you pull activators 
out of your IoC container before returning them then they will 
also have access to the services inside of them.

**IBottleFacility**: This is the mother of all of them and 
will let you do all kinds of things.

The Bottle Lifecyle
-------------------


#. Tell bottles BottleRegistry here are the bottles and the common activators.
#. Execute all bottle loaders - find and load all of the bottles (look for zip files, explode them / open wrap / nuget)
#. Bottles integrate all bottles (IBottleInfo) and loads all assemblies.
#. Run all registered bootstrappers and Spin up the container
#. Run all activators

Use Case of Bottles
-------------------

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

Bottles and NuGet
------------------

Whenever I describe bottles it often seems that people think that I shoul
be using NuGet or OpenWrap instead, however 'Bottles != NuGet or OpenWrap'
I hope that from the above you can see that, now that being said there is
no reason why a Bottle couldn't be bottled up as a NuGet package or an
OpenWrap for simple delivery, 'Bottles and NuGet/OpenWrap are besties.'
