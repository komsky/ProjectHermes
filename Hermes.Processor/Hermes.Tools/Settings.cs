namespace Hermes.Tools
{
    public class Settings : ISettings
    {
        public static string SectionName => "Settings";
        public string ChatGPTUrl { get; set; }
        public string PauseWord { get; set; }
        public string SendWord { get; set; }
        public int ListeningTimeout { get; set; }
    }
}
