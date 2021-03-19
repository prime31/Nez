---
id: installation
title: Installation
---

### Install as a submodule:

- create a [`Monogame Cross Platform Desktop Project`](https://docs.monogame.net/articles/getting_started/2_creating_a_new_project_netcore.html)
- clone or download the Nez repository: `git clone https://github.com/prime31/Nez.git`
- add the `Nez.Portable/Nez.csproj` project to your solution and add a reference to it in your main project
- make your main Game class (`Game1.cs` in a default project) subclass `Nez.Core`

#### Linking default content
If you intend to use any of the built in Effects or PostProcessors you should also copy or link the `DefaultContent/effects` folder into your projects `Content/nez/effects` folder and the `DefaultContent/textures` folder into `Content/nez/textures`. Be sure to set the Build Action to Content and enable the "Copy to output directory" property so they get copied into your compiled game. See the Nez.Samples csproj for an example on how to do this.

In dotnet core, you can link content files by adding the following into your project.csproj.
If you are using FNA you should replace `MG3.8Effects` with `FNAEffects`
```xml
<ItemGroup>
    <Content Include="..\Nez\DefaultContent\MG3.8Effects\**">
        <Link>Content\nez\effects\%(RecursiveDir)%(Filename)%(Extension)</Link>
        <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
    <Content Include="..\Nez\DefaultContent\textures\**">
        <Link>Content\nez\textures\%(RecursiveDir)%(Filename)%(Extension)</Link>
        <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
    <Content Include="..\Nez\DefaultContent\NezDefaultBMFont.xnb">
        <Link>Content\nez\NezDefaultBMFont.xnb</Link>
        <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
</ItemGroup>
```

Note: if you get compile errors referencing a missing `project.assets.json` file run `msbuild Nez.sln /t:restore` in the root Nez folder to restore them.

If you are using a .NET Core main application and want to switch the Nez projects over to .NET Standard 2.0, run the following command in a terminal. On Windows it will require the linux subsystem terminal: `find . -path Tests -prune -o -name 'Nez*.csproj' | grep -v Tests | grep -v Pipeline | xargs perl -pi -e 's/net471/netstandard2.0/g'`  You will also need to update the NuGet references in the Nez csproj files to point at the MonoGame dotnet Core versions. 

### Install through NuGet:

Note that using the NuGet packages isn't recommended. The source code has been carefully commented and contains a wealth of useful information. Use the NuGet packages to give Nez a test and if you like it consider switching to using the source code.

Add [Nez](https://www.nuget.org/packages/Nez/) to your project's NuGet packages. Optionally add the Nez.FarseerPhysics and Nez.Persistence NuGet packages.

Installing through NuGet, the contents of the `DefaultContent` content folder is also included in the package. You will find them under `packages/Nez.{VERSION}/tools`.

---

All Nez shaders are compiled for OpenGL so be sure to use the DesktopGL template, not DirectX! Nez only supports OpenGL out of the box to keep things compatible across Android/iOS/Mac/Linux/Windows.

If you are developing a mobile application you will need to enable touch input by calling `Input.Touch.EnableTouchSupport()`.
