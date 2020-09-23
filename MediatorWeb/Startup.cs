using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Hyperledger.Aries.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using static Hyperledger.Aries.Storage.WalletConfiguration;

namespace MediatorWeb
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var walletId = Environment.GetEnvironmentVariable("WALLET_ID") ?? Configuration["WALLET_ID"] ?? "mediator";
            var walletKey = Environment.GetEnvironmentVariable("WALLET_KEY") ?? Configuration["WALLET_KEY"] ?? "MyWalletKey";
            //var endpointUrl = Environment.GetEnvironmentVariable("ENDPOINT_HOST") ?? Configuration["ENDPOINT_HOST"] ?? "http://mediator";
            var endpointUrl = Environment.GetEnvironmentVariable("ENDPOINT_HOST") ?? Configuration["ENDPOINT_HOST"] ?? "http://localhost:5004";
            var genesisUrl = Environment.GetEnvironmentVariable("GENESIS_URL") ?? Configuration["GENESIS_URL"] ?? Path.GetFullPath("pool_genesis.txn");

            var storageConfig = new WalletStorageConfiguration();
            storageConfig.Path = Path.Combine(Environment.CurrentDirectory, "wallet");

            services.AddAriesFramework(builder =>
            {
                builder.RegisterMediatorAgent(options =>
                {
                    // Agent endpoint. Use fully qualified endpoint.
                    options.EndpointUri = endpointUrl;
                    // The path to the genesis transaction file.
                    options.GenesisFilename = genesisUrl;
                    // The identifier of the wallet
                    options.WalletConfiguration = new WalletConfiguration { Id = walletId, StorageConfiguration = storageConfig };
                    // Secret key used to open the wallet.
                    options.WalletCredentials =  new WalletCredentials { Key = walletKey };
                });
            });

            services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(Environment.CurrentDirectory ,"key")));            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAriesFramework();
            app.UseMediatorDiscovery();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Mediator Started.");
                });
            });
        }
    }
}
