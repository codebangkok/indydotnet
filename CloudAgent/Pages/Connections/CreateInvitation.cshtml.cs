using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Configuration;
using Hyperledger.Aries.Features.DidExchange;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Hyperledger.Aries.Extensions;

namespace CloudAgent.Pages.Connections
{
    public class CreateInvitationModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        private readonly IConnectionService _connectionService;
        private readonly IProvisioningService _provisioningService;
        private readonly IAgentProvider _agentContextProvider;     

        public string Invitation { get; set; }
        public string QRCode { get; set; }

        public CreateInvitationModel(ILogger<IndexModel> logger,
            IConnectionService connectionService, 
            IProvisioningService provisioningService,
            IAgentProvider agentContextProvider)
        {
            _logger = logger;
            _connectionService = connectionService;
            _provisioningService = provisioningService;
            _agentContextProvider = agentContextProvider;
        }

        public async Task OnGetAsync()
        {
            var context = await _agentContextProvider.GetContextAsync();
            var provisioning = await _provisioningService.GetProvisioningAsync(context.Wallet);

            var config = new InviteConfiguration { 
                AutoAcceptConnection = true,
                MyAlias = new ConnectionAlias { Name = provisioning.Owner.Name, ImageUrl = provisioning.Owner.ImageUrl }
            };
            var (invitation, connectionRecord) = await _connectionService.CreateInvitationAsync(context, config);            
            Invitation = $"{provisioning.Endpoint.Uri}?c_i={invitation.ToJson().ToBase64()}";            

            QRCode = $"https://chart.googleapis.com/chart?cht=qr&chs=300x300&chld=L|0&chl={Uri.EscapeDataString(Invitation)}";
        }
    }
}
