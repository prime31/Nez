using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Microsoft.Xna.Framework;

namespace Nez.Aseprite
{
	/// <summary>
	/// Utility class used to load the contents of an Aseprite (.ase/.aseprite) file.
	/// </summary>
	public static class AsepriteFileLoader
	{
		private const ushort ASE_HEADER_MAGIC = 0xA5E0;
		private const int ASE_HEADER_SIZE = 128;
		private const uint ASE_HEADER_FLAG_LAYER_OPACITY_VALID = 1;

		private const ushort ASE_FRAME_MAGIC = 0xF1FA;

		private const ushort ASE_CHUNK_OLD_PALETTE1 = 0x0004;
		private const ushort ASE_CHUNK_OLD_PALETTE2 = 0x0011;
		private const ushort ASE_CHUNK_LAYER = 0x2004;
		private const ushort ASE_CHUNK_CEL = 0x2005;
		private const ushort ASE_CHUNK_CEL_EXTRA = 0x2006;
		private const ushort ASE_CHUNK_COLOR_PROFILE = 0x2007;
		private const ushort ASE_CHUNK_EXTERNAL_FILES = 0x2008;
		private const ushort ASE_CHUNK_MASK = 0x2016;
		private const ushort ASE_CHUNK_PATH = 0x2017;
		private const ushort ASE_CHUNK_TAGS = 0x2018;
		private const ushort ASE_CHUNK_PALETTE = 0x2019;
		private const ushort ASE_CHUNK_USER_DATA = 0x2020;
		private const ushort ASE_CHUNK_SLICE = 0x2022;
		private const ushort ASE_CHUNK_TILESET = 0x2023;

		private const ushort ASE_LAYER_TYPE_NORMAL = 0;
		private const ushort ASE_LAYER_TYPE_GROUP = 1;
		private const ushort ASE_LAYER_TYPE_TILEMAP = 2;

		private const ushort ASE_LAYER_FLAG_VISIBLE = 1;
		private const ushort ASE_LAYER_FLAG_EDITABLE = 2;
		private const ushort ASE_LAYER_FLAG_LOCKED = 4;
		private const ushort ASE_LAYER_FLAG_BACKGROUND = 8;
		private const ushort ASE_LAYER_FLAG_PREFERS_LINKED = 16;
		private const ushort ASE_LAYER_FLAG_COLLAPSED = 32;
		private const ushort ASE_LAYER_FLAG_REFERENCE = 64;

		private const ushort ASE_CEL_TYPE_RAW_IMAGE = 0;
		private const ushort ASE_CEL_TYPE_LINKED = 1;
		private const ushort ASE_CEL_TYPE_COMPRESSED_IMAGE = 2;
		private const ushort ASE_CEL_TYPE_COMPRESSED_TILEMAP = 3;

		private const uint ASE_CEL_EXTRA_FLAG_PRECISE_BOUNDS_SET = 1;

		private const ushort ASE_PALETTE_FLAG_HAS_NAME = 1;

		private const uint ASE_USER_DATA_FLAG_HAS_TEXT = 1;
		private const uint ASE_USER_DATA_FLAG_HAS_COLOR = 2;

		private const uint ASE_SLICE_FLAGS_IS_NINE_PATCH = 1;
		private const uint ASE_SLICE_FLAGS_HAS_PIVOT = 2;

		private const uint ASE_TILESET_FLAG_EXTERNAL_FILE = 1;
		private const uint ASE_TILESET_FLAG_EMBEDDED = 2;

		private const byte TILE_ID_SHIFT = 0;
		private const uint TILE_ID_MASK = 0x1fffffff;
		private const uint TILE_FLIP_X_MASK = 0x20000000;
		private const uint TILE_FLIP_Y_MASK = 0x40000000;
		private const uint TILE_90CW_ROTATION_MASK = 0x80000000;

