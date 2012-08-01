Leveraging Bottles in your Framework
=======================================

To use Bottles you will be using the  BottleRegistry.

.. sourcecode:: csharp
    :linenos:

    BottleRegistry.LoadPackages(facility => 
    {
        facility.Assembly(assembly);
        facility.Loader(someLoader);
        
        facility.Activator(someActivator);
        
        facility.Bootstrapper(someBootstrapper);
        
    });

    BottleRegistry.Bottles;
    BottleRegistry.BottleAssemblies;

Once you have correctly setup the LoadPackages call you are now ready to roll.
You now have access to the static properties 'Bottles' and 'BottleAssemblies'.
Using these two properties, give you access to everything that bottles has loaded.
That is really all the magic there is. It loads up the assemblies into the 
current appdomain. You can load them your self with Assembly, kind of a PITA.
A better way is to use the Loader call. This is a way for you to discover bottles.
For instance, we use it discover 'linked' bottles, assembly bottles, and normal
bottles. You can make your own (like to use NuGet or something awesome) too!

