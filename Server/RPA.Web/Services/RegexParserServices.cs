using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using RPA.Core;
namespace RPA.Web.Services
{
    public interface IRegexParserServices
    {
        Dictionary<string, string> Parse(string content, List<RegexInfor> list);
    }

    public class RegexParserServices : IRegexParserServices
    {
        public RegexParserServices()
        {

        }

        public Dictionary<string, string> Parse(string content, List<RegexInfor> list)
        {
            var res = new Dictionary<string, string>();
            foreach (var item in list)
            {
                string resultValue = string.Empty;
                var checker = Regex.Match(content, item.Pattern, item.Options);
                if (checker.Success)
                {
                    if (item.SplitContent)
                    {
                        string replaceWith = "||";
                        var split = checker.Value.Trim()
                                    // Replace System New Liew
                                    .Replace(System.Environment.NewLine, replaceWith)
                                    // Default New Line
                                    .Replace("\r\n", replaceWith).Replace("\n", replaceWith)
                                    .Replace("\r", replaceWith)
                                    // Space
                                    .Replace(" ", replaceWith)
                                    .Split(replaceWith).Where(x => !string.IsNullOrEmpty(x))
                                    .ToList();
                        if (split.Count > 0)
                        {
                            if (item.ValueIndex == -1)
                            {
                                resultValue = split.Last();
                            }
                            else
                            {
                                if (item.ValueIndex <= split.Count - 1)
                                {
                                    resultValue = split[item.ValueIndex];
                                }
                            }
                        }
                    }
                    else if (!string.IsNullOrEmpty(item.DateFormat))
                    {
                        if (DateTime.TryParseExact(checker.Value, item.DateFormat, new CultureInfo("en-US"), DateTimeStyles.None, out DateTime date))
                        {
                            resultValue = date.ToString("yyyy-MM-dd");
                        }
                    }
                    else
                    {
                        resultValue = checker.Value;
                    }
                }
                res.Add(item.Name, resultValue);
            }
            return res;
        }
    }

}