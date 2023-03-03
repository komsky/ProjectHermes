using Google.Cloud.Speech.V1;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using NAudio.Wave;
using NAudio.CoreAudioApi;

namespace Hermes.Processor
{
    public class GoogleCloudApiClient
    {
        private bool _isInit;
        public const string GOOGLE_APPLICATION_CREDENTIALS_DEFAULT_LOCATION = "C:\\TEMP\\GoogleCredentials.json";

        public GoogleCloudApiClient()
        {
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", GOOGLE_APPLICATION_CREDENTIALS_DEFAULT_LOCATION);
        }
        public void AuthExplicit(string projectId = "", string jsonPath = GOOGLE_APPLICATION_CREDENTIALS_DEFAULT_LOCATION)
        {
            // Explicitly use service account credentials by specifying 
            // the private key file.
            var credential = GoogleCredential.FromFile(jsonPath);
            var storage = StorageClient.Create(credential);
            // Make an authenticated API request.
            var buckets = storage.ListBuckets(projectId);
            foreach (var bucket in buckets)
            {
                Console.WriteLine(bucket.Name);
            }
            _isInit = true;
        }
        public async Task<object> StreamingMicRecognizeAsync(int seconds, Func<string, Task> processResult)
        {
            if (IsMicrophoneAvailable())
            {
                Console.WriteLine("No microphone!");
                return -1;
            }

            var speech = SpeechClient.Create();
            var streamingCall = speech.StreamingRecognize();
            // Write the initial request with the config.
            await streamingCall.WriteAsync(
                new StreamingRecognizeRequest()
                {
                    StreamingConfig = new StreamingRecognitionConfig()
                    {
                        Config = new RecognitionConfig()
                        {
                            Encoding =
                            RecognitionConfig.Types.AudioEncoding.Linear16,
                            SampleRateHertz = 16000,
                            LanguageCode = "en",
                        },
                        InterimResults = true,
                    }
                });
            // Print responses as they arrive.
            Task printResponses = Task.Run(async () =>
            {
                bool hasMore = true;
                while (hasMore)
                {
                    var stream = streamingCall.GetResponseStream();
                    if (stream != null)
                    {
                        hasMore = await stream.MoveNextAsync();
                        foreach (var result in stream.Current.Results)
                        {
                            foreach (var alternative in result.Alternatives)
                            {
                                Console.WriteLine($"Perhaps {alternative.Transcript}?");
                                if (result.IsFinal)
                                {
                                    Console.WriteLine($"Pretty sure you've said: {alternative.Transcript}");
                                    processResult(alternative.Transcript);
                                }
                            }
                        }
                    }
                    else
                    {
                        hasMore = false;
                    }


                }
            });
            // Read from the microphone and stream to API.
            object writeLock = new object();
            bool writeMore = true;
            var waveIn = new WaveInEvent();
            waveIn.DeviceNumber = 0;
            waveIn.WaveFormat = new WaveFormat(16000, 1);
            waveIn.DataAvailable +=
                (object sender, WaveInEventArgs args) =>
                {
                    lock (writeLock)
                    {
                        if (!writeMore) return;
                        streamingCall.WriteAsync(
                            new StreamingRecognizeRequest()
                            {
                                AudioContent = Google.Protobuf.ByteString
                                    .CopyFrom(args.Buffer, 0, args.BytesRecorded)
                            }).Wait();
                    }
                };
            waveIn.StartRecording();
            Console.WriteLine("------- SPEAK NOW, OR STAY SILENT FOREVER ------");
            await Task.Delay(TimeSpan.FromSeconds(seconds));
            // Stop recording and shut down.
            waveIn.StopRecording();
            lock (writeLock) writeMore = false;
            await streamingCall.WriteCompleteAsync();
            await printResponses;
            return 0;
        }

        private bool IsMicrophoneAvailable()
        {
            var enumerator = new MMDeviceEnumerator();
            var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);

            return devices.Any(d =>
            {
                using var capture = new WasapiCapture(d);
                return capture.WaveFormat.Channels > 0;
            });
        }
    }
}
