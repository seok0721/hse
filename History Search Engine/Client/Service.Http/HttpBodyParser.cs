using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace Client.Http_Parser
{
    public class HttpBodyParser
    {
        private HtmlDocument doc;

        private string startNode;

        public HttpBodyParser()
        {
            doc = new HtmlDocument();
            startNode = "//html";
        }

        public List<string> parse(string content)
        {
            List<string> textList = new List<string>();

            doc.LoadHtml(content);

            if (doc.ParseErrors != null && doc.ParseErrors.Count() > 0)
            {
                throw new ArgumentException("Invalid html document");
            }
            else
            {
                if (doc.DocumentNode != null)
                {
                    HtmlNode bodyNode = doc.DocumentNode.SelectSingleNode(startNode);

                    if (bodyNode != null)
                    {
                        string[] bodyInnerText = bodyNode.InnerText.Split(new string[] {System.Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);

                        for (int i = 0; i < bodyInnerText.Length; i++)
                        {
                            string removeSpaceText = bodyInnerText[i].ExceptChars();

                            if (IsRight(removeSpaceText))
                            {
                                textList.AddRange(removeSpaceText.Split(new string[] {" "}, StringSplitOptions.RemoveEmptyEntries));
                            }
                        }

                        return textList;
                    }
                }
            }

            return null;
        }

        private Boolean IsRight(string str)
        {
            if (str.Length == 0 || Regex.IsMatch(str,@"}+") || Regex.IsMatch(str, @"<!--.*?-->") || Regex.IsMatch(str, @"//.*?"))
            {
                return false;
            }

            return true;
        }

        public string StartNode
        {
            get;
            set;
        }
    }

    public static class StringExtension
    {
        public static string ExceptChars(this string str)
        {
            StringBuilder sb = new StringBuilder(str.Length);
            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];
                switch (c)
                {
                    case '\t':
                    case '\r':
                    case '\n':
                        continue;
                    default:
                        sb.Append(c);
                        break;
                }
            }
            return sb.ToString();
        }
    }
}
