using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudAgent.Models;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Configuration;
using Hyperledger.Aries.Contracts;
using Hyperledger.Aries.Features.DidExchange;
using Hyperledger.Aries.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Hyperledger.Aries.Extensions;
using System.Text.Json;
using System.Text;

namespace CloudAgent.Pages.Connections
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        private readonly IEventAggregator _eventAggregator;
        private readonly IConnectionService _connectionService;
        private readonly IWalletService _walletService;
        private readonly IWalletRecordService _recordService;
        private readonly IProvisioningService _provisioningService;
        private readonly IAgentProvider _agentContextProvider;
        private readonly IMessageService _messageService;
        private readonly AgentOptions _walletOptions;

        public IEnumerable<ConnectionRecord> Connections { get; set; } 

        public IndexModel(ILogger<IndexModel> logger,
            IEventAggregator eventAggregator,
            IConnectionService connectionService, 
            IWalletService walletService, 
            IWalletRecordService recordService,
            IProvisioningService provisioningService,
            IAgentProvider agentContextProvider,
            IMessageService messageService,
            IOptions<AgentOptions> walletOptions)
        {
            _logger = logger;
            _eventAggregator = eventAggregator;
            _connectionService = connectionService;
            _walletService = walletService;
            _recordService = recordService;
            _provisioningService = provisioningService;
            _agentContextProvider = agentContextProvider;
            _messageService = messageService;
            _walletOptions = walletOptions.Value;            
        }
        
        public async Task OnGetAsync()
        {
            var context = await _agentContextProvider.GetContextAsync();
            Connections = await _connectionService.ListAsync(context);                
        }

        public ConnectionInvitationMessage DecodeInvitation(string invitation)
        {            
            return JsonSerializer.Deserialize<ConnectionInvitationMessage>(Encoding.UTF8.GetString(Convert.FromBase64String(invitation)));
        }

        public async Task OnPostRemoveAsync(string connectionId)
        {
            var context = await _agentContextProvider.GetContextAsync();
            await _connectionService.DeleteAsync(context, connectionId);
            Connections = await _connectionService.ListAsync(context);  
        }

    }
}
