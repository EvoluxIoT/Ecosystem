using Microsoft.Extensions.DependencyInjection;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using EvoluxIoT.SynapseLink;
using System.Text;
using Microsoft.AspNetCore.SignalR;
using EvoluxIoT.Web.Hubs;


namespace EvoluxIoT.Web.Services
{
    public class MqttClientService : IHostedService
    {
        
        MqttFactory __mqtt_factory = new MqttFactory();

        IManagedMqttClient __mqtt_client = null;

        ManagedMqttClientOptions __mqtt_client_options_builder = null;

        Dictionary<string, string> __keyboard_subscription_by_user = new Dictionary<string, string>();


        public Dictionary<string, (string, string, DateTime)> codes = new Dictionary<string, (string, string, DateTime)>();

        public bool SetupComplete { get { return __mqtt_client != null; } }

        Task keyboard_handler(MqttApplicationMessageReceivedEventArgs payload)
        {
            var message = Encoding.UTF8.GetString(payload.ApplicationMessage.PayloadSegment);

            var (command, parameters, event_id) = SynapseLinkCommands.ParseCommand(message);

            var device_id = payload.ApplicationMessage.Topic.Split("/")[0];

            if (command == SynapseLinkCommands.COMMAND_KEYBOARDREAD && payload.ApplicationMessage.Topic.EndsWith("/keyboard") && IsSubscribedKeyboard(device_id))
            {
                var keys = new List<bool>();
                foreach (var key in parameters)
                {
                    keys.Add(Convert.ToBoolean(key));



                }

                _hubContext.Clients.Group(__keyboard_subscription_by_user[device_id]).SendAsync("KeyboardUpdate", device_id, keys);
                

            }
            return Task.CompletedTask;


        }

        private readonly IHubContext<SynapseHub> _hubContext;

        public MqttClientService(IHubContext<SynapseHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public void SetupClient()
        {

            __mqtt_client = __mqtt_factory.CreateManagedMqttClient();

//__mqtt_client.ApplicationMessageReceivedAsync += keyboard_handler;


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

        public async Task UnsubscribeKeyboard(string synapse_id)
        {
            if (IsSubscribedKeyboard(synapse_id))
            {
                __keyboard_subscription_by_user.Remove(synapse_id);
                await __mqtt_client.UnsubscribeAsync($"{synapse_id}/keyboard");
            }
        }

        public bool IsSubscribedKeyboard(string synapse_id)
        {
            return __keyboard_subscription_by_user.ContainsKey(synapse_id);
        }

        public async Task SubscribeKeyboard(string user, string synapse_id)
        {
            if (!IsSubscribedKeyboard(synapse_id))
            {
                __keyboard_subscription_by_user.Add(synapse_id, user);
                await __mqtt_client.SubscribeAsync($"{synapse_id}/keyboard");
            }
        }

        public async Task<int?> MaxVersion(string synapse_id)
        {
            return await SynapseLinkCommands.MaxVersion(__mqtt_client, synapse_id);
        }

        public async Task<bool> Heartbeat(string synapse_id)
        {
            return await SynapseLinkCommands.Heartbeat(__mqtt_client, synapse_id);
        }

        public async Task<bool?> Reboot(string synapse_id)
        {
            return await SynapseLinkCommands.Reboot(__mqtt_client, synapse_id);
        }

        public async Task<bool?> DigitalRead(string synapse_id, int pin)
        {
            return await SynapseLinkCommands.DigitalRead(__mqtt_client, synapse_id, pin);
        }

        public async Task<bool?> DigitalWrite(string synapse_id, int pin, bool value)
        {
            return await SynapseLinkCommands.DigitalWrite(__mqtt_client, synapse_id, pin, value);
        }

        public async Task<int?> AnalogRead(string synapse_id, int pin)
        {
            return await SynapseLinkCommands.AnalogRead(__mqtt_client, synapse_id, pin);
        }

        public async Task<string?> DisplayRead(string synapse_id)
        {
            return await SynapseLinkCommands.DisplayRead(__mqtt_client, synapse_id);
        }

        public async Task<string?> DisplayWrite(string synapse_id, string text)
        {
            return await SynapseLinkCommands.DisplayWrite(__mqtt_client, synapse_id, text);
        }

    }
}
