using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UQ09.Enum;
using UQ09.Model;

namespace UQ09.Handle
{
    public class Collector
    {
        public List<UQParam> GetParam()
        {
            var jsonParamFile = Path.Combine(AppContext.BaseDirectory, "Parameter", "JSONParam.json");
            var jsonText = File.ReadAllText(jsonParamFile);
            var uqParams = ParseJsonToUQParams(jsonText);
            return uqParams;
        }
        static List<UQParam> ParseJsonToUQParams(string jsonText)
        {
            var jObject = JObject.Parse(jsonText);
            var uqParams = new List<UQParam>();

            foreach (var property in jObject.Properties())
            {
                var uqParam = property.Value.ToObject<UQParam>();
                if (uqParam == null) throw new Exception($"Cannot detect parameter");
                uqParam.Area = System.Enum.TryParse<UQArea>(property.Name, out var area) ? area : throw new Exception($"Invalid area: {property.Name}");
                uqParams.Add(uqParam);
            }

            return uqParams;
        }

        public UQMergeParam GetMergeParam()
        {
            var jsonMergeParamFile = Path.Combine(AppContext.BaseDirectory, "Parameter", "MergeParam.json");
            var jsonText = File.ReadAllText(jsonMergeParamFile);
            return JsonConvert.DeserializeObject<UQMergeParam>(jsonText)
                ?? new UQMergeParam()
                {
                    UQ09 = false,
                    UQ09Production = false,
                };
        }
    }
}
