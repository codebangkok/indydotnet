using System;
using Hyperledger.Aries.Agents;
using CloudAgent.Messages;
using CloudAgent.Protocols.BasicMessage;

namespace CloudAgent
{
    public class SimpleWebAgent : AgentBase
    {
        public SimpleWebAgent(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        protected override void ConfigureHandlers()
        {
            AddConnectionHandler();
            AddForwardHandler();
            AddHandler<BasicMessageHandler>();
            AddHandler<TrustPingMessageHandler>();
            AddDiscoveryHandler();
            AddTrustPingHandler();
            AddCredentialHandler();
            AddProofHandler();
        }
    }
}