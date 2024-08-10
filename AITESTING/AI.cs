using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml;
using Windows.UI;

namespace AITESTING
{
    internal class AI
    {
        public static async Task SendRequestAsync(string message)
        {
            var client = new HttpClient();
            var requestBody = new RequestBody
            {
                Messages = new[]
                {
                    new Message { Role = "user", Content = message }
                },
                SystemPrompt = "",
                Temperature = 0.9,
                TopK = 5,
                TopP = 0.9,
                MaxTokens = 256,
                WebAccess = false
            };

            var memoryStream = new MemoryStream();
            var serializer = new DataContractJsonSerializer(typeof(RequestBody));
            serializer.WriteObject(memoryStream, requestBody);
            memoryStream.Position = 0;
            var content = new StringContent(Encoding.UTF8.GetString(memoryStream.ToArray()), Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://chatgpt-42.p.rapidapi.com/chatgpt"),
                Headers =
                {
                    { "x-rapidapi-key", "" },
                    { "x-rapidapi-host", "chatgpt-42.p.rapidapi.com" }
                },
                Content = content
            };

            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
            }
        }
    }
}
