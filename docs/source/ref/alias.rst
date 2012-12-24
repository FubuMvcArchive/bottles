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
        Creates a new alias for the specified folder.

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

Notes
=====

All of the defined alias' are stored in a file titled ``.bottle-alias``.  This
file is stored current solution folder.

The alias **name** is unique in the alias list. If you define an alias more than
once, the Folder value of the last definition will take precedence over all
others.


