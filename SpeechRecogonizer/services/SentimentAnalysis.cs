using Newtonsoft.Json;
using SpeechRecogonizer.models;
using System.Reflection.Metadata;
using System.Text;
namespace SpeechRecogonizer.services
{
    public class SentimentAnalysis
    {

        string apiKey = "3KgYDqBrjpTVTDgq6t5cSPf5u9qqCjBfk8opei46aKh9mQa1lrnEJQQJ99AKACYeBjFXJ3w3AAAaACOGXnn5";
        string apiEndpoint = "https://sentiment-analysis-lab.cognitiveservices.azure.com/language/:analyze-text?api-version=2023-04-15-preview";
        public async Task<SentimentResponse?> AnalyzeSentimentFromApiAsync(string speechText)
        {
            try
            {
                Console.WriteLine($"Please wait analyzing sentiments...");
                var requestBody = new
                {
                    kind = "SentimentAnalysis",
                    parameters = new
                    {
                        modelVersion = "latest",
                        opinionMining = "True"
                    },
                    analysisInput = new
                    {
                        documents = new[]
                         {
                            new
                            {
                                id = "1",
                                language = "en",
                                text = speechText
                            }
                        }
                    }
                };

                // Serialize input data to JSON
                var jsonData = JsonConvert.SerializeObject(requestBody);
                using (var httpClient = new HttpClient())
                {
                    // Set headers
                    httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", apiKey);
                    // Make the POST request
                    var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    var response = await httpClient.PostAsync(apiEndpoint, content);
                    Console.WriteLine($"Please wait analyzing sentiments...");
                    if (response.IsSuccessStatusCode)
                    {
                        // Read and deserialize the response
                        var responseData = await response.Content.ReadAsStringAsync();
                        var sentimentResult = JsonConvert.DeserializeObject<SentimentResponse>(responseData);
                        return sentimentResult;
                    }
                    else
                    {
                        Console.WriteLine($"API call failed: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during API call: {ex.Message}");
            }

            return null;
        }
    }
}
