Using Bottles Day to Day
=========================

Creating Bottles
----------------

To create a bottle you need to put a '.package-manifest' file in the root of your
project. So lets pretend that we have the following file system set up::

    ~\
    MyProject.sln
    MyApp\
        MyApp.csproj
        <bunch of shiz>
    MyModule\
        MyModule.csproj
        <bunch of shiz>

The easy way to create this file is to execute the bottle init command which 
looks like this.::

    bottles init <path> <name>

So in our app it would be::

    bottles init .\MyApp MyApp -r application
    bottles init .\MyModule MyModule -r module 

.. note::

    bottles is usually a .cmd / .sh file that points to the BottleRunner.exe
    in the ``~\packages\bottles.<version>\tools\`` directory.

You should now have an ``.bottle-alias`` file at the root of the project that I
recommend checking into source control. You will have two new ``.package-manifest``
files that should be checked in as well.

The ``.bottle-alias`` file looks like this::

    <?xml version="1.0"?>
    <aliases xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
      <Aliases>
        <alias name="MyApp" folder=".\MyApp" />
        <alias name="MtModule" folder=".\MyModule" />
      </Aliases>
    </aliases>

.. note::

    The alias feature is helpful if you have a nested source folder that you
    don't want to type all the time or you want to shorten a long project name.

The package manifest files look like this::

    <?xml version="1.0"?>
    <package xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
      <Role>application</Role>
      <Name>MyApp</Name>
      <assembly>MyApp</assembly>
      <!-- other dependencies -->
      <DataFileSet Include="*.*">
        <DeepSearch>true</DeepSearch>
      </DataFileSet>
      <ContentFileSet Include="*.spark;*.as*x;*.htm;*.master;Content\*.*;*.config" Exclude="fubu-content\*.*;Content\help\.svn;Content\*.vsd">
        <DeepSearch>true</DeepSearch>
      </ContentFileSet>
    </package>

*More about the file sets later.*

Linking Bottles
---------------

The short version, is that it allows you to 'link' in other modules that might
be optional for your application. AWESOMENESS.

*content*::

    bottles link <app> <module>
    bottles link MyApp MyModule

This help in dev mode. Now the MyApp will have a ``.links`` file that will point
back to MyModule. And now the MyModule will be available from the
``BottleRegistry.Bottles`` property.

Managing DLLs
-------------

*content*

version dependencies: bottles doesn't help you, its up to the user
to not get them selves in a bind currently.