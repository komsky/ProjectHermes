using NAudio.Wave;
using DeepSpeechClient.Interfaces;
using DeepSpeechClient;
using System.Diagnostics;
using DeepSpeechClient.Models;
using NAudio.CoreAudioApi;

namespace Hermes.Processor
{
    public class DeepSpeechSimpleClient
    {
        MMDevice _captureDevice;
        WaveFormat _format; 
        public string DefaultModelsLocation; 
        public string ModelVersion => "deepspeech-0.9.3-models";
        public DeepSpeechSimpleClient() 
        {
            // Get the audio capture device to use
            var enumerator = new MMDeviceEnumerator();
            _captureDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Console);
            _format = new WaveFormat(16000, 16, 1);
            Console.WriteLine($"Using {_captureDevice.DeviceFriendlyName} as capture device");

            if (OperatingSystem.IsLinux())
            {
                DefaultModelsLocation = @"/home/komsky/AcousticModels";
            }
            else
            {
                DefaultModelsLocation = @"C:\\TFS\\Hermes\\Hermes.Processor\\DeepSpeechClient\\AcousticModels";
            }
        }

        public async Task Recognize(string[] args)
        {
            string model = Path.Combine(DefaultModelsLocation, $"{ModelVersion}.pbmm"); //tflite - extension for limited resources requires differend loading procedure
            string scorer = Path.Combine(DefaultModelsLocation, $"{ModelVersion}.scorer");
            string audio = Path.Combine(DefaultModelsLocation, $"audio\\combined_respeech.wav"); ;
            var audioFormat = new WaveFormat(16000, 16, 1);
            var bufferSize = audioFormat.AverageBytesPerSecond / 10;
            string hotwords = null;
            bool extended = false;
            if (args.Length > 0)
            {
                model = GetArgument(args, "--model");
                scorer = GetArgument(args, "--scorer");
                audio = GetArgument(args, "--audio");
                hotwords = GetArgument(args, "--hot_words");
                extended = !string.IsNullOrWhiteSpace(GetArgument(args, "--extended"));
            }

            Stopwatch stopwatch = new Stopwatch();
            try
            {
                Console.WriteLine("Loading model...");
                stopwatch.Start();
                // sphinx-doc: csharp_ref_model_start
                using (IDeepSpeech sttClient = new DeepSpeech(model ?? "output_graph.pbmm"))
                {
                    // sphinx-doc: csharp_ref_model_stop
                    stopwatch.Stop();

                    Console.WriteLine($"Model loaded - {stopwatch.Elapsed.Milliseconds} ms");
                    stopwatch.Reset();
                    if (scorer != null)
                    {
                        Console.WriteLine("Loading scorer...");
                        sttClient.EnableExternalScorer(scorer ?? "kenlm.scorer");
                    }

                    if (hotwords != null)
                    {
                        Console.WriteLine($"Adding hot-words {hotwords}");
                        char[] sep = { ',' };
                        string[] word_boosts = hotwords.Split(sep);
                        foreach (string word_boost in word_boosts)
                        {
                            char[] sep1 = { ':' };
                            string[] word = word_boost.Split(sep1);
                            sttClient.AddHotWord(word[0], float.Parse(word[1]));
                        }
                    }

                    RecognizeFromCaptureDevice(extended, stopwatch, sttClient);

                    //RecognizeFromAudioFile(audio, extended, stopwatch, sttClient);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void RecognizeFromCaptureDevice(bool extended, Stopwatch stopwatch, IDeepSpeech sttClient)
        {
            using (var capture = new WasapiCapture(_captureDevice))
            {
                capture.WaveFormat = _format;
                capture.StartRecording();
                Console.WriteLine("Speak now...");
                var stream = new MemoryStream();
                capture.DataAvailable += (_, e) => stream.Write(e.Buffer, 0, e.BytesRecorded);

                while (true)
                {
                    AnimateConsole(5);
                    capture.StopRecording();
                    var audioData = stream.ToArray();
                    var waveBuffer = new WaveBuffer(stream.ToArray());

                    //Console.WriteLine("Running inference....");

                    stopwatch.Start();

                    string speechResult;
                    // sphinx-doc: csharp_ref_inference_start
                    if (extended)
                    {
                        Metadata metaResult = sttClient.SpeechToTextWithMetadata(waveBuffer.ShortBuffer,
                            Convert.ToUInt32(waveBuffer.MaxSize / 2), 1);
                        speechResult = MetadataToString(metaResult.Transcripts[0]);
                    }
                    else
                    {
                        speechResult = sttClient.SpeechToText(waveBuffer.ShortBuffer,
                            Convert.ToUInt32(waveBuffer.MaxSize / 2));
                    }
                    // sphinx-doc: csharp_ref_inference_stop

                    stopwatch.Stop();

                    //Console.WriteLine($"Inference took: {stopwatch.Elapsed.ToString()}");
                    Console.WriteLine((extended ? $"Extended result: " : "Recognized text: ") + speechResult);

                    waveBuffer.Clear();
                    stream.SetLength(0);
                    capture.StartRecording();
                }
            }
        }

        /// <summary>
        /// Animates console
        /// </summary>
        /// <param name="duration">Animation duration in seconds</param>
        private void AnimateConsole(int duration = 10)
        {
            Console.CursorVisible = false; // hide the cursor
            Console.Write("[");
            for (int i = 0; i < duration; i++)
            {
                Console.Write("_");
            }
            Console.Write("]");

            Console.SetCursorPosition(1, Console.CursorTop);
            for (int i = 0; i <= duration; i++)
            {
                Console.SetCursorPosition(1, Console.CursorTop);
                for (int j = 0; j < duration; j++)
                {
                    if (j < i)
                    {
                        Console.Write("*");
                    }
                    else
                    {
                        Console.Write("_");
                    }
                }
                Thread.Sleep(1000);
            }
            Console.CursorVisible = true; // restore the cursor visibility
            Console.WriteLine();
        }

        private static void RecognizeFromAudioFile(string audio, bool extended, Stopwatch stopwatch, IDeepSpeech sttClient)
        {
            string audioFile = audio ?? "arctic_a0024.wav";
            var waveBuffer = new WaveBuffer(File.ReadAllBytes(audioFile));
            using (var waveInfo = new WaveFileReader(audioFile))
            {
                Console.WriteLine("Running inference....");

                stopwatch.Start();

                string speechResult;
                // sphinx-doc: csharp_ref_inference_start
                if (extended)
                {
                    Metadata metaResult = sttClient.SpeechToTextWithMetadata(waveBuffer.ShortBuffer,
                        Convert.ToUInt32(waveBuffer.MaxSize / 2), 1);
                    speechResult = MetadataToString(metaResult.Transcripts[0]);
                }
                else
                {
                    speechResult = sttClient.SpeechToText(waveBuffer.ShortBuffer,
                        Convert.ToUInt32(waveBuffer.MaxSize / 2));
                }
                // sphinx-doc: csharp_ref_inference_stop

                stopwatch.Stop();

                Console.WriteLine($"Audio duration: {waveInfo.TotalTime.ToString()}");
                Console.WriteLine($"Inference took: {stopwatch.Elapsed.ToString()}");
                //Console.WriteLine((extended ? $"Extended result: " : "Recognized text: ") + speechResult);
                Console.WriteLine(speechResult);
            }
            waveBuffer.Clear();
        }

        /// <summary>
        /// Get the value of an argurment.
        /// </summary>
        /// <param name="args">Argument list.</param>
        /// <param name="option">Key of the argument.</param>
        /// <returns>Value of the argument.</returns>
        static string GetArgument(IEnumerable<string> args, string option)
        => args.SkipWhile(i => i != option).Skip(1).Take(1).FirstOrDefault();

        static string MetadataToString(CandidateTranscript transcript)
        {
            var nl = Environment.NewLine;
            string retval =
             Environment.NewLine + $"Recognized text: {string.Join("", transcript?.Tokens?.Select(x => x.Text))} {nl}"
             + $"Confidence: {transcript?.Confidence} {nl}"
             + $"Item count: {transcript?.Tokens?.Length} {nl}"
             + string.Join(nl, transcript?.Tokens?.Select(x => $"Timestep : {x.Timestep} TimeOffset: {x.StartTime} Char: {x.Text}"));
            return retval;
        }
    }
}
