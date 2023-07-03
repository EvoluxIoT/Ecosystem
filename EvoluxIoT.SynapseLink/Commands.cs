using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace EvoluxIoT.SynapseLink
{
    public static class SynapseLinkCommands
    {
        // Command codes used by the Synapse Link protocol

        public const byte COMMAND_MAXVERSION = 0x00;
        public const byte COMMAND_HEARTBEAT = 0x01;
        public const byte COMMAND_ACKNOWLEDGE = 0x02;
        public const byte COMMAND_REBOOT = 0x03;
        public const byte COMMAND_DIGITALREAD = 0x04;
        public const byte COMMAND_DIGITALWRITE = 0x05;
        public const byte COMMAND_ANALOGREAD = 0x06;
        public const byte COMMAND_ANALOGWRITE = 0x07;
        public const byte COMMAND_PWMREAD = 0x08;
        public const byte COMMAND_PWMWRITE = 0x09;
        public const byte COMMAND_DISPLAYWRITE = 0x0A;

        public const int RESPONSE_TIMEOUT = 5000;

        public static (string, List<dynamic>, int) ParseMessage(string message)
        {
            if (!message.StartsWith("!"))
            {
                return (null, null, -1);
            }
            var command = message.Split(":,:");

            // Command Name
            var command_name = command[0];

            // Event Number
            var event_number = command[command.Length - 1];

            // Parameters
            var parameters = new List<dynamic>();
            for (int i = 1; i < command.Length - 1; i++)
            {
                parameters.Add(command[i]);
            }

            return (command_name, parameters, int.Parse(event_number));
        }

        

        // Methods for evoking commands

        public async static Task<bool> Reboot(IManagedMqttClient mqtt, string synapse_id)
        {
            AutoResetEvent AcknoledgeReceived = new AutoResetEvent(false);
            AutoResetEvent RebootReceived = new AutoResetEvent(false);



            mqtt.ApplicationMessageReceivedAsync += (e) =>
            {
                var message = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);
                var (command_name, parameters, event_number) = ParseMessage(message);

                if (command_name is null) return Task.CompletedTask;
                if (command_name == $"!{COMMAND_ACKNOWLEDGE}" && (string)parameters[0] == "reboot" && (string)parameters[1] == "[]")
                {
                    AcknoledgeReceived.Set();
                }
                else if (command_name == $"!{COMMAND_REBOOT}")
                {
                    RebootReceived.Set();
                }
                return Task.CompletedTask;
            };

            await mqtt.SubscribeAsync(synapse_id);
            await mqtt.PublishAsync(synapse_id, "reboot");


            AcknoledgeReceived.WaitOne(RESPONSE_TIMEOUT);

            return RebootReceived.WaitOne(RESPONSE_TIMEOUT);

        }

        public async static Task<bool> Heartbeat(IManagedMqttClient mqtt, string synapse_id)
        {

            AutoResetEvent AcknoledgeReceived = new AutoResetEvent(false);
            AutoResetEvent HeartbeatReceived = new AutoResetEvent(false);

            mqtt.ApplicationMessageReceivedAsync += (e) =>
            {
                var message = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);
                var (command_name, parameters, event_number) = ParseMessage(message);
                if (command_name is null) return Task.CompletedTask;
                if (command_name == $"!{COMMAND_ACKNOWLEDGE}" && (string)parameters[0] == "heartbeat" && (string)parameters[1] == "[]")
                {
                    AcknoledgeReceived.Set();
                }
                else if (command_name == $"!{COMMAND_HEARTBEAT}")
                {
                    HeartbeatReceived.Set();
                }
                return Task.CompletedTask;
            };

            await mqtt.SubscribeAsync(synapse_id);
            await mqtt.PublishAsync(synapse_id, "heartbeat");

            AcknoledgeReceived.WaitOne(RESPONSE_TIMEOUT);

            var state = HeartbeatReceived.WaitOne(RESPONSE_TIMEOUT);
            await mqtt.UnsubscribeAsync(synapse_id);
            return state;
        }

        public async static Task<int> MaxVersion(IManagedMqttClient mqtt, string synapse_id)
        {

            AutoResetEvent AcknoledgeReceived = new AutoResetEvent(false);
            AutoResetEvent MaxVersionReceived = new AutoResetEvent(false);

            int maxversion = -1;

            mqtt.ApplicationMessageReceivedAsync += (e) =>
            {
                var message = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);
                var (command_name, parameters, event_number) = ParseMessage(message);
                if (command_name is null) return Task.CompletedTask;
                if (command_name == $"!{COMMAND_ACKNOWLEDGE}" && (string)parameters[0] == "maxversion" && (string)parameters[1] == "[]")
                {
                    AcknoledgeReceived.Set();
                }
                else if (command_name == $"!{COMMAND_MAXVERSION}")
                {
                    maxversion = int.Parse(parameters[0]);
                    MaxVersionReceived.Set();
                }
                return Task.CompletedTask;
            };

            await mqtt.SubscribeAsync(synapse_id);
            await mqtt.PublishAsync(synapse_id, "maxversion");
            // MQTTnet will receive a ack message from the Synapse Link device and the response of max version on a second message
            AcknoledgeReceived.WaitOne(RESPONSE_TIMEOUT);
            MaxVersionReceived.WaitOne(RESPONSE_TIMEOUT);
            await mqtt.UnsubscribeAsync(synapse_id);

            return maxversion;
        }



    }
}
