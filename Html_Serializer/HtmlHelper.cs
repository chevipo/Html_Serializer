using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Html_Serializer
{
    internal class HtmlHelper
    {
        private readonly static HtmlHelper _instance = new HtmlHelper();
        public static HtmlHelper Instnace => _instance;
        public string[] HtmlTags { get; set; }
        public string[] HtmlVoidTags { get; set; }

        private HtmlHelper()
        {
            HtmlTags = JsonSerializer.Deserialize<string[]>(File.ReadAllText("Html_Tags/HtmlTags.json"));
            HtmlVoidTags = JsonSerializer.Deserialize<string[]>(File.ReadAllText("Html_Tags/HtmlVoidTags.json"));
        }
    }
}
