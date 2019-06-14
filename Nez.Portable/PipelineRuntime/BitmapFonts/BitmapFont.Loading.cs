using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;

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
                throw new ArgumentNullException("stream");

            if (!stream.CanSeek)
                throw new ArgumentException("Stream must be seekable in order to determine file format.", "stream");

            // read the first five bytes so we can try and work out what the format is
            // then reset the position so the format loaders can work
            var buffer = new byte[5];
            stream.Read(buffer, 0, 5);
            stream.Seek(0, SeekOrigin.Begin);
            var header = Encoding.ASCII.GetString(buffer);

            switch (header)
            {
                case "info ":
                    this.LoadText(stream);
                    break;
                case "<?xml":
                    this.LoadXml(stream);
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
                throw new ArgumentNullException("fileName");

            if (!File.Exists(filename))
                throw new FileNotFoundException(string.Format("Cannot find file '{0}'.", filename), filename);

            using (var stream = File.OpenRead(filename))
                this.Load(stream);

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
                throw new ArgumentNullException("stream");

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
                throw new ArgumentNullException("reader");

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
                                familyName = BitmapFontLoader.GetNamedString(parts, "face");
                                fontSize = BitmapFontLoader.GetNamedInt(parts, "size");
                                bold = BitmapFontLoader.GetNamedBool(parts, "bold");
                                italic = BitmapFontLoader.GetNamedBool(parts, "italic");
                                charset = BitmapFontLoader.GetNamedString(parts, "charset");
                                unicode = BitmapFontLoader.GetNamedBool(parts, "unicode");
                                stretchedHeight = BitmapFontLoader.GetNamedInt(parts, "stretchH");
                                smoothed = BitmapFontLoader.GetNamedBool(parts, "smooth");
                                superSampling = BitmapFontLoader.GetNamedInt(parts, "aa");
                                padding = BitmapFontLoader.ParsePadding(BitmapFontLoader.GetNamedString(parts, "padding"));
                                spacing = BitmapFontLoader.ParseInt2(BitmapFontLoader.GetNamedString(parts, "spacing"));
                                outlineSize = BitmapFontLoader.GetNamedInt(parts, "outline");
                                break;
                            case "common":
                                lineHeight = BitmapFontLoader.GetNamedInt(parts, "lineHeight");
                                baseHeight = BitmapFontLoader.GetNamedInt(parts, "base");
                                textureSize = new Point(BitmapFontLoader.GetNamedInt(parts, "scaleW"),
                                                            BitmapFontLoader.GetNamedInt(parts, "scaleH"));
                                packed = BitmapFontLoader.GetNamedBool(parts, "packed");
                                alphaChannel = BitmapFontLoader.GetNamedInt(parts, "alphaChnl");
                                redChannel = BitmapFontLoader.GetNamedInt(parts, "redChnl");
                                greenChannel = BitmapFontLoader.GetNamedInt(parts, "greenChnl");
                                blueChannel = BitmapFontLoader.GetNamedInt(parts, "blueChnl");
                                break;
                            case "page":
                                var id = BitmapFontLoader.GetNamedInt(parts, "id");
                                var name = BitmapFontLoader.GetNamedString(parts, "file");

                                pageData.Add(id, new Page(id, name));
                                break;
                            case "char":
                                var charData = new Character
                                {
                                    character = (char)BitmapFontLoader.GetNamedInt(parts, "id"),
                                    bounds = new Rectangle(BitmapFontLoader.GetNamedInt(parts, "x"),
                                                             BitmapFontLoader.GetNamedInt(parts, "y"),
                                                             BitmapFontLoader.GetNamedInt(parts, "width"),
                                                             BitmapFontLoader.GetNamedInt(parts, "height")),
                                    offset = new Point(BitmapFontLoader.GetNamedInt(parts, "xoffset"),
                                                         BitmapFontLoader.GetNamedInt(parts, "yoffset")),
                                    xAdvance = BitmapFontLoader.GetNamedInt(parts, "xadvance"),
                                    texturePage = BitmapFontLoader.GetNamedInt(parts, "page"),
                                    channel = BitmapFontLoader.GetNamedInt(parts, "chnl")
                                };
                                charDictionary.Add(charData.character, charData);
                                break;
                            case "kerning":
                                var key = new Kerning((char)BitmapFontLoader.GetNamedInt(parts, "first"),
                                                  (char)BitmapFontLoader.GetNamedInt(parts, "second"),
                                                  BitmapFontLoader.GetNamedInt(parts, "amount"));

                                if (!kerningDictionary.ContainsKey(key))
                                    kerningDictionary.Add(key, key.amount);
                                break;
                        }
                    }
                }
            } while (line != null);

            pages = BitmapFontLoader.ToArray(pageData.Values);
            characters = charDictionary;
            kernings = kerningDictionary;
        }

        /// <summary>
        /// Loads font information from the specified string.
        /// </summary>
        /// <param name="xml">String containing the font to load.</param>
        /// <remarks>The source data must be in BMFont XML format.</remarks>
        public void LoadXml(string xml)
        {
            using (var reader = new StringReader(xml))
                this.LoadXml(reader);
        }

        /// <summary>
        /// Loads font information from the specified stream. The source data must be in BMFont XML format.
        /// </summary>
        public void LoadXml(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            using (var reader = new StreamReader(stream))
                this.LoadXml(reader);
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
            familyName = properties.Attributes["face"].Value;
            fontSize = Convert.ToInt32(properties.Attributes["size"].Value);
            bold = Convert.ToInt32(properties.Attributes["bold"].Value) != 0;
            italic = Convert.ToInt32(properties.Attributes["italic"].Value) != 0;
            unicode = properties.Attributes["unicode"].Value != "0";
            stretchedHeight = Convert.ToInt32(properties.Attributes["stretchH"].Value);
            charset = properties.Attributes["charset"].Value;
            smoothed = Convert.ToInt32(properties.Attributes["smooth"].Value) != 0;
            superSampling = Convert.ToInt32(properties.Attributes["aa"].Value);
            padding = BitmapFontLoader.ParsePadding(properties.Attributes["padding"].Value);
            spacing = BitmapFontLoader.ParseInt2(properties.Attributes["spacing"].Value);
            outlineSize = properties.Attributes["outline"] != null ? Convert.ToInt32(properties.Attributes["outline"].Value) : 0;

            // common attributes
            properties = root.SelectSingleNode("common");
            baseHeight = Convert.ToInt32(properties.Attributes["base"].Value);
            lineHeight = Convert.ToInt32(properties.Attributes["lineHeight"].Value);
            textureSize = new Point(Convert.ToInt32(properties.Attributes["scaleW"].Value), Convert.ToInt32(properties.Attributes["scaleH"].Value));
            packed = Convert.ToInt32(properties.Attributes["packed"].Value) != 0;

            alphaChannel = properties.Attributes["alphaChnl"] != null ? Convert.ToInt32(properties.Attributes["alphaChnl"].Value) : 0;
            redChannel = properties.Attributes["redChnl"] != null ? Convert.ToInt32(properties.Attributes["redChnl"].Value) : 0;
            greenChannel = properties.Attributes["greenChnl"] != null ? Convert.ToInt32(properties.Attributes["greenChnl"].Value) : 0;
            blueChannel = properties.Attributes["blueChnl"] != null ? Convert.ToInt32(properties.Attributes["blueChnl"].Value) : 0;

            // load texture information
            foreach (XmlNode node in root.SelectNodes("pages/page"))
            {
                var page = new Page();
                page.id = Convert.ToInt32(node.Attributes["id"].Value);
                page.filename = node.Attributes["file"].Value;

                pageData.Add(page.id, page);
            }
            pages = BitmapFontLoader.ToArray(pageData.Values);

            // load character information
            foreach (XmlNode node in root.SelectNodes("chars/char"))
            {
                var character = new Character();
                character.character = (char)Convert.ToInt32(node.Attributes["id"].Value);
                character.bounds = new Rectangle(Convert.ToInt32(node.Attributes["x"].Value),
                                                 Convert.ToInt32(node.Attributes["y"].Value),
                                                 Convert.ToInt32(node.Attributes["width"].Value),
                                                 Convert.ToInt32(node.Attributes["height"].Value));
                character.offset = new Point(Convert.ToInt32(node.Attributes["xoffset"].Value),
                                             Convert.ToInt32(node.Attributes["yoffset"].Value));
                character.xAdvance = Convert.ToInt32(node.Attributes["xadvance"].Value);
                character.texturePage = Convert.ToInt32(node.Attributes["page"].Value);
                character.channel = Convert.ToInt32(node.Attributes["chnl"].Value);

                charDictionary[character.character] = character;
            }
            characters = charDictionary;

            // loading kerning information
            foreach (XmlNode node in root.SelectNodes("kernings/kerning"))
            {
                var key = new Kerning((char)Convert.ToInt32(node.Attributes["first"].Value),
                                  (char)Convert.ToInt32(node.Attributes["second"].Value),
                                  Convert.ToInt32(node.Attributes["amount"].Value));

                if (!kerningDictionary.ContainsKey(key))
                    kerningDictionary.Add(key, key.amount);
            }
            kernings = kerningDictionary;
        }

        void LoadTextures()
        {
            textures = new Texture2D[pages.Length];
            for (var i = 0; i < textures.Length; i++)
                textures[i] = Texture2D.FromStream(Core.graphicsDevice, TitleContainer.OpenStream(pages[i].filename));
        }
    }
}
