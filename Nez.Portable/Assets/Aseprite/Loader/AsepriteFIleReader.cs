using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;

namespace Nez.Aseprite
{
	public static class AsepriteFileReader
	{
		private const ushort ASE_HEADER_MAGIC = 0xA5E0;                 //  File Header Magic Number
		private const int ASE_HEADER_SIZE = 128;                        //  File Header Length, In Bytes
		private const uint ASE_HEADER_FLAG_LAYER_OPACITY_VALID = 1;     //  Header Flag (Is Layer Opacity Valid)

		private const ushort ASE_FRAME_MAGIC = 0xF1FA;                  //  Frame Magic Number

		private const ushort ASE_CHUNK_OLD_PALETTE1 = 0x0004;           //  Old Palette Chunk
		private const ushort ASE_CHUNK_OLD_PALETTE2 = 0x0011;           //  Old Palette Chunk
		private const ushort ASE_CHUNK_LAYER = 0x2004;                  //  Layer Chunk
		private const ushort ASE_CHUNK_CEL = 0x2005;                    //  Cel Chunk
		private const ushort ASE_CHUNK_CEL_EXTRA = 0x2006;              //  Cel Extra Chunk
		private const ushort ASE_CHUNK_COLOR_PROFILE = 0x2007;          //  Color Profile Chunk
		private const ushort ASE_CHUNK_EXTERNAL_FILES = 0x2008;         //  External Files Chunk
		private const ushort ASE_CHUNK_MASK = 0x2016;                   //  Mask Chunk (deprecated)
		private const ushort ASE_CHUNK_PATH = 0x2017;                   //  Path Chunk (never used)
		private const ushort ASE_CHUNK_TAGS = 0x2018;                   //  Tags Chunk
		private const ushort ASE_CHUNK_PALETTE = 0x2019;                //  Palette Chunk
		private const ushort ASE_CHUNK_USER_DATA = 0x2020;              //  User Data Chunk
		private const ushort ASE_CHUNK_SLICE = 0x2022;                  //  Slice Chunk
		private const ushort ASE_CHUNK_TILESET = 0x2023;                //  Tileset Chunk

		private const ushort ASE_LAYER_TYPE_NORMAL = 0;                 //  Layer Type Normal (Image) Layer
		private const ushort ASE_LAYER_TYPE_GROUP = 1;                  //  Layer Type Group
		private const ushort ASE_LAYER_TYPE_TILEMAP = 2;                //  Layer Type Tilemap

		private const ushort ASE_LAYER_FLAG_VISIBLE = 1;                //  Layer Flag (Is Visible)
		private const ushort ASE_LAYER_FLAG_EDITABLE = 2;               //  Layer Flag (Is Editable)
		private const ushort ASE_LAYER_FLAG_LOCKED = 4;                 //  Layer Flag  (Movement Locked)
		private const ushort ASE_LAYER_FLAG_BACKGROUND = 8;             //  Layer Flag (Is Background Layer)
		private const ushort ASE_LAYER_FLAG_PREFERS_LINKED = 16;        //  Layer Flag (Prefers Linked Cels)
		private const ushort ASE_LAYER_FLAG_COLLAPSED = 32;             //  Layer Flag (Displayed Collapsed)
		private const ushort ASE_LAYER_FLAG_REFERENCE = 64;             //  Layer Flag (Is Reference Layer)

		private const ushort ASE_CEL_TYPE_RAW_IMAGE = 0;                //  Cel Type (Raw Image)
		private const ushort ASE_CEL_TYPE_LINKED = 1;                   //  Cel Type (Linked)
		private const ushort ASE_CEL_TYPE_COMPRESSED_IMAGE = 2;         //  Cel Type (Compressed Image)
		private const ushort ASE_CEL_TYPE_COMPRESSED_TILEMAP = 3;       //  Cel Type (Compressed Tilemap)

		private const uint ASE_CEL_EXTRA_FLAG_PRECISE_BOUNDS_SET = 1;   //  Cel Extra Flag (Precise Bounds Set)

		private const ushort ASE_PALETTE_FLAG_HAS_NAME = 1;             //  Palette Flag (Color Has Name)

		private const uint ASE_USER_DATA_FLAG_HAS_TEXT = 1;             //  User Data Flag (Has Text)
		private const uint ASE_USER_DATA_FLAG_HAS_COLOR = 2;            //  User Data Flag (Has Color)

		private const uint ASE_SLICE_FLAGS_IS_NINE_PATCH = 1;           //  Slice Flag (Is 9-Patch Slice)
		private const uint ASE_SLICE_FLAGS_HAS_PIVOT = 2;               //  Slice Flag (Has Pivot Information)

		private const uint ASE_TILESET_FLAG_EXTERNAL_FILE = 1;          //  Tileset Flag (Includes Link To External File)
		private const uint ASE_TILESET_FLAG_EMBEDDED = 2;               //  Tileset Flag (Includes Tiles Inside File)

