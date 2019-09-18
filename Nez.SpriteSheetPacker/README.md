Nez Sprite Sheet Packer
==========
Nez includes a helper program for generating sprite atlases along with runtime classes to load them. When packing an atlas, this tool will also output a `.atlas` file that Nez can parse at runtime. For use with the runtime loader make sure the output image and atlas have the same name.

Sprite Sheet Packer will also handle animations. Any subdirectories that contain images will be setup as animations when packing the sprites into an atlas. In the example folder structure below the tool will generate the animations "player", "enemy1" and "enemy2" with any images present in those folders.

- root-dir
	- player
	- enemy1
	- enemy2


## Usage

`mono SpriteSheetPacker.exe -image:out.png -map:out.atlas -r -fps:7 folder/with/images`


## Options

```
/image:string     Output file name for the image.
/map:string       Output file name for the map.
/mw:int           Maximum ouput width. Default:'4096'
/mh:int           Maximum ouput height. Default:'4096'
/pad:int          Padding between images. Default:'1'
/pow2             Ensures output dimensions are powers of two.
/sqr              Ensures output is square.
/r                Searches subdirectories of any input directories.
/originX:float    Origin X for the images Default:'0.5'
/originY:float    Origin Y for the images Default:'0.5'
/fps:int          Framerate for any animations Default:'8'
/lua              Output LOVE2D lua file
<input>           Images to pack. Default:''
```



### Attribution
Forked from this old bit of code from Codeplex: https://archive.codeplex.com/?p=spritesheetpacker