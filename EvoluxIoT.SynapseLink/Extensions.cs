using MQTTnet.Extensions.ManagedClient;
using MQTTnet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvoluxIoT.SynapseLink
{
    internal static class Extensions
    {
        internal static async Task PublishAsync(this IManagedMqttClient mqtt, string topic, string payload, MQTTnet.Protocol.MqttQualityOfServiceLevel quality_of_service = MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce, bool retain = false)
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(quality_of_service)
                .WithRetainFlag(retain)
            .Build();

            await mqtt.EnqueueAsync(message);
        }
    }
}
