using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Nez.BitmapFonts
{
	public partial class BitmapFont
	{
		/// <summary>
		/// Load font information from the specified <see cref="Stream"/>.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
		/// <exception cref="ArgumentException">Thrown when one or more arguments have unsupported or
		/// illegal values.</exception>
		/// <exception cref="InvalidDataException">Thrown when an Invalid Data error condition occurs.</exception>
		/// <param name="stream">The stream to load.</param>
		public void Load(Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException(nameof(stream));

			if (!stream.CanSeek)
				throw new ArgumentException("Stream must be seekable in order to determine file format.", nameof(stream));

			// read the first five bytes so we can try and work out what the format is
			// then reset the position so the format loaders can work
			var buffer = new byte[5];
			stream.Read(buffer, 0, 5);
			stream.Seek(0, SeekOrigin.Begin);
			var header = Encoding.ASCII.GetString(buffer);

			switch (header)
			{
				case "info ":
					LoadText(stream);
					break;
				case "<?xml":
					LoadXml(stream);
					break;
				default:
					throw new InvalidDataException("Unknown file format.");
			}
		}

		/// <summary>
		/// Load font information from the specified file.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
		/// <exception cref="FileNotFoundException">Thrown when the requested file is not present.</exception>
		/// <param name="filename">The file name to load.</param>
		public void Load(string filename)
		{
			if (string.IsNullOrEmpty(filename))
				throw new ArgumentNullException(nameof(filename));

			if (!File.Exists(filename))
				throw new FileNotFoundException(string.Format("Cannot find file '{0}'.", filename), filename);

			using (var stream = File.OpenRead(filename))
				Load(stream);

			BitmapFontLoader.QualifyResourcePaths(this, Path.GetDirectoryName(filename));
		}

		/// <summary>
		/// Loads font information from the specified string.
		/// </summary>
		/// <param name="text">String containing the font to load.</param>
		/// <remarks>The source data must be in BMFont text format.</remarks>
		public void LoadText(string text)
		{
			using (var reader = new StringReader(text))
				LoadText(reader);
		}

		/// <summary>
		/// Loads font information from the specified stream.
		/// </summary>
		/// <remarks>
		/// The source data must be in BMFont text format.
		/// </remarks>
		/// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
		/// <param name="stream">The stream containing the font to load.</param>
		public void LoadText(Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException(nameof(stream));

			using (var reader = new StreamReader(stream))
				LoadText(reader);
		}

		/// <summary>
		/// Loads font information from the specified <see cref="TextReader"/>.
		/// </summary>
		/// <remarks>
		/// The source data must be in BMFont text format.
		/// </remarks>
		/// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
		/// <param name="reader">The <strong>TextReader</strong> used to feed the data into the font.</param>
		public void LoadText(TextReader reader)
		{
			if (reader == null)
				throw new ArgumentNullException(nameof(reader));

			var pageData = new SortedDictionary<int, Page>();
			var kerningDictionary = new Dictionary<Kerning, int>();
			var charDictionary = new Dictionary<char, Character>();

			string line;
			do
			{
				line = reader.ReadLine();
				if (line != null)
				{
					var parts = BitmapFontLoader.Split(line, ' ');
					if (parts.Length != 0)
					{
						switch (parts[0])
						{
							case "info":
								FamilyName = BitmapFontLoader.GetNamedString(parts, "face");
								FontSize = BitmapFontLoader.GetNamedInt(parts, "size");
								Bold = BitmapFontLoader.GetNamedBool(parts, "bold");
								Italic = BitmapFontLoader.GetNamedBool(parts, "italic");
								Charset = BitmapFontLoader.GetNamedString(parts, "charset");
								Unicode = BitmapFontLoader.GetNamedBool(parts, "unicode");
								StretchedHeight = BitmapFontLoader.GetNamedInt(parts, "stretchH");
								Smoothed = BitmapFontLoader.GetNamedBool(parts, "smooth");
								SuperSampling = BitmapFontLoader.GetNamedInt(parts, "aa");
								Padding = BitmapFontLoader.ParsePadding(
									BitmapFontLoader.GetNamedString(parts, "padding"));
								Spacing = BitmapFontLoader.ParseInt2(BitmapFontLoader.GetNamedString(parts, "spacing"));
								OutlineSize = BitmapFontLoader.GetNamedInt(parts, "outline");
								break;
							case "common":
								LineHeight = BitmapFontLoader.GetNamedInt(parts, "lineHeight");
								BaseHeight = BitmapFontLoader.GetNamedInt(parts, "base");
								TextureSize = new Point(BitmapFontLoader.GetNamedInt(parts, "scaleW"),
									BitmapFontLoader.GetNamedInt(parts, "scaleH"));
								Packed = BitmapFontLoader.GetNamedBool(parts, "packed");
								AlphaChannel = BitmapFontLoader.GetNamedInt(parts, "alphaChnl");
								RedChannel = BitmapFontLoader.GetNamedInt(parts, "redChnl");
								GreenChannel = BitmapFontLoader.GetNamedInt(parts, "greenChnl");
								BlueChannel = BitmapFontLoader.GetNamedInt(parts, "blueChnl");
								break;
							case "page":
								var id = BitmapFontLoader.GetNamedInt(parts, "id");
								var name = BitmapFontLoader.GetNamedString(parts, "file");

								pageData.Add(id, new Page(id, name));
								break;
							case "char":
								var charData = new Character
								{
									Char = (char) BitmapFontLoader.GetNamedInt(parts, "id"),
									Bounds = new Rectangle(BitmapFontLoader.GetNamedInt(parts, "x"),
										BitmapFontLoader.GetNamedInt(parts, "y"),
										BitmapFontLoader.GetNamedInt(parts, "width"),
										BitmapFontLoader.GetNamedInt(parts, "height")),
									Offset = new Point(BitmapFontLoader.GetNamedInt(parts, "xoffset"),
										BitmapFontLoader.GetNamedInt(parts, "yoffset")),
									XAdvance = BitmapFontLoader.GetNamedInt(parts, "xadvance"),
									TexturePage = BitmapFontLoader.GetNamedInt(parts, "page"),
									Channel = BitmapFontLoader.GetNamedInt(parts, "chnl")
								};
								charDictionary.Add(charData.Char, charData);
								break;
							case "kerning":
								var key = new Kerning((char) BitmapFontLoader.GetNamedInt(parts, "first"),
									(char) BitmapFontLoader.GetNamedInt(parts, "second"),
									BitmapFontLoader.GetNamedInt(parts, "amount"));

								if (!kerningDictionary.ContainsKey(key))
									kerningDictionary.Add(key, key.Amount);
								break;
						}
					}
				}
			} while (line != null);

			Pages = BitmapFontLoader.ToArray(pageData.Values);
			Characters = charDictionary;
			Kernings = kerningDictionary;
		}

		/// <summary>
		/// Loads font information from the specified string.
		/// </summary>
		/// <param name="xml">String containing the font to load.</param>
		/// <remarks>The source data must be in BMFont XML format.</remarks>
		public void LoadXml(string xml)
		{
			using (var reader = new StringReader(xml))
				LoadXml(reader);
		}

		/// <summary>
		/// Loads font information from the specified stream. The source data must be in BMFont XML format.
		/// </summary>
		public void LoadXml(Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException("stream");

			using (var reader = new StreamReader(stream))
				LoadXml(reader);
		}

		/// <summary>
		/// Loads font information from the specified <see cref="TextReader"/>.
		/// </summary>
		public void LoadXml(TextReader reader)
		{
			if (reader == null)
				throw new ArgumentNullException("reader");

			var document = new XmlDocument();
			var pageData = new SortedDictionary<int, Page>();
			var kerningDictionary = new Dictionary<Kerning, int>();
			var charDictionary = new Dictionary<char, Character>();

			document.Load(reader);
			var root = document.DocumentElement;

			// load the basic attributes
			var properties = root.SelectSingleNode("info");
			FamilyName = properties.Attributes["face"].Value;
			FontSize = Convert.ToInt32(properties.Attributes["size"].Value);
			Bold = Convert.ToInt32(properties.Attributes["bold"].Value) != 0;
			Italic = Convert.ToInt32(properties.Attributes["italic"].Value) != 0;
			Unicode = properties.Attributes["unicode"].Value != "0";
			StretchedHeight = Convert.ToInt32(properties.Attributes["stretchH"].Value);
			Charset = properties.Attributes["charset"].Value;
			Smoothed = Convert.ToInt32(properties.Attributes["smooth"].Value) != 0;
			SuperSampling = Convert.ToInt32(properties.Attributes["aa"].Value);
			Padding = BitmapFontLoader.ParsePadding(properties.Attributes["padding"].Value);
			Spacing = BitmapFontLoader.ParseInt2(properties.Attributes["spacing"].Value);
			OutlineSize = properties.Attributes["outline"] != null
				? Convert.ToInt32(properties.Attributes["outline"].Value)
				: 0;

			// common attributes
			properties = root.SelectSingleNode("common");
			BaseHeight = Convert.ToInt32(properties.Attributes["base"].Value);
			LineHeight = Convert.ToInt32(properties.Attributes["lineHeight"].Value);
			TextureSize = new Point(Convert.ToInt32(properties.Attributes["scaleW"].Value),
				Convert.ToInt32(properties.Attributes["scaleH"].Value));
			Packed = Convert.ToInt32(properties.Attributes["packed"].Value) != 0;

			AlphaChannel = properties.Attributes["alphaChnl"] != null
				? Convert.ToInt32(properties.Attributes["alphaChnl"].Value)
				: 0;
			RedChannel = properties.Attributes["redChnl"] != null
				? Convert.ToInt32(properties.Attributes["redChnl"].Value)
				: 0;
			GreenChannel = properties.Attributes["greenChnl"] != null
				? Convert.ToInt32(properties.Attributes["greenChnl"].Value)
				: 0;
			BlueChannel = properties.Attributes["blueChnl"] != null
				? Convert.ToInt32(properties.Attributes["blueChnl"].Value)
				: 0;

			// load texture information
			foreach (XmlNode node in root.SelectNodes("pages/page"))
			{
				var page = new Page();
				page.Id = Convert.ToInt32(node.Attributes["id"].Value);
				page.Filename = node.Attributes["file"].Value;

				pageData.Add(page.Id, page);
			}

			Pages = BitmapFontLoader.ToArray(pageData.Values);

			// load character information
			foreach (XmlNode node in root.SelectNodes("chars/char"))
			{
				var character = new Character();
				character.Char = (char) Convert.ToInt32(node.Attributes["id"].Value);
				character.Bounds = new Rectangle(Convert.ToInt32(node.Attributes["x"].Value),
					Convert.ToInt32(node.Attributes["y"].Value),
					Convert.ToInt32(node.Attributes["width"].Value),
					Convert.ToInt32(node.Attributes["height"].Value));
				character.Offset = new Point(Convert.ToInt32(node.Attributes["xoffset"].Value),
					Convert.ToInt32(node.Attributes["yoffset"].Value));
				character.XAdvance = Convert.ToInt32(node.Attributes["xadvance"].Value);
				character.TexturePage = Convert.ToInt32(node.Attributes["page"].Value);
				character.Channel = Convert.ToInt32(node.Attributes["chnl"].Value);

				charDictionary[character.Char] = character;
			}

			Characters = charDictionary;

			// loading kerning information
			foreach (XmlNode node in root.SelectNodes("kernings/kerning"))
			{
				var key = new Kerning((char) Convert.ToInt32(node.Attributes["first"].Value),
					(char) Convert.ToInt32(node.Attributes["second"].Value),
					Convert.ToInt32(node.Attributes["amount"].Value));

				if (!kerningDictionary.ContainsKey(key))
					kerningDictionary.Add(key, key.Amount);
			}

			Kernings = kerningDictionary;
		}

		void LoadTextures()
		{
			Textures = new Texture2D[Pages.Length];
			for (var i = 0; i < Textures.Length; i++)
				Textures[i] = Texture2D.FromStream(Core.GraphicsDevice, TitleContainer.OpenStream(Pages[i].Filename));
		}
	}
}