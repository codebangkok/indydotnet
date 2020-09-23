using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Features.DidExchange;
using Hyperledger.Aries.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace CloudAgent.Pages.Connections
{
    public class ViewInvitationModel : PageModel
    {        
        private readonly ILogger<IndexModel> _logger;
        
        private readonly IConnectionService _connectionService;
        private readonly IAgentProvider _agentContextProvider;
        private readonly IMessageService _messageService;      

        [Required]
        [BindProperty]
        public string InvitationDetails { get; set; }

        public ConnectionInvitationMessage InvitationMessage { get; set; }

        public ViewInvitationModel(ILogger<IndexModel> logger,            
            IConnectionService connectionService,             
            IAgentProvider agentContextProvider,
            IMessageService messageService)
        {
            _logger = logger;
            _connectionService = connectionService;
            _agentContextProvider = agentContextProvider;
            _messageService = messageService;
        }
        
        public void OnGet()
        {
        }

        public void OnPost(string invitationDetails)
        {
            this.InvitationDetails = invitationDetails;
            InvitationMessage = MessageUtils.DecodeMessageFromUrlFormat<ConnectionInvitationMessage>(InvitationDetails);
        }

        public async Task<IActionResult> OnPostAcceptInvitationAsync()
        {
            var context = await _agentContextProvider.GetContextAsync();            

            var invite = MessageUtils.DecodeMessageFromUrlFormat<ConnectionInvitationMessage>(InvitationDetails);
            var (request, record) = await _connectionService.CreateRequestAsync(context, invite);
                                
            await _messageService.SendAsync(context.Wallet, request, record);

            return RedirectToPage("Index");
        }
    }
}

