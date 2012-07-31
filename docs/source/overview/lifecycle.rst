The Bottle Lifecyle
===================


#. Tell bottles BottleRegistry here are the bottles and the common activators.
#. Execute all bottle loaders - find and load all of the bottles (look for zip files, explode them / open wrap / nuget)
#. Bottles integrate all bottles (IBottleInfo) and loads all assemblies.
#. Run all registered bootstrappers and Spin up the container
#. Run all activators