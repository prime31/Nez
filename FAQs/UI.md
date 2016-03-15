Nez UI
============
Nez UI is based on TableLayout ([click for documentation](https://github.com/EsotericSoftware/tablelayout/blob/master/README.md)) and the libGDX Scene2D UI system (docs are [here](https://github.com/libgdx/libgdx/wiki/Scene2d.ui)). You can find detailed docs on the libGDX table [here](https://github.com/libgdx/libgdx/wiki/Table). As of this writing the API is nearly identical. The main differences to be aware of when using the libGDX docs for reference are:

- the Actor class is Element in Nez
- the Widget and WidgetGroup classes dont exist in Nez. Similar functionality is found in the Element and Group classes but this is really only relevant if you are making your own custom controls.
