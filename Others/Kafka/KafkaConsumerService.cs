using CM.Shared.Kernel.Others.Kafka;
using System.Threading;
using System.Threading.Tasks;

namespace CM.Shared.Web.Others.Kafka
{
    public class KafkaConsumerService : BackgroundService
    {
        private readonly IKafkaConsumer KafkaConsumer;

        public KafkaConsumerService(IKafkaConsumer kafkaConsumer)
        {
            KafkaConsumer = kafkaConsumer;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await KafkaConsumer.Listen(stoppingToken);
        }
    }
}
