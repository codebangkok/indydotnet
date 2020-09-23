using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Features.DidExchange;
using Hyperledger.Aries;
using Hyperledger.Aries.Routing;
using static Hyperledger.Aries.Storage.WalletConfiguration;
using System.IO;
using Hyperledger.Aries.Storage;
using System.Collections.Generic;
using Hyperledger.Aries.Extensions;
using Hyperledger.Aries.Utils;

namespace EdgeConsole
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // string walletId = args.Length > 0 ? args[0] : "surasuk";
            // string agentName = args.Length > 1 ? args[1] : "Surasuk Oakkharaamonphong";
            // string endpointUri = args.Length > 2 ? args[2] : "https://crednet-mediator.azurewebsites.net";
            // // string endpointUri = args.Length > 2 ? args[2] : "http://localhost:5004";
            // string imageUrl = args.Length > 3 ? args[3] : $"{endpointUri}/images/{walletId}.jpg";            

            string walletId = Environment.GetEnvironmentVariable("WALLET_ID") ?? "surasuk";
            string agentName = Environment.GetEnvironmentVariable("AGENT_NAME") ?? "Surasuk Oakkharaamonphong";            
            string endpointUri = Environment.GetEnvironmentVariable("ENDPOINT_HOST") ?? "http:// 10.9.214.237:30901";
            string imageUrl = Environment.GetEnvironmentVariable("IMAGE_URL") ?? $"{endpointUri}/images/{walletId}.jpg";            

            var p = new Program(walletId, agentName, imageUrl, endpointUri);
            await p.StartHost();
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Edge Console ===");
                Console.WriteLine($"Wallet ID: {walletId}");
                Console.WriteLine($"Agent Name: {agentName}");
                Console.WriteLine($"Image Url: {imageUrl}");
                Console.WriteLine($"Endpoint: {endpointUri}");
                Console.WriteLine($"--------------------");
                Console.WriteLine($"1. List Connection");
                Console.WriteLine($"2. Delete Connection");
                Console.WriteLine($"3. Create Invitation");
                Console.WriteLine($"4. Revoke Invitation");
                Console.WriteLine($"5. Accept Invitation");
                Console.WriteLine($"6. Fetch Inbox");                                
                Console.WriteLine($"7. Register Device");
                Console.WriteLine($"8. Backup");
                Console.WriteLine($"9. Restore");
                Console.Write("Menu = ");
                var menu = int.Parse(Console.ReadLine());

                switch (menu)
                {
                    case 1: await p.ListConnection(); break;
                    case 2: await p.DeleteConnection(); break;
                    case 3: await p.CreateInvitation(); break;
                    case 4: await p.RevokeInvitation(); break;
                    case 5: await p.AcceptInvitation(); break;
                    case 6: await p.FetchInbox(); break;                    
                    case 7: await p.RegisterDevice(); break;
                    case 8: await p.Backup(); break;
                    case 9: await p.Restore(); break;
                }
            }
        }

        IHost Host { get; set; }
        string HostStatus => Host == null ? "None" : "Ok";

        IAgentContext agentContext;
        IConnectionService connectionService;
        IEdgeClientService edgeClientService;      
        IMessageService messageService;          

        string walletId;
        string agentName;
        string imageUrl;
        string endpointUri;

        public Program(string walletId, string agentName, string imageUrl, string endpointUri)
        {
            this.walletId = walletId;
            this.agentName = agentName;
            this.imageUrl = imageUrl;
            this.endpointUri = endpointUri;

            Console.CancelKeyPress += new ConsoleCancelEventHandler(Exit);
        }        

        protected async void Exit(object sender, ConsoleCancelEventArgs args)
        {
            await StopHost();       
            Environment.Exit(0);
        }

        public async Task StartHost()
        {
            Host = CreateHostBuilder(walletId, agentName, imageUrl, endpointUri).Build();
            try
            {
                await Host.StartAsync();                
                agentContext = await Host.Services.GetRequiredService<IAgentProvider>().GetContextAsync();
                connectionService = Host.Services.GetRequiredService<IConnectionService>();
                edgeClientService = Host.Services.GetRequiredService<IEdgeClientService>();
                messageService = Host.Services.GetRequiredService<IMessageService>();                                  
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }

        public async Task StopHost()
        {
            try
            {
                await Host.StopAsync();
                await Host.WaitForShutdownAsync();
                Host.Dispose();
                Host = null;             
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }

        public async Task ListConnection()
        {              
            try
            {                             
                var connections = await connectionService.ListAsync(agentContext);
                foreach (var connection in connections)
                {
                    Console.WriteLine($"{connection.Id} [{connection.State}] {connection.Alias?.Name}");
                }                           
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
            
            Console.Write("Press enter to continue.");  
            Console.ReadLine();
        }

        public async Task DeleteConnection()
        {           
            Console.Write("Connection ID = ");
            var connectionId = Console.ReadLine();   
            try
            {                             
                var isSuccess = await connectionService.DeleteAsync(agentContext, connectionId);
                if (isSuccess) Console.Write("Delete connection success.");
                else Console.WriteLine("Delete connection failed.");
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
            
            Console.ReadLine();
        }

        public async Task CreateInvitation()
        {            
            try
            {
                var inviteConfig = new InviteConfiguration { 
                    AutoAcceptConnection = true
                };
                var (invitation, record) = await connectionService.CreateInvitationAsync(agentContext, inviteConfig);                
                var invitationUrl = $"{endpointUri}?c_i={invitation.ToJson().ToBase64()}";                
                Console.WriteLine($"Invitation: {invitationUrl}");
                Console.Write("Create invitation completed.");
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
            
            Console.ReadLine();
        }

        public async Task FetchInbox()
        {            
            try
            {
                var (processCount, inboxItemMessage) = await edgeClientService.FetchInboxAsync(agentContext);
                foreach (var item in inboxItemMessage)
                {
                    Console.WriteLine($"Message[{item.Id}] = {item.Data}");
                }
                Console.Write("Fetch inbox completed.");
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
            
            Console.ReadLine();
        }

        public async Task AcceptInvitation()
        {            
            Console.Write("Invitation = ");
            var invitationUrl = Console.ReadLine();
            try
            {
                var invitation = MessageUtils.DecodeMessageFromUrlFormat<ConnectionInvitationMessage>(invitationUrl);
                var (request, record) = await connectionService.CreateRequestAsync(agentContext, invitation);
                await messageService.SendAsync(agentContext.Wallet, request, record);
                Console.Write("Accept invitation completed.");
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
            
            Console.ReadLine();
        }

        public async Task RevokeInvitation()
        {              
            Console.Write("Invitation ID = ");
            var invitationId = Console.ReadLine();
            try
            {                   
                await connectionService.RevokeInvitationAsync(agentContext, invitationId);                         
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
            
            Console.Write("Revoke invitation completed.");  
            Console.ReadLine();
        }

        public async Task RegisterDevice()
        {  
            Console.Write("Device ID = ");
            var deviceId = Console.ReadLine();
            Console.Write("Device Vendor = ");
            var deviceVendor = Console.ReadLine();
            try
            {
                var deviceInfo = new AddDeviceInfoMessage {
                    DeviceId = deviceId,
                    DeviceVendor = deviceVendor
                };
                await edgeClientService.AddDeviceAsync(agentContext, deviceInfo);
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
            
            Console.Write("Press enter to continue.");  
            Console.ReadLine();
        }

        public async Task Backup()
        {   
            try
            {      
                var seed = Guid.NewGuid().ToString().Replace("-", "");
                await edgeClientService.CreateBackupAsync(agentContext, seed);
                Console.WriteLine($"Seed = {seed}");
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
            
            Console.Write("Backup completed.");  
            Console.ReadLine();
        }

        public async Task Restore()
        {   
            Console.Write("Seed = ");           
            var seed = Console.ReadLine();
            try
            {
                await edgeClientService.RestoreFromBackupAsync(agentContext, seed);                      
                agentContext = await Host.Services.GetRequiredService<IAgentProvider>().GetContextAsync();
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
            
            Console.Write("Restore completed.");  
            Console.ReadLine();
        }
        
        private IHostBuilder CreateHostBuilder(string walletId, string agentName, string imageUrl, string endpointUri) =>
            Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddAriesFramework(builder =>
                    {
                        builder.RegisterEdgeAgent(options =>
                        {
                            var storageConfig = new WalletStorageConfiguration();
                            storageConfig.Path = Path.Combine(Environment.CurrentDirectory, "wallet");

                            options.AgentName = agentName;
                            // options.AgentImageUri = imageUrl;
                            options.EndpointUri = endpointUri;
                            options.WalletConfiguration = new WalletConfiguration { Id = walletId, StorageConfiguration = storageConfig };
                        });
                    });
                });
    }
}