using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ImageCompressionWorker
{
    public class Function1
    {
        private readonly ILogger<Function1> _logger;

        public Function1(ILogger<Function1> logger)
        {
            _logger = logger;
        }

        [Function("CompressImageAndNotify")]
        public MyOutputType Run(
            // 💡 FIX: Stream ki jagah byte[] kar diya
            [BlobTrigger("izmages/{name}", Connection = "StorageConnection")] byte[] inputBlob,
            string name)
        {
            _logger.LogInformation($"[START] Image mil gayi: {name}");

            // Ab inputBlob pehle se hi bytes mein hai, toh MemoryStream ki zaroorat hi nahi!

            _logger.LogInformation($"[SUCCESS] Image process ho gayi!");

            // 🚀 JADOO: Ek hi return statement se dono jagah data bhejo
            return new MyOutputType
            {
                // A. Direct byte array ko output dabbe mein daal do
                CompressedImage = inputBlob,

                // B. SignalR se browser par Toaster bhejo
                //SignalRMessage = new SignalRMessageAction("ReceiveNotification")
                //{
                //    Arguments = new[] { $"✅ Boom! Teri image '{name}' background mein compress hokar cloud par save ho gayi hai!" }
                //}
            };
        }
    }

    // 📦 MULTIPLE OUTPUT BINDING CLASS
    public class MyOutputType
    {
        // Output 1: Blob Storage
        [BlobOutput("compressed-images/{name}", Connection = "StorageConnection")]
        public byte[] CompressedImage { get; set; }

        // Output 2: SignalR Cloud
        //[SignalROutput(HubName = "notificationHub", ConnectionStringSetting = "AzureSignalRConnectionString")]
        //public
        //
        //Action SignalRMessage { get; set; }
    }
}