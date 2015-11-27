using System.Collections.Generic;
using Newtonsoft.Json;


namespace Nez.TexturePackerImporter
{
    public class TexturePackerFile
    {
        [JsonProperty("frames")]
        public List<TexturePackerRegion> Regions;

        [JsonProperty("meta")]
        public TexturePackerMeta Metadata;
    }
}