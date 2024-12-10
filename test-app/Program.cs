using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Transcription;
using Microsoft.VisualBasic;
using System;
using System.Threading.Tasks;

namespace HelloWorld
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await CreateConversationAsync();
        }

        static async Task CreateConversationAsync()
        {
            // Replace these values with the details of your Cognitive Speech subscription
            string subscriptionKey = "Fe3OQtbq04LyxHdS2g7HNZW7en0MwBBAzXKic3UaLfktO1kqTQcsJQQJ99AKACYeBjFXJ3w3AAAYACOGFRJ0";

            // Replace below with your region identifier from here: https://aka.ms/speech/sdkregion
            string region = "eastus";

            // Sets source and target languages.
            // Replace with the languages of your choice, from list found here: https://aka.ms/speech/sttt-languages
            string fromLanguage = "en-US";
            string toLanguage = "en-US";

            // Set this to the display name you want for the conversation host
            string displayName = "The host";

            // Create the task completion source that will be used to wait until the user presses Ctrl + C
            var completionSource = new TaskCompletionSource<bool>();

            // Register to listen for Ctrl + C
            Console.CancelKeyPress += (s, e) =>
            {
                completionSource.TrySetResult(true);
                e.Cancel = true; // don't terminate the current process
            };

            // Create an instance of the speech translation config
            var config = SpeechTranslationConfig.FromSubscription(subscriptionKey, region);
            config.SpeechRecognitionLanguage = fromLanguage;
            config.AddTargetLanguage(toLanguage);

            // Create the conversation
            using (var conversation = await Conversation.CreateConversationAsync(config).ConfigureAwait(false))
            {
                // Start the conversation so the host user and others can join
                await conversation.StartConversationAsync().ConfigureAwait(false);

                // Get the conversation ID. It will be up to your scenario to determine how this is shared with other participants.
                string conversationId = conversation.ConversationId;
                Console.WriteLine($"Created '{conversationId}' conversation");

                // At this point, you can use the conversation object to manage the conversation. 
                // For example, to mute everyone else in the room you can call this method:
                await conversation.MuteAllParticipantsAsync().ConfigureAwait(false);

                // Configure which audio source you want to use. If you are using a text only language, you 
                // can use the other overload of the constructor that takes no arguments
                var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
                using (var conversationTranslator = new ConversationTranslator(audioConfig))
                {
                    // You should connect all the event handlers you need at this point
                    conversationTranslator.SessionStarted += (s, e) =>
                    {
                        Console.WriteLine($"Session started: {e.SessionId}");
                    };
                    conversationTranslator.SessionStopped += (s, e) =>
                    {
                        Console.WriteLine($"Session stopped: {e.SessionId}");
                    };
                    conversationTranslator.Canceled += (s, e) =>
                    {
                        switch (e.Reason)
                        {
                            case CancellationReason.EndOfStream:
                                Console.WriteLine($"End of audio reached");
                                break;

                            case CancellationReason.Error:
                                Console.WriteLine($"Canceled due to error. {e.ErrorCode}: {e.ErrorDetails}");
                                break;
                        }
                    };
                    conversationTranslator.ConversationExpiration += (s, e) =>
                    {
                        Console.WriteLine($"Conversation will expire in {e.ExpirationTime.TotalMinutes} minutes");
                    };
                    conversationTranslator.ParticipantsChanged += (s, e) =>
                    {
                        Console.Write("The following participant(s) have ");
                        switch (e.Reason)
                        {
                            case ParticipantChangedReason.JoinedConversation:
                                Console.Write("joined");
                                break;

                            case ParticipantChangedReason.LeftConversation:
                                Console.Write("left");
                                break;

                            case ParticipantChangedReason.Updated:
                                Console.Write("been updated");
                                break;
                        }

                        Console.WriteLine(":");

                        foreach (var participant in e.Participants)
                        {
                            Console.WriteLine($"\t{participant.DisplayName}");
                        }
                    };
                    conversationTranslator.TextMessageReceived += (s, e) =>
                    {
                        Console.WriteLine($"Received an instant message from '{e.Result.ParticipantId}': '{e.Result.Text}'");
                        foreach (var entry in e.Result.Translations)
                        {
                            Console.WriteLine($"\tTranslated into '{entry.Key}': '{entry.Value}'");
                        }
                    };
                    conversationTranslator.Transcribed += (s, e) =>
                    {
                        Console.WriteLine($"Received a transcription from '{e.Result.ParticipantId}': '{e.Result.Text}'");
                        foreach (var entry in e.Result.Translations)
                        {
                            Console.WriteLine($"\tTranslated into '{entry.Key}': '{entry.Value}'");
                        }
                    };
                    conversationTranslator.Transcribing += (s, e) =>
                    {
                        Console.WriteLine($"Received a partial transcription from '{e.Result.ParticipantId}': '{e.Result.Text}'");
                        foreach (var entry in e.Result.Translations)
                        {
                            Console.WriteLine($"\tTranslated into '{entry.Key}': '{entry.Value}'");
                        }
                    };

                    // Enter the conversation to start receiving events
                    await conversationTranslator.JoinConversationAsync(conversation, displayName).ConfigureAwait(false);

                    // You can now send an instant message to all other participants in the room
                    await conversationTranslator.SendTextMessageAsync("The instant message to send").ConfigureAwait(false);

                    // If specified a speech to text language, you can start capturing audio
                    await conversationTranslator.StartTranscribingAsync().ConfigureAwait(false);
                    Console.WriteLine("Started transcribing. Press Ctrl + c to stop");

                    // At this point, you should start receiving transcriptions for what you are saying using the default microphone. Press Ctrl+c to stop audio capture
                    await completionSource.Task.ConfigureAwait(false);

                    // Stop audio capture
                    await conversationTranslator.StopTranscribingAsync().ConfigureAwait(false);

                    // Leave the conversation. After this you will no longer receive events
                    await conversationTranslator.LeaveConversationAsync().ConfigureAwait(false);
                }

                // End the conversation
                await conversation.EndConversationAsync().ConfigureAwait(false);

                // Delete the conversation. Any other participants that are still in the conversation will be removed
                await conversation.DeleteConversationAsync().ConfigureAwait(false);
            }
        }
    }
}