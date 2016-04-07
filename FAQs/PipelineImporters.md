Pipeline Importers
==========



Tiled Tilemaps
==========



BM Font
==========
BMFont processing can be done in two different ways. In the importer settings there is a bool (`packTexturesIntoXnb`) to toggle how the processor handles the files. If true (the default), the texture will be packed right in with the BMFont data in a single xnb file. If false, the texture will not be packed in the xnb. Setting it to false lets you use a shared texture atlas that includes the font atlas. It requires an extra bit of setup. In the .fnt file locate the <pages> element. Each page needs to have the **file** element correctly set to point to your atlas image. Additionally, two new elements need to be added manually: **x** and **y** indicating the top-left point in the atlas that the font texture is located.



Overlap2D
==========



Texture Atlas Generator
==========

