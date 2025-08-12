using System.Text.Json;
using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using SharpCifs.Util.Sharpen;

namespace RPA.Tools
{
    public static class HttpClientHelper
    {
        public static async Task<HttpResponseMessage> HttpSendMail(string[] receiver, string displayName, string subject, string content, List<string> paths, string senderID = null, string senderPassword = null)
        {            
            var fileInfos = new List<FileInfo>();
            foreach(var path in paths)
            {
                fileInfos.Add(new FileInfo(path));
            }
            using (var httpClient = new HttpClient())
            {
                var ctn = HttpClientHelper.CreateFormAndFile(
                    fileInfos,
                    new
                    {
                        DisplayName = displayName,
                        Subject = subject,
                        Content = content,
                        Receivers = receiver,
                        SenderID = senderID,
                        SenderPassword = senderPassword
                    });
                return await httpClient.PostAsync("http://ros:8103/utility/sendMail", ctn);
            }
        }
        public static MultipartFormDataContent CreateFormAndFile(List<FileInfo> files, object data,Encoding encode = null)
        {
            var keys = CreateKeys(data);
            var content = new MultipartFormDataContent();
            foreach (var key in keys)
            {
                content.Add(new StringContent(key.Value, encode != null ? encode : Encoding.UTF8), key.Key);
            }
            foreach(var file in files)
            {
                content.Add(new StreamContent(File.OpenRead(file.FullName)), "files", file.Name);
            }
            return content;
        }
        public static FormUrlEncodedContent CreateForm(object data)
        {
            var result = CreateKeys(data);
            return new FormUrlEncodedContent(result);
        }

        public static List<KeyValuePair<string, string>> CreateKeys(object data)
        {
            var result = new List<KeyValuePair<string, string>>();
            result.AddRange(ObjectToKey(string.Empty, data));
            return result;
        }

        static string ConfigName(string name, string child)
        {
            return string.IsNullOrEmpty(name) ? child : $"{name}[{child}]";
        }

        static List<KeyValuePair<string, string>> ObjectToKey(string name, object obj)
        {
            var result = new List<KeyValuePair<string, string>>();
            // Check if is an ienumerable type
            IEnumerable<object> asIEnum = obj as IEnumerable<object>;
            if (asIEnum != null)
            {
                // loop through it and add index in name
                var index = 0;
                foreach (var asEnumItem in asIEnum)
                {
                    result.AddRange(ObjectToKey(ConfigName(name, index.ToString()), asEnumItem));
                    index += 1;
                }

                return result;
            }

            // If is DateTime
            DateTime? date = obj as DateTime?;
            if (date != null)
            {
                result.Add(new KeyValuePair<string, string>(name, date.Value.ToString("yyyy-MM-dd HH:mm:ss")));
                return result;
            }

            TimeSpan? timeSpan = obj as TimeSpan?;
            if (timeSpan != null)
            {
                result.Add(new KeyValuePair<string, string>(name, timeSpan.Value.ToString()));
                return result;
            }

            var t = obj.GetType();
            if (t.IsPrimitive || t == typeof(Decimal) || t == typeof(String) || t == typeof(Guid))
            {
                result.Add(new KeyValuePair<string, string>(name, obj.ToString()));
                return result;
            }

            if (t.IsEnum)
            {
                result.Add(new KeyValuePair<string, string>(name, ((int)obj).ToString()));
                return result;
            }

            // Check if is an object with property
            var props = obj.GetType().GetProperties().ToList();
            foreach (var prop in props)
            {
                var value = prop.GetValue(obj, null);
                if (value != null)
                {
                    result.AddRange(ObjectToKey(ConfigName(name, prop.Name), value));
                }

            }

            return result;

        }
    }

}
