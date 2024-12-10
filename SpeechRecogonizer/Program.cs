using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Transcription;
using Newtonsoft.Json;
using SpeechRecogonizer.hubClient;
using SpeechRecogonizer.models;
using SpeechRecogonizer.services;

class Program
{
    private static bool stopRecognition = false;
    private static SpeechRecognizer recognizer;
    static async Task Main(string[] args)
    {
        // Retrieve Azure Speech Service settings
        string apiKey = "demo";
        string serviceRegion = "eastus";

        var signalRClient = new SignalRClient();
        // Connect to the SignalR server
        await signalRClient.ConnectAsync();
        // Start speech recognition
        await StartRecognitionAsync(apiKey, serviceRegion, signalRClient);
        // Wait for the Enter key to stop the service
        Console.WriteLine("Press Enter again to stop the speech recognition...");
        Console.ReadLine();
        stopRecognition = true; // Stop the recognition
        await StopRecognitionAsync(); // Stop the speech recognizer
        signalRClient.Disconnect();
    }

    private static async Task StartRecognitionAsync(string speechKey, string serviceRegion , SignalRClient signalRClient)
    {
        var config = SpeechConfig.FromSubscription(speechKey, serviceRegion);
        // Extend silence timeout for longer audio
        config.SetProperty(PropertyId.SpeechServiceConnection_EndSilenceTimeoutMs, "50000");
        recognizer = new SpeechRecognizer(config);

        recognizer.Recognizing += (sender, e) =>
        {
           // Console.Clear();
            Console.Write("Listening... ");
            Console.WriteLine(e.Result.Text);
            signalRClient.SendMessageAsync("Recognizing", e.Result.Text);
        };

        recognizer.Recognized += async (sender, e) =>
        {
            if (e.Result.Reason == ResultReason.RecognizedSpeech)
            {
                var speechText = e.Result.Text;
                if (!string.IsNullOrEmpty(speechText))
                {
                    Console.WriteLine($"Recognized: {speechText}");
                    // Additional processing can be added here
                    SentimentAnalysis sentiment = new SentimentAnalysis();
                    SentimentResponse sentimentResponse = await sentiment.AnalyzeSentimentFromApiAsync(speechText);
                    var results = sentimentResponse?.Results?.Documents[0];
                    printResults(results);
                    var responseString = JsonConvert.SerializeObject(sentimentResponse);
                    await signalRClient.SendMessageAsync("Recognized", responseString);

                }
            }
        };

        recognizer.Canceled += (sender, e) =>
        {
            if (e.Reason == CancellationReason.Error)
            {
                Console.WriteLine($"Speech Recognition canceled. Reason: {e.ErrorDetails}");
            }
        };

        recognizer.SessionStopped += (sender, e) =>
        {
            Console.WriteLine("Recognition session stopped.");
        };

        recognizer.SpeechEndDetected += (sender, e) =>
        {
            Console.WriteLine("SpeechEndDetected.");
        };

        await recognizer.StartContinuousRecognitionAsync();
        Console.WriteLine("Speech recognition started. Speak now...");

        while (!stopRecognition)
        {
            await Task.Delay(500); // Keep the recognition running while waiting for Enter key to stop
        }
    }

    private static async Task StopRecognitionAsync()
    {
        if (recognizer != null)
        {
            await recognizer.StopContinuousRecognitionAsync();
            recognizer.Dispose();
            Console.WriteLine("Speech recognition stopped.");
        }
    }

    public static void printResults(SentimentDocument results)
    {
        if (results != null)
        {
            if (results.Sentences != null && results.Sentences.Count > 0)
            {
                Console.WriteLine($"Text: {results.Sentences[0].Text}");
            }
            else
            {
                Console.WriteLine("No sentences available.");
            }

            Console.WriteLine($"Sentiments: {results?.Sentiment}");

            if (results.ConfidenceScores != null)
            {
                Console.WriteLine($"Positive: {results.ConfidenceScores.Positive}");
                Console.WriteLine($"Negative: {results.ConfidenceScores.Negative}");
                Console.WriteLine($"Neutral: {results.ConfidenceScores.Neutral}");
            }
            else
            {
                Console.WriteLine("Confidence scores are not available.");
            }

            Console.WriteLine("***************************************");
        }
        else
        {
            Console.WriteLine("Results are null.");
        }
    }
}