		/// <summary>
		/// Loads an Aseprite (.ase/.aseprite) file and creates an instance with the contents of the file.
		/// </summary>
		/// <param name="name">The name of the Aseprite (.ase/.aseprite) file to load</param>
		/// <param name="premultiplyAlpha">
		/// Indicates whether color data generated while reading the content of this Aseprite file should be
		/// premultipled.  Default is false.
		/// </param>
		/// <returns>
		/// A new instance of the <see cref="AsepriteFile"/> class initialized with the contents read from the Aseprite
		/// file.
		/// </returns>
		/// <exception cref="InvalidOperationException">
		/// Thrown if an error occurs while attempting to read the Aseprite file. See exception message for details.
		/// </exception>
		public static AsepriteFile Load(string name, bool premultiplyAlpha = false)
		{
			using (var stream = TitleContainer.OpenStream(name))
			{
				using (BinaryReader reader = new BinaryReader(stream))
				{
					return ReadAsepriteFile(reader, name, premultiplyAlpha);
				}
			}
		}

		/// <summary>
		/// Attempts to load an Aseprite (.ase/.aseprite) file and creates an instance with the contents of the file.
		/// </summary>
		/// <remarks>
		/// Errors occurred while attempting to load the Aseprite file will be logged in <see cref="Debug"/> output.
		/// </remarks>
		/// <param name="name">The name of the Aseprite (.ase/.aseprite) file to load</param>
		/// <param name="premultiplyAlpha">
		/// Indicates whether color data generated while reading the content of this Aseprite file should be
		/// premultipled.  Default is false.
		/// </param>
		/// <param name="file">
		/// When this method returns, if <see langword="true"/>, contains the contents of the Aseprite file that was
		/// loaded; otherwise, <see langword="null"/>.
		/// </param>
		/// <returns>
		/// <see langword="true"/> if the contents of the Aseprite file could be loaded without any issue; otherwise,
		/// <see langword="false"/>.
		/// </returns>
		public static bool TryLoad(string name, bool premultiplyAlpha, out AsepriteFile file)
		{
			file = null;

			using (var stream = TitleContainer.OpenStream(name))
			{
				try
				{
					using (BinaryReader reader = new BinaryReader(stream))
					{
						file = ReadAsepriteFile(reader, name, premultiplyAlpha);
					}
				}
				catch (Exception ex)
				{
					Debug.Error(ex.Message);
				}
			}

			return file != null;
		}

