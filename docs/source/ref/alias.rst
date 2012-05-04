.. _alias:

=============
bottles alias
=============

The bottles *alias* command is used to manage folder aliases.

Usages
======

    ``bottles alias``
        Lists all of the aliases for the current solution folder.
        
    ``bottles alias <name> [-r, --remove]``
        Removes the specified alias.
        
    ``bottles alias <name> <folder>``
        Creates a new alias for a the specified folder.

Arguments
=========

    **name**
        The name of the alias
    
    **folder**
        The path to the actual folder
        
Flags
=====

    ``--remove, -r``
        Removes the Alias
