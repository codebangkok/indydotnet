using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CloudAgent.Protocols.BasicMessage;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Configuration;
using Hyperledger.Aries.Features.DidExchange;
using Hyperledger.Aries.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CloudAgent.Pages.Connections
{
    public class DetailModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        private readonly IConnectionService _connectionService;
        private readonly IWalletService _walletService;
        private readonly IWalletRecordService _recordService;
        private readonly IMessageService _messageService;
        private readonly AgentOptions _walletOptions;
        
        public ConnectionRecord Connection { get; set; }
        public IEnumerable<BasicMessageRecord> Messages { get; set; }
        public bool? TrustPingSuccess { get; set; }

        public DetailModel(ILogger<IndexModel> logger,
            IConnectionService connectionService, 
            IWalletService walletService, 
            IWalletRecordService recordService,
            IMessageService messageService,
            IOptions<AgentOptions> walletOptions)
        {
            _logger = logger;
            _connectionService = connectionService;
            _walletService = walletService;
            _recordService = recordService;
            _messageService = messageService;
            _walletOptions = walletOptions.Value;            
        }

        public async Task OnGetAsync(string id, bool? trustPingSuccess = null)
        {
            var context = new DefaultAgentContext
            {
                Wallet = await _walletService.GetWalletAsync(_walletOptions.WalletConfiguration, _walletOptions.WalletCredentials)
            };

            Connection = await _connectionService.GetAsync(context, id);
            Messages = await _recordService.SearchAsync<BasicMessageRecord>(context.Wallet, SearchQuery.Equal(nameof(BasicMessageRecord.ConnectionId), id), null, 10);
            TrustPingSuccess = trustPingSuccess;
        }

        public async Task OnPostSendMessageAsync(string connectionId, string text)
        {
            var context = new DefaultAgentContext
            {
                Wallet = await _walletService.GetWalletAsync(_walletOptions.WalletConfiguration, _walletOptions.WalletCredentials)
            };

            var sentTime = DateTime.UtcNow;
            var messageRecord = new BasicMessageRecord
            {
                Id = Guid.NewGuid().ToString(),
                Direction = MessageDirection.Outgoing,
                Text = text,
                SentTime = sentTime,
                ConnectionId = connectionId
            };
            var message = new BasicMessage
            {
                Content = text,
                SentTime = sentTime.ToString("s", CultureInfo.InvariantCulture)
            };
            var connection = await _connectionService.GetAsync(context, connectionId);

            // Save the outgoing message to the local wallet for chat history purposes
            await _recordService.AddAsync(context.Wallet, messageRecord);

            // Send an agent message using the secure connection
            await _messageService.SendAsync(context.Wallet, message, connection);
        }
    }
}
