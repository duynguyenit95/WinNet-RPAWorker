using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RPA.Core.Data
{
    public abstract class FormRecognizerResultBase
    {
        public string DocType { get; set; }
        public decimal DocTypeConfidence { get; set; }
        public string ModelID { get; set; }

        public string ParseResult { get; set; }

        public List<string> ParsedLog { get; set; } = new List<string>();

        private JObject ParsedJObject { get; set; }

        public FormRecognizerResultBase(string parseResult)
        {
            if (string.IsNullOrEmpty(parseResult))
            {
                return;
            }
            ParseResult = parseResult;
            ParsedJObject = JObject.FromObject(JObject.Parse(parseResult));
            var documentResults = JArray.FromObject(ParsedJObject["analyzeResult"]["documentResults"]);
            var document = JObject.FromObject(documentResults.FirstOrDefault());
            ParseHeader(document);
            ParseField(JObject.FromObject(document["fields"]));
        }
        public void ParseHeader(JObject documentResult)
        {
            DocType = documentResult["docType"].ToString();
            DocTypeConfidence = decimal.Parse(documentResult["docTypeConfidence"].ToString());
            ModelID = documentResult["modelId"].ToString();
        }       

        public virtual void ParseField(JObject fields)
        {

        }

        public virtual string GetString(JToken fieldObject)
        {
            if (fieldObject["type"].ToString() == "string")
            {
                if (fieldObject["valueString"] != null)
                {
                    return fieldObject["valueString"].ToString();
                }
            }
            return string.Empty; 
        }
        public virtual decimal GetDecimal(JToken fieldObject)
        {
            if (fieldObject["type"].ToString() == "number")
            {
                return decimal.Parse(fieldObject["valueNumber"].ToString());
            }
            return 0m;
        }

        public virtual decimal GetInt(JToken fieldObject)
        {
            if (fieldObject["type"].ToString() == "integer")
            {
                if (fieldObject["valueInteger"] != null)
                {
                    return int.Parse(fieldObject["valueInteger"].ToString());
                }

            }
            return 0;
        }
    }
}
