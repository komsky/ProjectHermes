using Iris.Rms.Voice;

namespace Hermes.Processor
{
    public class TextProcessor
    {
        public async Task Listen()
        {
            try
            {
                //var pathToFile = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase), "voicerms.json") ;

                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "C:\\voicerms.json");
                var client = new GoogleCloudApiClient();
                var textResponse = string.Empty;
                do
                {
                    var task = await client.StreamingMicRecognizeAsync(seconds : 10, PushToChat);
                } while (true);
            }
            catch
            {

            }
        }

        private Task PushToChat(string arg)
        {
            throw new NotImplementedException();
        }

        public List<string> SplitTextIntoSentences(string text)
        {
            var sentences = new List<string>();
            var currentSentence = "";

            for (var i = 0; i < text.Length; i++)
            {
                //
                var currentChar = text[i];

                // Add character to current sentence
                currentSentence += currentChar;

                // is the current character a delimiter? if so, add current part to array and clear
                if (
                       // Latin punctuation
                       currentChar == ','
                    || currentChar == ':'
                    || currentChar == '.'
                    || currentChar == '!'
                    || currentChar == '?'
                    || currentChar == ';'
                    || currentChar == '…'
                    // Chinese/japanese punctuation
                    || currentChar == '、'
                    || currentChar == '，'
                    || currentChar == '。'
                    || currentChar == '．'
                    || currentChar == '！'
                    || currentChar == '？'
                    || currentChar == '；'
                    || currentChar == '：'
                    )
                {
                    if (currentSentence.Trim() != "") sentences.Add(currentSentence.Trim());
                    currentSentence = "";
                }
            }
            return sentences;
        }
    }
}
