using System;
using CloudAgent.Messages;
using Hyperledger.Aries.Agents;
using System.Text.Json.Serialization;

namespace CloudAgent.Protocols.BasicMessage
{
    public class BasicMessage : AgentMessage
    {
        public BasicMessage()
        {
            Id = Guid.NewGuid().ToString();
            Type = CustomMessageTypes.BasicMessageType;
        }
        
        [JsonPropertyName("content")]
        public string Content { get; set; }

        [JsonPropertyName("sent_time")]
        public string SentTime { get; set; }
    }
}