using Microsoft.Extensions.DependencyInjection;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using EvoluxIoT.SynapseLink;

namespace EvoluxIoT.Web.Services
{
    public class MqttClientService : IHostedService
    {
        
        MqttFactory __mqtt_factory = new MqttFactory();

        IManagedMqttClient __mqtt_client = null;

        ManagedMqttClientOptions __mqtt_client_options_builder = null;

        public bool SetupComplete { get { return __mqtt_client != null; } }

        public void SetupClient()
        {

            __mqtt_client = __mqtt_factory.CreateManagedMqttClient();

            
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer("mqtt.evoluxiot.pt", 8883)
                .WithTls()
                .WithCredentials("evoluxiot", "evoluxiot")
                .WithClientId("webservice")
            .Build();

            __mqtt_client_options_builder = new ManagedMqttClientOptionsBuilder()
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                .WithClientOptions(options)
            .Build();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (!SetupComplete)
            {
                SetupClient();
            }
            await __mqtt_client.StartAsync(__mqtt_client_options_builder);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await __mqtt_client.StopAsync();
        }

        public async Task PublishAsync(string topic, string payload, MQTTnet.Protocol.MqttQualityOfServiceLevel quality_of_service = MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce, bool retain = false)
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(quality_of_service)
                .WithRetainFlag(retain)
            .Build();

            await __mqtt_client.EnqueueAsync(message);
        }

        public async Task SubscribeAsync(string topic, MQTTnet.Protocol.MqttQualityOfServiceLevel quality_of_service = MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce)
        {
            await __mqtt_client.SubscribeAsync(topic, quality_of_service);
        }

        public async Task UnsubscribeAsync(string topic)
        {
            await __mqtt_client.UnsubscribeAsync(topic);
        }

        public async Task<int> MaxVersion(string synapse_id)
        {
            return await SynapseLinkCommands.MaxVersion(__mqtt_client, synapse_id);
        }

        public async Task<bool> Heartbeat(string synapse_id)
        {
            return await SynapseLinkCommands.Heartbeat(__mqtt_client, synapse_id);
        }

        public async Task<bool> Reboot(string synapse_id)
        {
            return await SynapseLinkCommands.Reboot(__mqtt_client, synapse_id);
        }





    }
}
