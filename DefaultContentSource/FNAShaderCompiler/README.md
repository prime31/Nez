## Nez FNA Shader Compiler
This script builds the Nez shaders with fxc.exe from the DirectX SDK. The ShaderCompilerScript requires that the `shaderCompilerPath` parameter point to a valid Wine bottle that is setup with fxc.exe in the root (on Mac or Linux). The app can be made with any Wine bottle/cask maker such as Wineskin Winery. If you are on Windows the script can point directly to fxc.exe without the use of Wine.

Once Wine is setup the script can be opened and run. You will be prompted for the input and output folders. The shaders will be compiled to fbx's. Note that the shaders from effects/transitions (or any subclasses in the effects class) need to be manually copied to the proper directory after they are compiled.
