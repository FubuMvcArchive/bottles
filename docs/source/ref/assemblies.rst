.. _assemblies:

==================
bottles assemblies
==================

The bottles *assemblies* command is used to add assemblies to a given manifest.

Usages
======

    ``bottles assemblies <mode> <directory> [-f, --file <filename>] [-o, --open], [-t, --target Debug|Release]``
        Removes or adds all assemblies in the sepcified <directory> to the 
        manifest file.
        
    ``bottles assemblies <mode> <directory> <assemblyname> [-f, --file <filename>] [-o, --open], [-t, --target Debug|Release]``
        Removes or adds a single assembly name to the manifest file.

Arguments
=========

    **mode**
        `Add`, `remove`, or `list` the assemblies for the manifest
        
    **directory**
        The package or application directory.
        
    **assemblyname**
        The name of the assembly to be added or removed.
                
Flags
=====

    ``--file <filename>, -f <filename>``
        Overrides the name of the manifest file if it's not the default
        ``.package-manifest`` or ``.fubu-manifest``
        
    ``--open, -o``
        Opens the manifest file in your editor
        
    ``--target Debug|Release, -t Debug|Release``
        Choose the compilation target for any assemblies. Default is **Debug**