		private static AsepriteFile ReadAsepriteFile(this BinaryReader reader, string name, bool premultiplyAlpha)
		{
			reader.BaseStream.Seek(0, SeekOrigin.Begin);

			//  Reference to the last group layer that is read so that subsequent child layers can be added to it.
			AsepriteGroupLayer lastGroupLayer = null;

			//  Flag to determine if palette has been read. This is used to flag that user data chunk is for sprite due 
			//	to changes in Aseprite version 1.3
			bool paletteRead = false;

			//  Read the Aseprite file header
			reader.IgnoreDword();
			ushort hMagic = reader.ReadWord();

			if (hMagic != ASE_HEADER_MAGIC)
			{
				reader.Dispose();
				throw new InvalidOperationException($"Invalid header magic number (0x{hMagic:X4}). This does not appear to be a valid Aseprite file");
			}

			ushort nFrames = reader.ReadWord();
			ushort canvasWidth = reader.ReadWord();
			ushort canvasHeight = reader.ReadWord();

			if (canvasWidth < 1 || canvasHeight < 1)
			{
				reader.Dispose();
				throw new InvalidOperationException($"Invalid canvas size {canvasWidth}x{canvasHeight}.");
			}

			ushort depth = reader.ReadWord();
			AsepriteColorDepth colorDepth = (AsepriteColorDepth)depth;

			if (!Enum.IsDefined(typeof(AsepriteColorDepth), colorDepth))
			{
				reader.Dispose();
				throw new InvalidOperationException($"Invalid color depth: {depth}");
			}

			uint headerFlags = reader.ReadDword();
			bool isLayerOpacityValid = HasFlag(headerFlags, ASE_HEADER_FLAG_LAYER_OPACITY_VALID);

			reader.IgnoreWord();
			reader.IgnoreDword();
			reader.IgnoreDword();
			byte transparentIndex = reader.ReadByte();

			AsepritePalette palette = new AsepritePalette(transparentIndex);
			List<AsepriteFrame> frames = new List<AsepriteFrame>();
			List<AsepriteLayer> layers = new List<AsepriteLayer>();
			List<AsepriteTag> tags = new List<AsepriteTag>();
			List<AsepriteSlice> slices = new List<AsepriteSlice>();
			List<AsepriteTileset> tilesets = new List<AsepriteTileset>();
			List<string> warnings = new List<string>();
			AsepriteUserData spriteUserData = new AsepriteUserData();

			if (!isLayerOpacityValid)
			{
				warnings.Add("Layer opacity valid flag is not set. All layer opacity will default to 255");
			}

			if (transparentIndex > 0 && colorDepth != AsepriteColorDepth.Indexed)
			{
				//  Transparent color index is only valid in indexed depth
				transparentIndex = 0;
				warnings.Add("Transparent index only valid for Indexed Color Depth. Defaulting to 0");
			}


			reader.IgnoreBytes(3);
			ushort paletteColorCount = reader.ReadWord();

			//  Remainder of header is not needed, skipping to end of header
			reader.BaseStream.Seek(ASE_HEADER_SIZE, SeekOrigin.Begin);

			//  Read frame-by-frame until all frames are read.
			for (int frameNum = 0; frameNum < nFrames; frameNum++)
			{
				List<AsepriteCel> cels = new List<AsepriteCel>();

				//  Reference to the last chunk that can have user data so we can apply a User Data chunk to it when one
				//	is read.
				IAsepriteUserData lastWithUserData = null;

				//  Tracks the iteration of the tags when reading user data for tags chunk.
				int tagIterator = 0;

				//  Read the frame header
				reader.IgnoreDword();
				ushort frameMagic = reader.ReadWord();

				if (frameMagic != ASE_FRAME_MAGIC)
				{
					reader.Dispose();
					throw new InvalidOperationException($"Invalid frame magic number (0x{frameMagic:X4}) in frame {frameNum}.");
				}

				int nChunks = reader.ReadWord();
				ushort duration = reader.ReadWord();
				reader.IgnoreBytes(2);
				uint moreChunks = reader.ReadDword();

				//  Determine which chunk count to use
				if (nChunks == 0xFFFF && nChunks < moreChunks)
				{
					nChunks = (int)moreChunks;
				}

				//  Read chunk-by-chunk until all chunks in this frame are read.
				for (int chunkNum = 0; chunkNum < nChunks; chunkNum++)
				{
					long chunkStart = reader.BaseStream.Position;
					uint chunkLength = reader.ReadDword();
					ushort chunkType = reader.ReadWord();
					long chunkEnd = chunkStart + chunkLength;

					switch (chunkType)
					{
						case ASE_CHUNK_LAYER:
							ushort layerFlags = reader.ReadWord();
							ushort layerType = reader.ReadWord();
							ushort level = reader.ReadWord();
							reader.IgnoreWord();
							reader.IgnoreWord();
							ushort blend = reader.ReadWord();
							byte opacity = reader.ReadByte();
							reader.IgnoreBytes(3);
							string layerName = reader.ReadAsepriteString();

							if (!isLayerOpacityValid)
							{
								opacity = 255;
							}

							//  Validate blend mode
							AsepriteBlendMode blendMode = (AsepriteBlendMode)blend;
							if (!Enum.IsDefined(typeof(AsepriteBlendMode), blendMode))
							{
								reader.Dispose();
								throw new InvalidOperationException($"Unknown blend mode '{blend}' found in layer '{name}'");
							}

							bool isVisible = HasFlag(layerFlags, ASE_LAYER_FLAG_VISIBLE);
							bool isBackground = HasFlag(layerFlags, ASE_LAYER_FLAG_BACKGROUND);
							bool isReference = HasFlag(layerFlags, ASE_LAYER_FLAG_REFERENCE);

							AsepriteLayer layer = null;

							switch (layerType)
							{
								case ASE_LAYER_TYPE_NORMAL:
									layer = new AsepriteImageLayer(isVisible, isBackground, isReference, level, blendMode, opacity, layerName);
									break;
								case ASE_LAYER_TYPE_GROUP:
									layer = new AsepriteGroupLayer(isVisible, isBackground, isReference, level, blendMode, opacity, layerName);
									break;
								case ASE_LAYER_TYPE_TILEMAP:
									uint tilesetIndex = reader.ReadDword();    //  Tileset index
									AsepriteTileset tileset = tilesets[(int)tilesetIndex];
									layer = new AsepriteTilemapLayer(tileset, isVisible, isBackground, isReference, level, blendMode, opacity, layerName);
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
							ushort celLayerIndex = reader.ReadWord();
							short celXPosition = reader.ReadShort();
							short celYPosition = reader.ReadShort();
							byte celOpacity = reader.ReadByte();
							ushort celType = reader.ReadWord();
							reader.IgnoreBytes(7);

							AsepriteCel cel = null;
							Point celPosition = new Point(celXPosition, celYPosition);
							AsepriteLayer celLayer = layers[celLayerIndex];

							switch (celType)
							{
								case ASE_CEL_TYPE_RAW_IMAGE:
									{
										ushort celWidth = reader.ReadWord();
										ushort celHeight = reader.ReadWord();
										long pixelDataLen = chunkEnd - reader.BaseStream.Position;
										byte[] pixelData = reader.ReadBytes((int)pixelDataLen);

										Color[] pixels = PixelsToColor(pixelData, colorDepth, palette, premultiplyAlpha);
										cel = new AsepriteImageCel(celWidth, celHeight, pixels, celLayer, celPosition, celOpacity);
									}
									break;
								case ASE_CEL_TYPE_LINKED:
									{
										ushort linkedFrameIndex = reader.ReadWord();

										AsepriteCel otherCel = frames[linkedFrameIndex].Cels[cels.Count];
										cel = new AsepriteLinkedCel(otherCel, celLayer, celPosition, celOpacity);
									}
									break;
								case ASE_CEL_TYPE_COMPRESSED_IMAGE:
									{
										ushort celWidth = reader.ReadWord();
										ushort celHeight = reader.ReadWord();
										long compressDataLength = chunkEnd - reader.BaseStream.Position;
										byte[] compressedData = reader.ReadBytes((int)compressDataLength);
										byte[] pixelData = Deflate(compressedData);
										Color[] pixels = PixelsToColor(pixelData, colorDepth, palette, premultiplyAlpha);
										cel = new AsepriteImageCel(celWidth, celHeight, pixels, celLayer, celPosition, celOpacity);
									}
									break;
								case ASE_CEL_TYPE_COMPRESSED_TILEMAP:
									{
										ushort celWidthInTiles = reader.ReadWord();
										ushort celHeightInTiles = reader.ReadWord();
										ushort bitsPerTile = reader.ReadWord();
										uint tileIDBitmask = reader.ReadDword();
										uint xFlipBitmask = reader.ReadDword();
										uint yFlipBitmask = reader.ReadDword();
										uint rotationBitmask = reader.ReadDword();
										reader.IgnoreBytes(10);
										long compressedDataLength = chunkEnd - reader.BaseStream.Position;
										byte[] compressedData = reader.ReadBytes((int)compressedDataLength);
										byte[] tileData = Deflate(compressedData);

										//  Per Aseprite file spec, the "bits" per tile is, at the moment, always 
										// 	32-bits.  This means it's 4-bytes per tile (32 / 8 = 4).  Meaning that each 
										//	tile value is a uint (DWORD)
										int bytesPerTile = 4;
										AsepriteTile[] tiles = new AsepriteTile[tileData.Length / bytesPerTile];

										for (int i = 0, b = 0; i < tiles.Length; i++, b += bytesPerTile)
										{
											byte[] dword = new byte[sizeof(uint)];
											Array.Copy(tileData, b, dword, 0, sizeof(uint));
											uint value = BitConverter.ToUInt32(dword, 0);
											uint tileId = (value & TILE_ID_MASK) >> TILE_ID_SHIFT;
											uint xFlip = value & TILE_FLIP_X_MASK;
											uint yFlip = value & TILE_FLIP_Y_MASK;
											uint rotate = value & TILE_90CW_ROTATION_MASK;

											AsepriteTile tile = new AsepriteTile(tileId, xFlip, yFlip, rotate);
											tiles[i] = tile;
										}

										cel = new AsepriteTilemapCel(celWidthInTiles, celHeightInTiles, bitsPerTile, tileIDBitmask, xFlipBitmask, yFlipBitmask, rotationBitmask, tiles, celLayer, celPosition, celOpacity);
									}
									break;
								default:
									reader.Dispose();
									throw new InvalidOperationException($"Unknown cel type '{celType}'");
							}

							lastWithUserData = cel;
							cels.Add(cel);
							break;

						case ASE_CHUNK_TAGS:
							{
								ushort tagCount = reader.ReadWord();
								reader.IgnoreBytes(8);

								for (int i = 0; i < tagCount; i++)
								{
									ushort fromFrame = reader.ReadWord();
									ushort toFrame = reader.ReadWord();
									byte direction = reader.ReadByte();
									AsepriteLoopDirection loopDirection = (AsepriteLoopDirection)direction;

									//  Validate direction value
									if (!Enum.IsDefined(typeof(AsepriteLoopDirection), loopDirection))
									{
										reader.Dispose();
										throw new InvalidOperationException($"Unknown loop direction '{direction}'");
									}

									reader.IgnoreBytes(8);
									byte r = reader.ReadByte();
									byte g = reader.ReadByte();
									byte b = reader.ReadByte();
									reader.IgnoreByte();
									string tagName = reader.ReadAsepriteString();
									Color tagColor = premultiplyAlpha ? Color.FromNonPremultiplied(r, g, b, byte.MaxValue) : new Color(r, g, b, byte.MaxValue);
									AsepriteTag tag = new AsepriteTag(fromFrame, toFrame, loopDirection, tagColor, tagName);
									tags.Add(tag);
								}

								tagIterator = 0;

								if (tags.Count > 0)
								{
									lastWithUserData = tags[0];
								}
								else
								{
									lastWithUserData = null;
								}
							}
							break;

						case ASE_CHUNK_PALETTE:
							{
								uint newSize = reader.ReadDword();
								uint from = reader.ReadDword();
								uint to = reader.ReadDword();
								reader.IgnoreBytes(8);

								if (newSize > 0)
								{
									Array.Resize(ref palette.Colors, (int)newSize);
								}

								for (uint i = from; i <= to; i++)
								{
									ushort flags = reader.ReadWord();
									byte r = reader.ReadByte();
									byte g = reader.ReadByte();
									byte b = reader.ReadByte();
									byte a = reader.ReadByte();

									if (HasFlag(flags, ASE_PALETTE_FLAG_HAS_NAME))
									{
										reader.IgnoreString();    //  Color name (ignored)
									}
									palette.Colors[(int)i] = premultiplyAlpha ? Color.FromNonPremultiplied(r, g, b, a) : new Color(r, g, b, a);
								}

								paletteRead = true;
							}
							break;

						case ASE_CHUNK_USER_DATA:
							{
								uint userDataFlags = reader.ReadDword();

								string text = string.Empty;
								if (HasFlag(userDataFlags, ASE_USER_DATA_FLAG_HAS_TEXT))
								{
									text = reader.ReadAsepriteString();
								}

								Color? color = null;
								if (HasFlag(userDataFlags, ASE_USER_DATA_FLAG_HAS_COLOR))
								{
									byte r = reader.ReadByte();
									byte g = reader.ReadByte();
									byte b = reader.ReadByte();
									byte a = reader.ReadByte();
									color = premultiplyAlpha ? Color.FromNonPremultiplied(r, g, b, a) : new Color(r, g, b, a);
								}

								if (lastWithUserData == null && paletteRead)
								{
									spriteUserData.Text = text;
									spriteUserData.Color = color;
								}
								else if (lastWithUserData != null)
								{
									lastWithUserData.UserData.Text = text;
									lastWithUserData.UserData.Color = color;

									if (lastWithUserData.GetType() == typeof(AsepriteTag))
									{

										//  Tags are a special case, user data for tags comes all together 
										//	(one next to the other) after  the tags chunk, in the same order:
										//
										//  * TAGS CHUNK (TAG1, TAG2, ..., TAGn)
										//  * USER DATA CHUNK FOR TAG1
										//  * USER DATA CHUNK FOR TAG2
										//  * ...
										//  * USER DATA CHUNK FOR TAGn
										//
										//  So here we expect that the next user data chunk  will correspond to the next
										//	tag in the tags  collection
										tagIterator++;

										if (tagIterator < tags.Count)
										{
											lastWithUserData = tags[tagIterator];
										}
										else
										{
											lastWithUserData = null;
										}
									}
								}
							}
							break;

						case ASE_CHUNK_SLICE:
							{
								uint sliceKeyCount = reader.ReadDword();
								uint sliceFlags = reader.ReadDword();
								reader.IgnoreDword();
								string sliceName = reader.ReadAsepriteString();

								bool isNinePatch = HasFlag(sliceFlags, ASE_SLICE_FLAGS_IS_NINE_PATCH);
								bool hasPivot = HasFlag(sliceFlags, ASE_SLICE_FLAGS_HAS_PIVOT);

								AsepriteSlice slice = new AsepriteSlice(isNinePatch, hasPivot, sliceName);

								for (uint i = 0; i < sliceKeyCount; i++)
								{
									uint startFrame = reader.ReadDword();
									int sliceXPosition = reader.ReadLong();
									int sliceYPosition = reader.ReadLong();
									uint sliceWidth = reader.ReadDword();
									uint sliceHeight = reader.ReadDword();

									Rectangle sliceBounds = new Rectangle(sliceXPosition, sliceYPosition, (int)sliceWidth, (int)sliceHeight);
									Rectangle? sliceCenterBounds = null;
									Point? slicePivotPoint = null;

									if (slice.IsNinePatch)
									{
										int centerBoundsX = reader.ReadLong();
										int centerBoundsY = reader.ReadLong();
										uint centerBoundsWidth = reader.ReadDword();
										uint centerBoundsHeight = reader.ReadDword();
										sliceCenterBounds = new Rectangle(centerBoundsX, centerBoundsY, (int)centerBoundsWidth, (int)centerBoundsHeight);
									}

									if (slice.HasPivot)
									{
										int pivotX = reader.ReadLong();
										int pivotY = reader.ReadLong();
										slicePivotPoint = new Point(pivotX, pivotY);
									}

									AsepriteSliceKey key = new AsepriteSliceKey(slice, (int)startFrame, sliceBounds, sliceCenterBounds, slicePivotPoint);
									slice.Keys.Add(key);
								}

								slices.Add(slice);
								lastWithUserData = slice;
							}
							break;

						case ASE_CHUNK_TILESET:
							{
								uint tilesetID = reader.ReadDword();
								uint tilesetFlags = reader.ReadDword();
								uint tileCount = reader.ReadDword();
								ushort tileWidth = reader.ReadWord();
								ushort tileHeight = reader.ReadWord();
								reader.IgnoreShort();
								reader.IgnoreBytes(14);
								string tilesetName = reader.ReadAsepriteString();

								if (HasFlag(tilesetFlags, ASE_TILESET_FLAG_EXTERNAL_FILE))
								{
									reader.Dispose();
									throw new InvalidOperationException($"Tileset '{tilesetName}' includes tileset in external file. This is not supported at this time");
								}

								if (HasFlag(tilesetFlags, ASE_TILESET_FLAG_EMBEDDED))
								{
									uint compressedDataLength = reader.ReadDword();
									byte[] compressedData = reader.ReadBytes((int)compressedDataLength);
									byte[] pixelData = Deflate(compressedData);
									Color[] pixels = PixelsToColor(pixelData, colorDepth, palette, premultiplyAlpha);
									AsepriteTileset tileset = new AsepriteTileset((int)tilesetID, (int)tileCount, tileWidth, tileHeight, tilesetName, pixels);
									tilesets.Add(tileset);
								}
								else
								{
									throw new InvalidOperationException($"Tileset '{tilesetName}' does not include tileset image in file");
								}
							}
							break;

						case ASE_CHUNK_OLD_PALETTE1:
						case ASE_CHUNK_OLD_PALETTE2:
							warnings.Add($"Old Palette Chunk (0x{chunkType:X4}) ignored");
							goto SEEK_TO_CHUNK_END;

						case ASE_CHUNK_CEL_EXTRA:
							warnings.Add($"Cel Extra Chunk (0x{chunkType:x4}) ignored");
							goto SEEK_TO_CHUNK_END;

						case ASE_CHUNK_COLOR_PROFILE:
							warnings.Add($"Color Profile Chunk (0x{chunkType:X4}) ignored");
							goto SEEK_TO_CHUNK_END;

						case ASE_CHUNK_EXTERNAL_FILES:
							warnings.Add($"External Files Chunk (0x{chunkType:X4}) ignored");
							goto SEEK_TO_CHUNK_END;

						case ASE_CHUNK_MASK:
							warnings.Add($"Mask Chunk (0x{chunkType:X4}) ignored");
							goto SEEK_TO_CHUNK_END;

						case ASE_CHUNK_PATH:
							warnings.Add($"Path Chunk (0x{chunkType:X4}) ignored");
							goto SEEK_TO_CHUNK_END;

						default:
							warnings.Add($"Unknown chunk type encountered (0x{chunkType:X4}). Ignored");
							goto SEEK_TO_CHUNK_END;
					}

				SEEK_TO_CHUNK_END:
					reader.BaseStream.Seek(chunkEnd, SeekOrigin.Begin);
				}

				AsepriteFrame frame = new AsepriteFrame($"{name}_{frameNum}", duration, cels, canvasWidth, canvasHeight);
				frames.Add(frame);
			}

			if (palette.Colors.Length != paletteColorCount)
			{
				warnings.Add($"Number of colors in header ({paletteColorCount}) does not match final palette count ({palette.Colors.Length})");
			}

			return new AsepriteFile(name, palette, canvasWidth, canvasHeight, colorDepth, frames, layers, tags, slices, warnings);
		}

		private static bool HasFlag(uint value, uint flag) => (value & flag) != 0;

		private static Color[] PixelsToColor(byte[] pixels, AsepriteColorDepth colorDepth, AsepritePalette palette, bool premultiplyAlpha)
		{
			switch (colorDepth)
			{
				case AsepriteColorDepth.Indexed: return IndexedPixelsToColor(pixels, palette);
				case AsepriteColorDepth.Grayscale: return GrayscalePixelsToColor(pixels, premultiplyAlpha);
				case AsepriteColorDepth.RGBA: return RGBAPixelsToColor(pixels, premultiplyAlpha);
				default: throw new InvalidOperationException($"Unknown Color Depth: {colorDepth}");
			}
		}

		private static Color[] RGBAPixelsToColor(byte[] pixels, bool premultiplyAlpha)
		{
			int bytesPerPixel = (int)AsepriteColorDepth.RGBA / 8;
			Color[] results = new Color[pixels.Length / bytesPerPixel];

			for (int i = 0, b = 0; i < results.Length; i++, b += bytesPerPixel)
			{
				byte red = pixels[b];
				byte green = pixels[b + 1];
				byte blue = pixels[b + 2];
				byte alpha = pixels[b + 3];
				results[i] = premultiplyAlpha ? Color.FromNonPremultiplied(red, green, blue, alpha) : new Color(red, green, blue, alpha);
			}

			return results;
		}

		private static Color[] GrayscalePixelsToColor(byte[] pixels, bool premultiplyAlpha)
		{
			int bytesPerPixel = (int)AsepriteColorDepth.Grayscale / 8;
			Color[] results = new Color[pixels.Length / bytesPerPixel];

			for (int i = 0, b = 0; i < results.Length; i++, b += bytesPerPixel)
			{
				byte red = pixels[b];
				byte green = pixels[b];
				byte blue = pixels[b];
				byte alpha = pixels[b + 1];
				results[i] = premultiplyAlpha ? Color.FromNonPremultiplied(red, green, blue, alpha) : new Color(red, green, blue, alpha);
			}

			return results;
		}

		private static Color[] IndexedPixelsToColor(byte[] pixels, AsepritePalette palette)
		{
			int bytesPerPixel = (int)AsepriteColorDepth.Indexed / 8;
			Color[] results = new Color[pixels.Length / bytesPerPixel];

			for (int i = 0; i < pixels.Length; i++)
			{
				int index = pixels[i];

				if (index == palette.TransparentIndex)
				{
					results[i] = Color.Transparent;
				}
				else
				{
					results[i] = palette.Colors[index];
				}
			}

			return results;
		}

		private static byte[] Deflate(byte[] buffer)
		{
			using (MemoryStream compressedStream = new MemoryStream(buffer))
			{
				//	First 2 bytes are the zlib header information, skip it, we don't need it
				compressedStream.Seek(2, SeekOrigin.Begin);

				using (MemoryStream decompressedStream = new MemoryStream())
				{
					using (DeflateStream deflateStream = new DeflateStream(compressedStream, CompressionMode.Decompress))
					{
						deflateStream.CopyTo(decompressedStream);
						return decompressedStream.ToArray();
					}
				}
			}
		}

		private static ushort ReadWord(this BinaryReader reader) => reader.ReadUInt16();
		private static short ReadShort(this BinaryReader reader) => reader.ReadInt16();
		private static uint ReadDword(this BinaryReader reader) => reader.ReadUInt32();
		private static int ReadLong(this BinaryReader reader) => reader.ReadInt32();
		private static string ReadAsepriteString(this BinaryReader reader) => System.Text.Encoding.UTF8.GetString(reader.ReadBytes(reader.ReadWord()));

		private static void IgnoreBytes(this BinaryReader reader, int len) => reader.BaseStream.Position += len;
		private static void IgnoreByte(this BinaryReader reader) => reader.IgnoreBytes(sizeof(byte));
		private static void IgnoreWord(this BinaryReader reader) => reader.IgnoreBytes(sizeof(ushort));
		private static void IgnoreShort(this BinaryReader reader) => reader.IgnoreBytes(sizeof(short));
		private static void IgnoreDword(this BinaryReader reader) => reader.IgnoreBytes(sizeof(uint));
		private static void IgnoreLong(this BinaryReader reader) => reader.IgnoreBytes(sizeof(int));
		private static void IgnoreString(this BinaryReader reader) => reader.IgnoreBytes(reader.ReadWord());
	}
}
