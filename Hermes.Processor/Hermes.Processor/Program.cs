using MasterDevs.ChromeDevTools.Protocol.Chrome.Page;
using MasterDevs.ChromeDevTools.Protocol.Chrome.DOM;
using Hermes.Tools;
using log4net;
using MasterDevs.ChromeDevTools;

namespace Hermes.Processor
{
    internal class Program
    {
        const int ViewPortWidth = 1440;
        const int ViewPortHeight = 900; 
        static ILog _logger = LogManager.GetLogger(typeof(Program));
        static Settings _settings;
        private static void Main(string[] args)
        {
            
            ConfigureSettings();
            TextProcessor textProcessor = new TextProcessor(_logger, _settings);
            DeepSpeechSimpleClient client = new DeepSpeechSimpleClient();
            client.Recognize(new string[0]).Wait();
            //await textProcessor.Listen(); //TODO: Configure events to push text results to Chrome Session
            //Task.Delay(3600000); //waiting one hour just for testing
            //This is the Chrome session
            //Task.Run(async () =>
            //{
            //    // synchronization
            //    var screenshotDone = new ManualResetEventSlim();

            //    // STEP 1 - Run Chrome
            //    var chromeProcessFactory = new ChromeProcessFactory(new StubbornDirectoryCleaner());
            //    using (var chromeProcess = chromeProcessFactory.Create(9222, true))
            //    {
            //        // STEP 2 - Create a debugging session
            //        var sessionInfo = (await chromeProcess.GetSessionInfo()).LastOrDefault();
            //        var chromeSessionFactory = new ChromeSessionFactory();
            //        var chromeSession = chromeSessionFactory.Create(sessionInfo.WebSocketDebuggerUrl);

            //        // STEP 3 - Send a command
            //        //
            //        // Here we are sending a commands to tell chrome to set the viewport size 
            //        // and navigate to the specified URL
            //        await chromeSession.SendAsync(new SetDeviceMetricsOverrideCommand
            //        {
            //            Width = ViewPortWidth,
            //            Height = ViewPortHeight,
            //            Scale = 1
            //        });

            //        var navigateResponse = await chromeSession.SendAsync(new NavigateCommand
            //        {
            //            Url = "http://www.google.com"
            //        });
            //        Console.WriteLine("NavigateResponse: " + navigateResponse.Id);

            //        // STEP 4 - Register for events (in this case, "Page" domain events)
            //        // send an command to tell chrome to send us all Page events
            //        // but we only subscribe to certain events in this session
            //        var pageEnableResult = await chromeSession.SendAsync<MasterDevs.ChromeDevTools.Protocol.Chrome.Page.EnableCommand>();
            //        Console.WriteLine("PageEnable: " + pageEnableResult.Id);

            //        chromeSession.Subscribe<LoadEventFiredEvent>(loadEventFired =>
            //        {
            //            // we cannot block in event handler, hence the task
            //            Task.Run(async () =>
            //            {
            //                Console.WriteLine("LoadEventFiredEvent: " + loadEventFired.Timestamp);

            //                var documentNodeId = (await chromeSession.SendAsync(new GetDocumentCommand())).Result.Root.NodeId;
            //                var bodyNodeId =
            //                    (await chromeSession.SendAsync(new QuerySelectorCommand
            //                    {
            //                        NodeId = documentNodeId,
            //                        Selector = "body"
            //                    })).Result.NodeId;
            //                var height = (await chromeSession.SendAsync(new GetBoxModelCommand { NodeId = bodyNodeId })).Result.Model.Height;

            //                await chromeSession.SendAsync(new SetDeviceMetricsOverrideCommand
            //                {
            //                    Width = ViewPortWidth,
            //                    Height = height,
            //                    Scale = 1
            //                });

            //                Console.WriteLine("Taking screenshot");
            //                var screenshot = await chromeSession.SendAsync(new CaptureScreenshotCommand { Format = "png" });

            //                var data = Convert.FromBase64String(screenshot.Result.Data);
            //                File.WriteAllBytes("output.png", data);
            //                Console.WriteLine("Screenshot stored");

            //                // tell the main thread we are done
            //                screenshotDone.Set();
            //            });
            //        });

            //        // wait for screenshoting thread to (start and) finish
            //        screenshotDone.Wait();

            //        Console.WriteLine("Exiting ..");
            //    }
            //}).Wait();
        }

        private static Task ProcessTextResult(string arg)
        {
            throw new NotImplementedException();
        }

        static void ConfigureSettings()
        {
            JsonConfigurationLoader.TryBuildConfigurationStore<Settings>(Settings.SectionName, out var settings);
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }
    }
}




