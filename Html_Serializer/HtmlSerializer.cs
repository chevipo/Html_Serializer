using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Html_Serializer
{
    internal class HtmlSerializer
    {
        public async Task<string> Load(string url)
        {
            HttpClient client = new HttpClient();
            var response = await client.GetAsync(url);
            var html = await response.Content.ReadAsStringAsync();
            return html;
        }
        public HtmlElement Serialize(string html)
        {
            return BuildHtmlTree(SeparateHtmlTags(html));
        }

        private List<string> SeparateHtmlTags(string html)
        {
            var cleanHtml = new Regex("\\s+").Replace(html, " ");
            var scriptRegex = new Regex("<script.*?</script>", RegexOptions.Singleline);

            var scriptMatches = scriptRegex.Matches(cleanHtml);
            var placeholders = new Dictionary<string, string>();

            const string placeholderFormat = "SCRIPT_PLACEHOLDER_{0}";

            foreach (Match match in scriptMatches)
            {
                var uniquePlaceholder = $"{string.Format(placeholderFormat, placeholders.Count)}";
                placeholders[uniquePlaceholder] = match.Value.Trim();
                cleanHtml = cleanHtml.Replace(match.Value, $"<{uniquePlaceholder}>");
            }

            var tags = new Regex("<(.*?)>").Split(cleanHtml)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => line.Trim());

            var placeholderRegex = new Regex($"{placeholderFormat.Replace("{0}", @"(\d+)")}");
            var restoredTags = new List<string>();
            foreach (var tag in tags)
            {
                if (placeholderRegex.IsMatch(tag))
                {
                    var openTag = new Regex("<(.*?)>").Match(placeholders[tag]).Value;
                    restoredTags.Add(openTag.Substring(1, openTag.Length - 2));

                    var openTagEndIndex = placeholders[tag].IndexOf('>') + 1;
                    var closeTagStartIndex = placeholders[tag].LastIndexOf("</script>");

                    var content = placeholders[tag].Substring(openTagEndIndex, closeTagStartIndex - openTagEndIndex).Trim();
                    if (!string.IsNullOrEmpty(content))
                    {
                        restoredTags.Add(content);
                    }

                    restoredTags.Add("/script");

                }
                else
                {
                    restoredTags.Add(tag);
                }
            }

            return restoredTags;
        }

        private HtmlElement BuildHtmlTree(List<string> htmlLines)
        {
            HtmlElement root = new HtmlElement() { Name = "html" }, current = root;

            foreach (var line in htmlLines)
            {
                var firstWord = line.Split()[0];
                if (firstWord == "/html")
                    break;
                if (firstWord.StartsWith('/') && HtmlHelper.Instnace.HtmlTags.Contains(firstWord.Substring(1)))
                    current = current.Parent;
                else if (HtmlHelper.Instnace.HtmlTags.Contains(firstWord)
                    || HtmlHelper.Instnace.HtmlVoidTags.Contains(firstWord))
                {
                    var element = new HtmlElement(firstWord) { Parent = current };
                    current.Children.Add(element);
                    var attributeString = line.Substring(firstWord.Length);
                    element.Attributes = Regex.Matches(attributeString, "([^\\s]*?)=\"(.*?)\"")
                        .ToDictionary(
                        match => match.Groups[1].Value,
                        match => match.Groups[2].Value
                        );
                    element.Classes = element.Attributes.GetValueOrDefault("class", "")
                        .Split(" ", StringSplitOptions.RemoveEmptyEntries)
                        .ToList<string>();
                    element.Id = element.Attributes.GetValueOrDefault("id", null);
                    current = HtmlHelper.Instnace.HtmlVoidTags.Contains(element.Name) || line.EndsWith('/') ? current : element;

                }
                else
                    current.InnerHtml += line;
            }
            return root;
        }
    }
}
