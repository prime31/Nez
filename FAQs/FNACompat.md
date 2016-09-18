FNA Compatibility
==========
To make getting up and running with FNA easier there is a separate branch along with a simple demo project (FNATester) that has the MonoGame Content Builder wired up to work with FNA. Note that you still have to install the required FNA native libs per the [FNA documentation](https://github.com/FNA-XNA/FNA/wiki/1:-Download-and-Update-FNA). The [MonoGameCompat class](https://github.com/prime31/Nez/blob/62bbcca5e346413cacc2c3f9e765e11ead568de5/Nez-PCL/Utils/MonoGameCompat.cs) is included as well and consists of a few extension methods that implmement some commonly used method that are in MonoGame but not in FNA.

Here is what you need to do to get up and running with Nez + FNA:

- clone this repo recursively
- switch to the Nez.FNA branch (you may need to update the FNA submodules via `git submodule update`)
- open the Nez solution and build it
- open your project add a reference to the Nez.FNA project


### (optional) Pipeline Tool setup for access to the Nez content importers
If you want to use the Nez content importers follow the steps outlined [here](https://github.com/prime31/Nez/blob/master/README.md#optional-pipeline-tool-setup-for-access-to-the-nez-pipeline-importers)



### (optional) MonoGame Content Builder (MGCB) setup for your FNA project

stub. content in progress...In the meantime a quick and simple way to get a project setup that uses FNA with MonoGame's content builder is to do the following:

- install MonoGame (download [here](http://www.monogame.net/downloads/))
- create a new project that uses the MonoGame DesktopGL template
- add FNA as a submodule
- open the project references and remove the MonoGame references then add the FNA project as a reference
- delete the MonoGame DLLs/dylibs (libSDL, NVorbis, OpenTK, etc)
