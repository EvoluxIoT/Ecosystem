using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvoluxIoT.SynapseLink
{

    public class SynapseLinkCommands
    {


        // Command Codes
        public const int COMMAND_HELLO = 0x00;
        public const int COMMAND_GOODBYE = 0x01;
        public const int COMMAND_MAXVERSION = 0x02;
        public const int COMMAND_HEARTBEAT = 0x03;
        public const int COMMAND_ACKNOWLEDGE = 0x04;
        public const int COMMAND_REBOOT = 0x05;
        public const int COMMAND_DIGITALREAD = 0x06;
        public const int COMMAND_DIGITALWRITE = 0x07;
        public const int COMMAND_ANALOGREAD = 0x08;
        public const int COMMAND_DISPLAYREAD = 0x0A;
        public const int COMMAND_DISPLAYWRITE = 0x0B;
        public const int COMMAND_KEYBOARDREAD = 0x0C;

        // SynapseLink Options
        public const int RESPONSE_TIMEOUT = 5000;

        // Command Parsing
        public static (int, List<String>, int) ParseCommand(string message)
        {
            List<String> parameters = new List<String>();

            // Don't parse sent messages
            if (!message.StartsWith("!"))
            {
                return (-1, parameters, -1);
            }

            // Remove sent message indicator
            message = message.Substring(1);

            // Split message into command parts
            List<String> data = message.Split(":,:").ToList();

            // Command must have at least 2 parts (command and event_id)
            if (data.Count < 2)
            {
                return (-1, parameters, -1);
            }

            // Parameterless command
            else if (data.Count == 2)
            {
                return (Convert.ToInt32(data[0]), parameters, Convert.ToInt32(data[1]));
            }   
            // Command with parameters
            else
            {
                // Add parameters to list
                for (int i = 1; i < data.Count - 1; i++)
                {
                    parameters.Add(data[i]);
                }

                return (Convert.ToInt32(data[0]), parameters, Convert.ToInt32(data[data.Count - 1]));
            }
        }

        // Command Building
        public static string BuildCommand(int command, List<String> parameters, int event_id)
        {


            string message = command.ToString();

            // Add parameters to message
            foreach (string parameter in parameters)
            {
                message += ":,:" + parameter;
            }

            // Add event_id to message
            message += ":,:" + event_id.ToString();

            return message;
        }

        // Command Handling

        public static async Task<bool> WaitAcknoledgeFor(IManagedMqttClient mqtt, string synapse_id, (int, List<String>, int) command_check, int timeout_miliseconds = 5000)
        {
            // Acknoledge successfuly received?
            bool acknoledged = false;

            // Monitor timeout
            System.Timers.Timer timeout_monitor = new System.Timers.Timer(timeout_miliseconds);
            timeout_monitor.AutoReset = false;
            timeout_monitor.Elapsed += (sender, e) =>
            {
                timeout_monitor.Stop();
                acknoledged = false;
            };

            Task handler(MqttApplicationMessageReceivedEventArgs payload)
            {
                var message = Encoding.UTF8.GetString(payload.ApplicationMessage.PayloadSegment);

                var check = new List<String> { command_check.Item1.ToString() };
                check.AddRange(command_check.Item2);

                var (command, parameters, event_id) = ParseCommand(message);

                if (command == COMMAND_ACKNOWLEDGE && payload.ApplicationMessage.Topic == synapse_id && parameters == check)
                {
                    timeout_monitor.Stop();
                    acknoledged = true;
                }

                return Task.CompletedTask;
            }

            mqtt.ApplicationMessageReceivedAsync += handler;

            await mqtt.SubscribeAsync(synapse_id);
            timeout_monitor.Start();

            while (timeout_monitor.Enabled) { 
                await Task.Delay(100); 
            }
            mqtt.ApplicationMessageReceivedAsync -= handler;
            await mqtt.UnsubscribeAsync(synapse_id);

            return acknoledged;
        }

        // Commands

        public static async Task<int?> MaxVersion(IManagedMqttClient mqtt, string synapse_id, int timeout_miliseconds = 5000)
        {
            int? max_version = null;
            var acknoledged = false;

            System.Timers.Timer timeout_monitor = new System.Timers.Timer(timeout_miliseconds);
            timeout_monitor.AutoReset = false;
            timeout_monitor.Elapsed += (sender, e) =>
            {
                timeout_monitor.Stop();
            };

            (int, List<String>, int) send_command = (COMMAND_MAXVERSION, new List<String>(), 0);

            Task handler_2(MqttApplicationMessageReceivedEventArgs payload)
            {
                var message = Encoding.UTF8.GetString(payload.ApplicationMessage.PayloadSegment);

                var (command, parameters, event_id) = ParseCommand(message);

                if (command == COMMAND_ACKNOWLEDGE && payload.ApplicationMessage.Topic == synapse_id && parameters == new List<string> { send_command.Item1.ToString() })
                {
                    acknoledged = true;
                }
                if (command == COMMAND_MAXVERSION && payload.ApplicationMessage.Topic == synapse_id)
                {
                    max_version = Convert.ToInt32(parameters[0]);
                }

                return Task.CompletedTask;
            }

            mqtt.ApplicationMessageReceivedAsync += handler_2;

            await mqtt.SubscribeAsync(synapse_id);
            await mqtt.PublishAsync(synapse_id, BuildCommand(send_command.Item1, send_command.Item2, send_command.Item3));
            timeout_monitor.Start();

            while (timeout_monitor.Enabled)
            {
                await Task.Delay(100);
                if (acknoledged && max_version != null)
                {
                    timeout_monitor.Stop();
                }
            }
            await mqtt.UnsubscribeAsync(synapse_id);

            if (acknoledged)
            {

                return max_version;
            } else
            {
                return null;
            }
        }

        public static async Task<bool> Heartbeat(IManagedMqttClient mqtt, string synapse_id, int timeout_miliseconds = 5000)
        {
            bool heartbeat = false;
            var acknoledged = false;

            System.Timers.Timer timeout_monitor = new System.Timers.Timer(timeout_miliseconds);
            timeout_monitor.AutoReset = false;
            timeout_monitor.Elapsed += (sender, e) =>
            {
                timeout_monitor.Stop();
            };

            (int, List<String>, int) send_command = (COMMAND_HEARTBEAT, new List<String>(), 0);

            Task handler_2(MqttApplicationMessageReceivedEventArgs payload)
            {
                var message = Encoding.UTF8.GetString(payload.ApplicationMessage.PayloadSegment);

                var (command, parameters, event_id) = ParseCommand(message);

                if (command == COMMAND_ACKNOWLEDGE && payload.ApplicationMessage.Topic == synapse_id && parameters == new List<string> { send_command.Item1.ToString() })
                {
                    acknoledged = true;
                }
                if (command == COMMAND_HEARTBEAT && payload.ApplicationMessage.Topic == synapse_id)
                {
                    heartbeat = true;
                }

                return Task.CompletedTask;
            }

            mqtt.ApplicationMessageReceivedAsync += handler_2;

            await mqtt.SubscribeAsync(synapse_id);
            await mqtt.PublishAsync(synapse_id, BuildCommand(send_command.Item1, send_command.Item2, send_command.Item3));
            timeout_monitor.Start();

            while (timeout_monitor.Enabled)
            {
                await Task.Delay(100);
                if (acknoledged && heartbeat == true)
                {
                    timeout_monitor.Stop();
                }
            }
            await mqtt.UnsubscribeAsync(synapse_id);

            return heartbeat;
        }

        public static async Task<bool?> Reboot(IManagedMqttClient mqtt, string synapse_id, int timeout_miliseconds = 5000)
        {
            bool? reboot = null;
            var acknoledged = false;

            System.Timers.Timer timeout_monitor = new System.Timers.Timer(timeout_miliseconds);
            timeout_monitor.AutoReset = false;
            timeout_monitor.Elapsed += (sender, e) =>
            {
                timeout_monitor.Stop();
            };

            (int, List<String>, int) send_command = (COMMAND_REBOOT, new List<String>(), 0);

            Task handler_2(MqttApplicationMessageReceivedEventArgs payload)
            {
                var message = Encoding.UTF8.GetString(payload.ApplicationMessage.PayloadSegment);

                var (command, parameters, event_id) = ParseCommand(message);

                if (command == COMMAND_ACKNOWLEDGE && payload.ApplicationMessage.Topic == synapse_id && parameters == new List<string> { send_command.Item1.ToString() })
                {
                    acknoledged = true;
                }
                if (command == COMMAND_REBOOT && payload.ApplicationMessage.Topic == synapse_id)
                {
                    reboot = true;
                }

                return Task.CompletedTask;
            }

            mqtt.ApplicationMessageReceivedAsync += handler_2;

            await mqtt.SubscribeAsync(synapse_id);
            await mqtt.PublishAsync(synapse_id, BuildCommand(send_command.Item1, send_command.Item2, send_command.Item3));
            timeout_monitor.Start();

            while (timeout_monitor.Enabled)
            {
                await Task.Delay(100);
                if (acknoledged && reboot == true)
                {
                    timeout_monitor.Stop();
                }
            }
            await mqtt.UnsubscribeAsync(synapse_id);

            return reboot;
        }

        public static async Task<bool?> DigitalRead(IManagedMqttClient mqtt, string synapse_id, int port, int timeout_miliseconds = 5000)
        {
            bool? state = null;
            var acknoledged = false;

            System.Timers.Timer timeout_monitor = new System.Timers.Timer(timeout_miliseconds);
            timeout_monitor.AutoReset = false;
            timeout_monitor.Elapsed += (sender, e) =>
            {
                timeout_monitor.Stop();
            };

            (int, List<String>, int) send_command = (COMMAND_DIGITALREAD, new List<String> { port.ToString() }, 0);

            Task handler_2(MqttApplicationMessageReceivedEventArgs payload)
            {
                var message = Encoding.UTF8.GetString(payload.ApplicationMessage.PayloadSegment);

                var (command, parameters, event_id) = ParseCommand(message);

                if (command == COMMAND_ACKNOWLEDGE && payload.ApplicationMessage.Topic == synapse_id && parameters == new List<string> { send_command.Item1.ToString() })
                {
                    acknoledged = true;
                }
                if (command == COMMAND_DIGITALREAD && payload.ApplicationMessage.Topic == synapse_id)
                {
                    state = Convert.ToBoolean(Convert.ToInt32(parameters[0]));
                }

                return Task.CompletedTask;
            }

            mqtt.ApplicationMessageReceivedAsync += handler_2;

            await mqtt.SubscribeAsync(synapse_id);
            await mqtt.PublishAsync(synapse_id, BuildCommand(send_command.Item1, send_command.Item2, send_command.Item3));
            timeout_monitor.Start();

            while (timeout_monitor.Enabled)
            {
                await Task.Delay(100);
                if (acknoledged && state != null)
                {
                    timeout_monitor.Stop();
                }
            }
            await mqtt.UnsubscribeAsync(synapse_id);

            return state;

        }

        public static async Task<bool?> DigitalWrite(IManagedMqttClient mqtt, string synapse_id, int port, bool state, int timeout_miliseconds = 5000)
        {
            bool? success = null;
            var acknoledged = false;

            System.Timers.Timer timeout_monitor = new System.Timers.Timer(timeout_miliseconds);
            timeout_monitor.AutoReset = false;
            timeout_monitor.Elapsed += (sender, e) =>
            {
                timeout_monitor.Stop();
            };

            (int, List<String>, int) send_command = (COMMAND_DIGITALWRITE, new List<String> { port.ToString(), Convert.ToInt32(state).ToString() }, 0);

            Task handler_2(MqttApplicationMessageReceivedEventArgs payload)
            {
                var message = Encoding.UTF8.GetString(payload.ApplicationMessage.PayloadSegment);

                var (command, parameters, event_id) = ParseCommand(message);

                if (command == COMMAND_ACKNOWLEDGE && payload.ApplicationMessage.Topic == synapse_id && parameters == new List<string> { send_command.Item1.ToString() })
                {
                    acknoledged = true;
                }
                if (command == COMMAND_DIGITALWRITE && payload.ApplicationMessage.Topic == synapse_id)
                {
                    success = Convert.ToBoolean(Convert.ToInt32(parameters[0]));
                }

                return Task.CompletedTask;
            }

            mqtt.ApplicationMessageReceivedAsync += handler_2;

            await mqtt.SubscribeAsync(synapse_id);
            await mqtt.PublishAsync(synapse_id, BuildCommand(send_command.Item1, send_command.Item2, send_command.Item3));
            timeout_monitor.Start();

            while (timeout_monitor.Enabled)
            {
                await Task.Delay(100);
                if (acknoledged && success != null)
                {
                    timeout_monitor.Stop();
                }
            }
            await mqtt.UnsubscribeAsync(synapse_id);

            return success;
        }

        public static async Task<int?> AnalogRead(IManagedMqttClient mqtt, string synapse_id, int port, int timeout_miliseconds = 5000)
        {
            int? value = null;
            var acknoledged = false;

            System.Timers.Timer timeout_monitor = new System.Timers.Timer(timeout_miliseconds);
            timeout_monitor.AutoReset = false;
            timeout_monitor.Elapsed += (sender, e) =>
            {
                timeout_monitor.Stop();
            };

            (int, List<String>, int) send_command = (COMMAND_ANALOGREAD, new List<String> { port.ToString() }, 0);

            Task handler_2(MqttApplicationMessageReceivedEventArgs payload)
            {
                var message = Encoding.UTF8.GetString(payload.ApplicationMessage.PayloadSegment);

                var (command, parameters, event_id) = ParseCommand(message);

                if (command == COMMAND_ACKNOWLEDGE && payload.ApplicationMessage.Topic == synapse_id && parameters == new List<string> { send_command.Item1.ToString() })
                {
                    acknoledged = true;
                }
                if (command == COMMAND_ANALOGREAD && payload.ApplicationMessage.Topic == synapse_id)
                {
                    value = Convert.ToInt32(parameters[0]);
                }

                return Task.CompletedTask;
            }

            mqtt.ApplicationMessageReceivedAsync += handler_2;

            await mqtt.SubscribeAsync(synapse_id);
            await mqtt.PublishAsync(synapse_id, BuildCommand(send_command.Item1, send_command.Item2, send_command.Item3));
            timeout_monitor.Start();

            while (timeout_monitor.Enabled)
            {
                await Task.Delay(100);
                if (acknoledged && value != null)
                {
                    timeout_monitor.Stop();
                }
            }
            await mqtt.UnsubscribeAsync(synapse_id);

            return value;
        }

        public static async Task<string?> DisplayRead(IManagedMqttClient mqtt, string synapse_id, int timeout_miliseconds = 5000)
        {
            string? text_o = null;
            var acknoledged = false;

            System.Timers.Timer timeout_monitor = new System.Timers.Timer(timeout_miliseconds);
            timeout_monitor.AutoReset = false;

            timeout_monitor.Elapsed += (sender, e) =>
            {
                timeout_monitor.Stop();
            };

            (int, List<String>, int) send_command = (COMMAND_DISPLAYREAD, new List<String>(), 0);

            Task handler_2(MqttApplicationMessageReceivedEventArgs payload)
            {
                var message = Encoding.UTF8.GetString(payload.ApplicationMessage.PayloadSegment);

                var (command, parameters, event_id) = ParseCommand(message);

                if (command == COMMAND_ACKNOWLEDGE && payload.ApplicationMessage.Topic == synapse_id && parameters == new List<string> { send_command.Item1.ToString() })
                {
                    acknoledged = true;
                }
                if (command == COMMAND_DISPLAYREAD && payload.ApplicationMessage.Topic == synapse_id)
                {
                    text_o = parameters[0];
                }

                return Task.CompletedTask;
            }

            mqtt.ApplicationMessageReceivedAsync += handler_2;

            await mqtt.SubscribeAsync(synapse_id);
            await mqtt.PublishAsync(synapse_id, BuildCommand(send_command.Item1, send_command.Item2, send_command.Item3));
            timeout_monitor.Start();

            while (timeout_monitor.Enabled)
            {
                await Task.Delay(100);
                if (acknoledged && text_o != null)
                {
                    timeout_monitor.Stop();
                }
            }

            await mqtt.UnsubscribeAsync(synapse_id);

            return text_o;
        }


        public static async Task<string?> DisplayWrite(IManagedMqttClient mqtt, string synapse_id, string text, int timeout_miliseconds = 5000)
        {
            string? text_o = null;
            var acknoledged = false;

            System.Timers.Timer timeout_monitor = new System.Timers.Timer(timeout_miliseconds);
            timeout_monitor.AutoReset = false;
            timeout_monitor.Elapsed += (sender, e) =>
            {
                timeout_monitor.Stop();
            };

            (int, List<String>, int) send_command = (COMMAND_DISPLAYWRITE, new List<String> { text }, 0);

            Task handler_2(MqttApplicationMessageReceivedEventArgs payload)
            {
                var message = Encoding.UTF8.GetString(payload.ApplicationMessage.PayloadSegment);

                var (command, parameters, event_id) = ParseCommand(message);

                if (command == COMMAND_ACKNOWLEDGE && payload.ApplicationMessage.Topic == synapse_id && parameters == new List<string> { send_command.Item1.ToString() })
                {
                    acknoledged = true;
                }
                if (command == COMMAND_DISPLAYWRITE && payload.ApplicationMessage.Topic == synapse_id)
                {
                    text_o = parameters[0];
                }

                return Task.CompletedTask;
            }

            mqtt.ApplicationMessageReceivedAsync += handler_2;

            await mqtt.SubscribeAsync(synapse_id);
            await mqtt.PublishAsync(synapse_id, BuildCommand(send_command.Item1, send_command.Item2, send_command.Item3));
            timeout_monitor.Start();

            while (timeout_monitor.Enabled)
            {
                await Task.Delay(100);
                if (acknoledged && text_o != null)
                {
                    timeout_monitor.Stop();
                }
            }
            await mqtt.UnsubscribeAsync(synapse_id);

            return text_o;
        }

        public static async Task<List<bool>?> KeyboardRead(IManagedMqttClient mqtt, string synapse_id, int timeout_miliseconds = 5000)
        {
            List<bool>? keys = null;
            var acknoledged = false;

            System.Timers.Timer timeout_monitor = new System.Timers.Timer(timeout_miliseconds);
            timeout_monitor.AutoReset = false;
            timeout_monitor.Elapsed += (sender, e) =>
            {
                timeout_monitor.Stop();
            };

            (int, List<String>, int) send_command = (COMMAND_KEYBOARDREAD, new List<String>(), 0);

            Task handler_2(MqttApplicationMessageReceivedEventArgs payload)
            {
                var message = Encoding.UTF8.GetString(payload.ApplicationMessage.PayloadSegment);

                var (command, parameters, event_id) = ParseCommand(message);

                if (command == COMMAND_ACKNOWLEDGE && payload.ApplicationMessage.Topic == synapse_id && parameters == new List<string>())
                {
                    acknoledged = true;
                }
                if (command == COMMAND_KEYBOARDREAD && payload.ApplicationMessage.Topic == synapse_id)
                {
                    keys = new List<bool>();
                    foreach (var key in parameters)
                    {
                        keys.Add(Convert.ToBoolean(key));
                    }
                }

                return Task.CompletedTask;
            }

            mqtt.ApplicationMessageReceivedAsync += handler_2;

            await mqtt.SubscribeAsync(synapse_id);
            await mqtt.PublishAsync(synapse_id, BuildCommand(send_command.Item1, send_command.Item2, send_command.Item3));
            timeout_monitor.Start();

            while (timeout_monitor.Enabled)
            {
                await Task.Delay(100);
                if (acknoledged && keys != null)
                {
                    timeout_monitor.Stop();
                }
            }
            await mqtt.UnsubscribeAsync(synapse_id);

            return keys;
        }









    }
}