		private const byte TILE_ID_SHIFT = 0;                           //  Tile ID Bitmask Shift
		private const uint TILE_ID_MASK = 0x1fffffff;                   //  Tile ID Bitmask
		private const uint TILE_FLIP_X_MASK = 0x20000000;               //  Tile Flip X Bitmask
		private const uint TILE_FLIP_Y_MASK = 0x40000000;               //  Tile Flip Y Bitmask
		private const uint TILE_90CW_ROTATION_MASK = 0x80000000;        //  Tile 90CW Rotation Bitmask

		public static AsepriteFile ReadFile(string path)
		{
			if (!File.Exists(path))
			{
				throw new FileNotFoundException($"Unable to locate a file at the path '{path}'");
			}

			using (Stream stream = File.OpenRead(path))
			{
				return ReadStream(stream);
			}
		}

		public static AsepriteFile ReadStream(Stream stream)
		{
			if (stream == null)
			{
				throw new ArgumentNullException(nameof(stream), $"{nameof(stream)} cannot be null");
			}

			if (!stream.CanRead)
			{
				throw new ArgumentException($"{nameof(stream)} must be a readable stream");
			}

			if (!stream.CanSeek)
			{
				throw new ArgumentException($"{nameof(stream)} must be a seekable");
			}

			stream.Position = 0;

			using (BinaryReader reader = new BinaryReader(stream))
			{
				return Read(reader);
			}
		}

