FNA Compatibility
==========
To make getting up and running with FNA easier there are separate csproj files (*.FNA.csproj). Note that you still have to install the required FNA native libs per the [FNA documentation](https://github.com/FNA-XNA/FNA/wiki/1:-Download-and-Update-FNA). The [MonoGameCompat class](../Nez.Portable/Utils/MonoGameCompat.cs) is included as well and consists of a few extension methods that implmement some commonly used method that are in MonoGame but not in FNA.

See the "Using Nez with FNA" section of the [README.md](../README.md#using-nez-with-fna) for setup details.
