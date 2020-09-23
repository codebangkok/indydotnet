using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Hyperledger.Aries.Storage;
using CloudAgent.Utils;
using System.IO;
using CloudAgent.Protocols.BasicMessage;
using CloudAgent.Messages;
using Jdenticon.AspNetCore;
using static Hyperledger.Aries.Storage.WalletConfiguration;
using Hyperledger.Aries.Features.DidExchange;
using Microsoft.AspNetCore.DataProtection;

namespace CloudAgent
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();

            // Register agent framework dependency services and handlers            
            var agentName = Environment.GetEnvironmentVariable("AGENT_NAME") ?? Configuration["AGENT_NAME"] ?? "Krung Thai Bank";
            var walletId = Environment.GetEnvironmentVariable("WALLET_ID") ?? Configuration["WALLET_ID"] ?? "ktb";
            var walletKey = Environment.GetEnvironmentVariable("WALLET_KEY") ?? Configuration["WALLET_KEY"] ?? "MyWalletKey";            
            var endpointUrl = Environment.GetEnvironmentVariable("ENDPOINT_HOST") ?? Configuration["ENDPOINT_HOST"] ?? "http://host.docker.internal:5000";
            // var genesisUrl = Environment.GetEnvironmentVariable("GENESIS_URL") ?? Configuration["GENESIS_URL"] ?? Path.GetFullPath("pool_genesis_local.txn");
            var genesisUrl = Environment.GetEnvironmentVariable("GENESIS_URL") ?? Configuration["GENESIS_URL"] ?? Path.GetFullPath("pool_genesis.txn");            
            var poolName = Environment.GetEnvironmentVariable("POOL_NAME") ?? Configuration["POOL_NAME"] ?? "KTBPool";
            
            var args = Environment.GetCommandLineArgs();
            if (args.Count() >= 4)
            {
                agentName = args[1];
                walletId = args[2];
                endpointUrl = args[3];
            }
            var imageUrl = Environment.GetEnvironmentVariable("IMAGE_URL") ?? Configuration["IMAGE_URL"] ?? $"{endpointUrl}/images/{walletId}.png";            

            var storageConfig = new WalletStorageConfiguration();
            storageConfig.Path = Path.Combine(Environment.CurrentDirectory, "wallet");
            
            services.AddAriesFramework(builder => {
                builder.RegisterAgent<SimpleWebAgent>(c => {
                    c.AgentName = agentName; 
                    // c.AgentImageUri = imageUrl;
                    c.EndpointUri = endpointUrl; 
                    c.WalletConfiguration = new WalletConfiguration { Id = walletId, StorageConfiguration = storageConfig };
                    c.WalletCredentials = new WalletCredentials { Key = walletKey };
                    c.GenesisFilename = genesisUrl;
                    c.PoolName = poolName;
                });
            });

            services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(Environment.CurrentDirectory ,"key")));

            // Register custom handlers with DI pipeline
            services.AddSingleton<BasicMessageHandler>();
            services.AddSingleton<TrustPingMessageHandler>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseAriesFramework();
            app.UseJdenticon();

            app.UseRouting();

            app.UseAuthorization();            

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
