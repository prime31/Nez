FNA Compatibility
==========
To make getting up and running with FNA easier there is a separate branch along with a simple demo project (FNATester) that has the MonoGame Content Builder wired up to work with FNA. Note that you still have to install the required FNA native libs per the [FNA documentation](https://github.com/FNA-XNA/FNA/wiki/1:-Download-and-Update-FNA).

Here is what you need to do to get up and running with Nez + FNA:

- clone this repo recursively
- switch to the Nez.FNA branch (you may need to update the FNA submodules via `git submodule update`)
- open the Nez solution and build it
- open your project add a reference to the Nez.FNA project


### (optional) Pipeline Tool setup for access to the Nez content importers
If you want to use the Nez content importers follow the steps outlined [here](https://github.com/prime31/Nez/blob/master/README.md#optional-pipeline-tool-setup-for-access-to-the-nez-pipeline-importers)



### (optional) MonoGame Content Builder (MGCB) setup for your FNA project

stub. content in progress...