		private static AsepriteFile Read(BinaryReader reader)
		{
			//	Reference to the last group layer that is read so that subsequent child layers can be added to it.
			AsepriteGroupLayer lastGroupLayer = null;

			//	Flag to determine if palette has been read. This is used to flag that user data chunk is for sprite due
			//	to changes in Aseprite version 1.3 file spec.
			bool paletteRead = false;

			//	Read the Aseprite file header
			_ = reader.ReadUInt32();                //	File size (ignored, don't need)
			ushort hMagic = reader.ReadUInt16();    //	Header magic number

			if (hMagic != ASE_HEADER_MAGIC)
			{
				reader.Dispose();
				throw new InvalidOperationException($"Invalid header magicnumber (0x{hMagic:X4}).  This does not appear to be a valid Aseprite file.");
			}

			ushort nFrames = reader.ReadUInt16();   //	Total number of frames
			ushort width = reader.ReadUInt16();     //	Width, in pixels, of each frame
			ushort height = reader.ReadUInt16();    //	Height, in pixels, of each frame

			if (width < 1 || height < 1)
			{
				reader.Dispose();
				throw new InvalidOperationException($"Invalid canvas size {width}x{height}.");
			}

			ushort depth = reader.ReadUInt16();     //	Color depth (bits per pixel)

			if (!Enum.IsDefined(typeof(AsepriteColorDepth), depth))
			{
				reader.Dispose();
				throw new InvalidOperationException($"Invalid color depth: {depth}");
			}

			uint hFlags = reader.ReadUInt32();      //	Header flags
			bool isLayerOpacityValid = (hFlags & ASE_HEADER_FLAG_LAYER_OPACITY_VALID) != 0;

			_ = reader.ReadUInt16();                    //	Speed (ms between frame) (deprecated)
			_ = reader.ReadUInt32();                    //	Set to zero (ignored)
			_ = reader.ReadUInt32();                    //	Set to zero (ignored)
			byte transparentIndex = reader.ReadByte();  //	Index of transparent color in palette

			AsepritePalette palette = new AsepritePalette(transparentIndex);
			AsepriteColorDepth colorDepth = (AsepriteColorDepth)depth;
			List<AsepriteFrame> frames = new List<AsepriteFrame>();
			List<AsepriteLayer> layers = new List<AsepriteLayer>();
			List<AsepriteTag> tags = new List<AsepriteTag>();
			List<AsepriteSlice> slices = new List<AsepriteSlice>();
			List<AsepriteTileset> tileSets = new List<AsepriteTileset>();
			List<string> warnings = new List<string>();

			if (!isLayerOpacityValid)
			{
				warnings.Add("Layer opacity valid flag is not set.  All layer opacity will default to 255");
			}

			if (transparentIndex > 0 && colorDepth != AsepriteColorDepth.Indexed)
			{
				//	Transparent color index is only valid on indexed color depth mode
				transparentIndex = 0;
				warnings.Add("Transparent index only valid for Indexed Color Depth mode. Defaulting to 0");
			}

			_ = reader.ReadBytes(3);                //	Ignore these bytes
			ushort nColors = reader.ReadUInt16();   //	Number of colors

			//	Remainder of header is not needed, skipping to the end of header
			reader.BaseStream.Position = ASE_HEADER_SIZE;

			//	Read frame-by-frame until all frames are read
			for (int frameNum = 0; frameNum < nFrames; frameNum++)
			{
				List<AsepriteCel> cels = new List<AsepriteCel>();

				//	Reference to the last chunk that can have user data so that we can apply a user data chunk to it
				//	when one is read
				IAsepriteUserData lastWithUserData = null;

				//	tracks the iteration of tags when reading user data for tag chunks
				int tagIterator = 0;

				//	Read the frame header
				_ = reader.ReadUInt32();                //	Bytes in frame (don't need, ignored)
				ushort fMagic = reader.ReadUInt16();    //	Frame magic number

				if (fMagic != ASE_FRAME_MAGIC)
				{
					reader.Dispose();
					throw new InvalidOperationException($"Invalid frame magic number (0x{fMagic:X4}) in frame {frameNum}");
				}

				int nChunks = reader.ReadUInt16();      //	Old field which specified chunk count.
				ushort duration = reader.ReadUInt16();  //	Frame duration, in milliseconds.
				_ = reader.ReadBytes(2);                //	For future (set to zero).
				uint moreChunks = reader.ReadUInt32();  //	New field which specifies chunk count.

				//	Determine which chunk count to use
				if (nChunks == 0xFFFF && nChunks < moreChunks)
				{
					nChunks = (int)moreChunks;
				}

				//	Read chunk-by-chunk until all chunks in this frame are read.
				for (int chunkNum = 0; chunkNum < nChunks; chunkNum++)
				{
					long chunkStart = reader.BaseStream.Position;
					uint chunkLength = reader.ReadUInt32();     //	Size of chunk, in bytes
					ushort chunkType = reader.ReadUInt16();     //	The type of chunk
					long chunkEnd = chunkStart + chunkLength;

					switch (chunkType)
					{
						case ASE_CHUNK_LAYER:
							ushort layerFlags = reader.ReadUInt16();
							ushort layerType = reader.ReadUInt16();
							ushort level = reader.ReadUInt16();
							_ = reader.ReadUInt16();
							_ = reader.ReadUInt16();
							ushort blend = reader.ReadUInt16();
							byte opacity = reader.ReadByte();
							_ = reader.ReadBytes(3);
							string name = reader.ReadString();

							if (!isLayerOpacityValid)
							{
								opacity = 255;
							}

							//	Validate Blend Mod
							if (!Enum.IsDefined(typeof(AsepriteBlendMode), blend))
							{
								reader.Dispose();
								throw new InvalidOperationException($"Unknown blend mode '{blend}' found in layer '{name}'");
							}

							AsepriteBlendMode blendMode = (AsepriteBlendMode)blend;
							bool isVisible = (layerFlags & ASE_LAYER_FLAG_VISIBLE) != 0;
							bool isBackground = (layerFlags & ASE_LAYER_FLAG_BACKGROUND) != 0;
							bool isReference = (layerFlags & ASE_LAYER_FLAG_REFERENCE) != 0;

							AsepriteLayer layer;

							switch (layerType)
							{
								case ASE_LAYER_TYPE_NORMAL:
									layer = new AsepriteImageLayer(isVisible, isBackground, isReference, level, blendMode, opacity, name);
									break;
								case ASE_LAYER_TYPE_GROUP:
									layer = new AsepriteGroupLayer(isVisible, isBackground, isReference, level, blendMode, opacity, name);
									break;
								case ASE_LAYER_TYPE_TILEMAP:
									uint tilesetIndex = reader.ReadUInt32();
									AsepriteTileset tileset = tileSets[(int)tilesetIndex];
									layer = new AsepriteTilemapLayer(tileset, isVisible, isBackground, isReference, level, blendMode, opacity, name);
									break;
								default:
									reader.Dispose();
									throw new InvalidOperationException($"Unknown layer type '{layerType}'");
							}

							if (level != 0 && lastGroupLayer != null)
							{
								lastGroupLayer.Children.Add(layer);
							}

							if (layer.GetType() == typeof(AsepriteGroupLayer))
							{
								lastGroupLayer = (AsepriteGroupLayer)layer;
							}

							lastWithUserData = layer;
							layers.Add(layer);
							break;


						case ASE_CHUNK_CEL:
							ushort celLayerIndex = reader.ReadUInt16();	//	Index of Layer the cel is on.
							short celXPosition = reader.ReadInt16();    //	Cel Y Position.
							short celYPosition = reader.ReadInt16();    //	Cel X Position.
							byte celOpacity = reader.ReadByte();        //	Cel opacity level
							ushort celType = reader.ReadUInt16();       //	Cel type.
							_ = reader.ReadBytes(7);                    //	For future (set to zero).

							Point celPosition = new Point(celXPosition, celYPosition);
							AsepriteLayer celLayer = layers[celLayerIndex];

							AsepriteCel cel = null;
							switch(celType)
							{
								case ASE_CEL_TYPE_RAW_IMAGE:
									ushort celWidth = reader.ReadUInt16();	//	Width of cel, in pixels
									ushort celHeight = reader.ReadUInt16();	//	Height of cel, in pixels
									byte[] pixelData = reader.
							}
							break;
					}
				}



			}



		}
	}
}