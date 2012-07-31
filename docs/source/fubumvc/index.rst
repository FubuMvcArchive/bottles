Leveraging Bottles in your FubuMVC app
=======================================

Briefly, the simplest way to use it is the following.

.. sourcecode:: csharp
    :linenos:

    public class BottleRegistry : Registry //structure map
    {
        public BottleRegistry()
        {
            Scan(scanner =>
            {
                BottleRegistry.BottleAssemblies.Each(scanner.Assembly);
                scanner.LookForRegistries();
            });
        }
    }

The above code scans every loaded assembly for registries, and then loads 
them up into StructureMap. This is great for having bottles override various
services. FubuMVC will itself do a lot of work for the routes and what not.

For more on using Bottles in FubuMVC see the docs for FubuMVC.