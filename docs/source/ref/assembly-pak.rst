.. _assembly-pak:

=============
bottles assembly-pak
=============

The bottles *assembly-pak* command is used to bundle up the content and data
files for a self contained assembly package.

Usages
======

    `bottles assembly-pak <rootfolder> [-p --proj-file <projfile>]`
        Bundles the specified content.
        
Arguments
=========

    rootfolder
        The root folder for the project, if different from the project file's
        folder
        
Flags
=====

    --proj-file <projfile>, -p <projfile>
        The name of the `.csproj` file.  If set, this command attempts to add
        the zip files as embedded resources
