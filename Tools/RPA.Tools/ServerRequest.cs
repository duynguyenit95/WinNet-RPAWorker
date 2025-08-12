using System.Text.Json;
using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace RPA.Tools
{
    public static class ServerRequest
    {
        public static async Task<bool> Post(string _serverURL, string path, MultipartFormDataContent formContent, string _token)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Token", _token);
                var response = await client.PostAsync(_serverURL + path, formContent);
                var content = response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return true;
                }
            }
            return false;
        }

        public static async Task<bool> Post(string _serverURL, string path, FormUrlEncodedContent formContent, string _token)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Token", _token);
                var response = await client.PostAsync(_serverURL + path, formContent);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return true;
                }
            }
            return false;
        }

        public static async Task<T> Post<T>(string _serverURL, string path, FormUrlEncodedContent formContent, string _token) where T : class
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Token", _token);
                var response = await client.PostAsync(_serverURL + path, formContent);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(content))
                    {
                        return null;
                    }
                    var requestResult = JsonConvert.DeserializeObject<T>(content);
                    return requestResult;
                }
            }
            return null;
        }

        public static async Task<T> Get<T>(string _serverURL, string path, string token) where T : class
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Token", token);
                var response = await client.GetAsync(_serverURL + path);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(content))
                    {
                        return null;
                    }
                    var requestResult = JsonConvert.DeserializeObject<T>(content);
                    return requestResult;
                }
            }
            return null;
        }

        public static async Task<T> PostSingleFile<T>(Uri uri, string filePath) where T : class
        {
            using (HttpClient client = new HttpClient())
            {
                using (FileStream fsSource = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    var streamContent = new StreamContent(fsSource);
                    var response = await client.PostAsync(uri, streamContent);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var requestResult = JsonConvert.DeserializeObject<T>(content);
                        return requestResult;
                    }
                }

            }
            return null;
        }
    }

}
