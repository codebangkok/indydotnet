using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hyperledger.Aries.Configuration;
using Hyperledger.Aries.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Hyperledger.Aries.Features.DidExchange;

namespace CloudAgent.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        private readonly IWalletService _walletService;
        private readonly IProvisioningService _provisioningService;
        private readonly AgentOptions _walletOptions;

        public ProvisioningRecord Provisioning { get; set; }
        public string ImageUrl { get; set; }

        public IndexModel(ILogger<IndexModel> logger,
            IWalletService walletService,
            IProvisioningService provisioningService,
            IOptions<AgentOptions> walletOptions)
        {
            _logger = logger;
            _walletService = walletService;
            _provisioningService = provisioningService;
            _walletOptions = walletOptions.Value;                        
        }

        public async Task OnGetAsync()
        {
            var wallet = await _walletService.GetWalletAsync(
                _walletOptions.WalletConfiguration,
                _walletOptions.WalletCredentials);

            Provisioning = await _provisioningService.GetProvisioningAsync(wallet);            
        }        
    }
}
