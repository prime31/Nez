---
layout: docs
permalink: documentation/setup/installation
title: Docs
---

Setup
==========

### Quick version:

- clone or download the Nez repository
- add the Nez-PCL/Nez.csproj project to your solution and add a reference to it in your main project
- make your main Game class (Game1.cs in a default project) subclass Nez.Core


### (optional) Pipeline Tool setup for access to the Nez Pipeline importers

- add the Nez.PipelineImporter/Nez.PipelineImporter.csproj project to your solution
- open the Nez.PipelineImporter references dialog and add a reference to the Nez project
- build the Nez.PipelineImporter project to generate the DLLs
- open the Pipeline Tool by double-clicking your Content.mgcb file and add references to PipelineImporter.dll, Ionic.ZLib.dll, Newtonsoft.Json.dll and Nez.dll.


All Nez shaders are compiled for OpenGL so be sure to use the DesktopGL template, not DirectX! Nez only supports OpenGL out of the box to keep things compatible across Android/iOS/Mac/Linux/Windows.

If you intend to use any of the built in Effects or PostProcessors you should also copy or link the DefaultContent/effects folder into your projects Content/nez/effects folder. Be sure to set the Build Action to Content and enable the "Copy to output directory" property so they get copied into your compiled game.

If you are developing a mobile application you will need to enable touch input by calling `Input.touch.enableTouchSupport()`.
