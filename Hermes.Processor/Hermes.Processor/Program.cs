// See https://aka.ms/new-console-template for more information
using Hermes.Processor;

ConfigureSettings();
TextProcessor textProcessor = new TextProcessor();
await textProcessor.Listen();
void ConfigureSettings()
{
    throw new NotImplementedException();
}