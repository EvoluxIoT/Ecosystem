using MQTTnet.AspNetCore;
using MQTTnet.Diagnostics;
using MQTTnet.Server;

namespace EvoluxIoT.Web.Services
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMqttClient(this IServiceCollection services)
        {

            services.AddSingleton<MqttClientService>();
            services.AddHostedService<MqttClientService>(s => s.GetService<MqttClientService>());

            return services;
        }
    }
}
