using Azure.Messaging.ServiceBus;
using System.Text.Json;

namespace JobPortalCORE.Services
{
    // BackgroundService lagane se ye website ke piche continuously chalne lagta hai
    public class ServiceBusReceiverWorker : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ServiceBusReceiverWorker> _logger;
        private ServiceBusClient _client;
        private ServiceBusProcessor _processor;

        public ServiceBusReceiverWorker(IConfiguration configuration, ILogger<ServiceBusReceiverWorker> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        // 🟢 Ye method website start hote hi automatically chal jayega
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string connectionString = _configuration.GetConnectionString("ServiceBusConnectionString");
            string queueName = "job-queue"; // Wahi dabba jahan chithi bheji thi

            _client = new ServiceBusClient(connectionString);

            // Sender ki jagah ab hum Processor (Receiver) bana rahe hain
            _processor = _client.CreateProcessor(queueName, new ServiceBusProcessorOptions());

            // 1. Jab naya message aayega toh kya karna hai?
            _processor.ProcessMessageAsync += MessageHandler;

            // 2. Agar koi error aayega toh kya karna hai?
            _processor.ProcessErrorAsync += ErrorHandler;

            // Processor ko chalu kar do (Watchman ko duty par bhej do)
            await _processor.StartProcessingAsync(stoppingToken);

            _logger.LogInformation("🚀 Service Bus Receiver (Watchman) duty par lag gaya hai!");
        }

        // 📨 THE REAL JADOO: Jaise hi queue mein message aayega, ye method chalega
        // 📨 THE REAL JADOO: Message padho aur Email bhej do!
        private async Task MessageHandler(ProcessMessageEventArgs args)
        {
            string body = args.Message.Body.ToString();
            _logger.LogInformation($"\n📦 NAYA MESSAGE AAYA HAI! \nData: {body}\n");

            try
            {
                // 1. JSON string ko wapas C# Object mein Decode (Deserialize) karo
                var payload = JsonSerializer.Deserialize<ServiceBusPayload>(body);

                if (payload != null && !string.IsNullOrEmpty(payload.CandidateEmail))
                {
                    // 2. 👨‍🍳 ASLI KAAM: Email Bhejna (Tere Gmail SMTP se)
                    SendRealEmail(payload.CandidateName, payload.CandidateEmail, payload.UserMessage);
                    _logger.LogInformation($"📧 SUCCESS: {payload.CandidateName} ({payload.CandidateEmail}) ko Cloud se Mail chali gayi!");
                }

                // 3. Sab theek raha toh Queue ko bolo chithi delete kar de
                await args.CompleteMessageAsync(args.Message);
                _logger.LogInformation("✅ Message safely queue se uda diya gaya!");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Galti ho gayi: {ex.Message}");
                // Note: Humne yahan CompleteMessageAsync nahi bulaya.
                // Iska matlab agar mail fail hui, toh message queue mein wapas chala jayega aur Retry hoga!
            }
        }

        // 📧 Mail bhejne ka helper method (Wahi tera purana code)
        private void SendRealEmail(string name, string email, string messageData)
        {
            System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
            System.Net.Mail.SmtpClient smtpClient = new System.Net.Mail.SmtpClient();

            message.From = new System.Net.Mail.MailAddress("gulshankumar.mailid01@gmail.com");
            message.To.Add(email);
            message.Subject = "Hello from Azure Service Bus! ☁️";

            string MailBody = $"<h3>Cloud Architecture Test</h3><p><b>Name:</b> {name}</p><p><b>Message:</b> {messageData}</p><p>Ye email direct Azure Service Bus Worker se aayi hai!</p>";

            message.Body = MailBody;
            message.IsBodyHtml = true;

            smtpClient.Port = 587;
            smtpClient.Host = "smtp.gmail.com";
            smtpClient.EnableSsl = true;
            smtpClient.UseDefaultCredentials = false;
            // ⚠️ Apna wahi 16-digit wala App Password yahan daal dena
            smtpClient.Credentials = new System.Net.NetworkCredential("gulshankumar.mailid01@gmail.com", "qrzxtmcskcrwiduc");
            smtpClient.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;

            smtpClient.Send(message);
        }

        // ❌ Agar connection mein ya code mein error aaya
        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            _logger.LogError($"Galti ho gayi bhai: {args.Exception.Message}");
            return Task.CompletedTask;
        }

        // 🔴 Jab website band hogi toh watchman ko bhi duty se hatao
        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🛑 Receiver band ho raha hai...");
            await _processor.StopProcessingAsync(stoppingToken);
            await _processor.DisposeAsync();
            await _client.DisposeAsync();
            await base.StopAsync(stoppingToken);
        }
    }

    // Ye class JSON ke data ko C# mein hold karegi
    public class ServiceBusPayload
    {
        public string CandidateName { get; set; }
        public string CandidateEmail { get; set; }
        public string UserMessage { get; set; }
        public DateTime AppliedAt { get; set; }
    }
}