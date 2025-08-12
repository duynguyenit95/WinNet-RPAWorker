using System.Net.Http;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace Azure.FormRecognizer
{
    public class FormRecognizerUtilities
    {
        private readonly string endPoint;
        private readonly string API_Key;
        private readonly string API_Version = "v2.1";
        private HttpClient httpClient;
        public FormRecognizerUtilities(string endPoint, string apiKey)
        {
            this.endPoint = endPoint;
            this.API_Key = apiKey;
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", API_Key);
        }

        public async Task<FormRecognizerAPIResult> UseRestAPI(string modelID, string filePath)
        {
            var result = new FormRecognizerAPIResult();
            var fileType = GetFileType(filePath);
            if (string.IsNullOrEmpty(fileType))
            {
                result.Content.Add("File type is not supported !");
                return result;
            }
            FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var post_url = $"{endPoint}/formrecognizer/{API_Version}/custom/models/{modelID}/analyze?includeTextDetails=true";
            //httpClient.DefaultRequestHeaders.Add("Content-Type", fileType);
            HttpContent content = new StreamContent(fileStream);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(fileType);
            var response = await httpClient.PostAsync(post_url, content);
            if (response.IsSuccessStatusCode)
            {
                var resultURI = response.Headers.GetValues("Operation-Location").ToList();
                if (resultURI.Any())
                {
                    result.IsSuccess = true;
                }
                foreach (var uri in resultURI)
                {
                    var isSuccess = false;
                    var flag = 0;
                    while (!isSuccess)
                    {
                        var resultResponse = await httpClient.GetAsync(uri);
                        var responseResult = await resultResponse.Content.ReadAsStringAsync();
                        var jObject = JObject.Parse(responseResult);
                        if (jObject["status"].ToString() == "succeeded")
                        {
                            result.Content.Add(responseResult);
                            isSuccess = true;
                        }
                        flag++;
                        if (flag > 5)
                        {
                            result.Content.Add(responseResult);
                            break;
                        }
                        await Task.Delay(1000);
                    }
                }
            }
            else
            {
                result.Content.Add($"{response.StatusCode} {await response.Content.ReadAsStringAsync()}");
            }
            fileStream.Dispose();
            return result;
        }

        public string GetFileType(string filePath)
        {
            var fileExtension = System.IO.Path.GetExtension(filePath);
            switch (fileExtension)
            {
                case ".pdf": return "application/pdf";
                case ".jpeg": return "image/jpeg";
                case ".png": return "image/png";
                case ".bmp": return "image/bmp";
                case ".tiff": return "image/tiff";
                default: return string.Empty;
            }
        }
    }
}
