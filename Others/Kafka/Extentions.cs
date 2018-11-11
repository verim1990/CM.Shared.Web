using CM.Shared.Kernel.Others.Kafka;
using CM.Shared.Web.Others.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CM.Shared.Web.Others.Redis
{
    public static class ExtentionsForKafka
    {
        public static IServiceCollection IncludeKafka(this IServiceCollection services, KafkaSettings kafkaSettings, KafkaServiceSettings kafkaServiceSettings)
        {
            return services
                .AddScoped(provider => kafkaSettings)
                .AddScoped(provider => kafkaServiceSettings.Producer)
                .AddScoped(provider => kafkaServiceSettings.Consumer)
                .AddSingleton<IKafkaProducer, KafkaProducer>()
                .AddSingleton<IKafkaConsumer, KafkaConsumer>()
                .AddHostedService<KafkaConsumerService>();
        }
    }
}
