using System;
using System.Threading;
using ActiveMQCommon;
using Apache.NMS;
using Apache.NMS.Util;
using Newtonsoft.Json;

namespace ActiveMQClient
{
    /// <summary>
    /// https://activemq.apache.org/components/nms/examples/nms-simple-asynchronous-consumer-example
    /// </summary>
    internal class Program
    {

        private static AutoResetEvent semaphore = new AutoResetEvent(false);

        private static void Main()
        {
            Console.Title = "Client";

            var uri = new Uri("activemq:tcp://IT29:61616");
            var factory = new NMSConnectionFactory(uri);

            using var connection = factory.CreateConnection();
            using var session = connection.CreateSession();
            var destination = SessionUtil.GetDestination(session, "queue://lot-bid");
            using var producer = session.CreateProducer(destination);
            var timeout = TimeSpan.FromSeconds(10);
            using var consumer = session.CreateConsumer(destination);

            connection.Start();
            producer.DeliveryMode = MsgDeliveryMode.Persistent;
            producer.RequestTimeout = timeout;

            consumer.Listener += OnMessage;

            semaphore.WaitOne();

            Console.WriteLine("Done!");
        }

        protected static void OnMessage(IMessage receivedMsg)
        {
            if (receivedMsg is ITextMessage message)
            {
                var item = JsonConvert.DeserializeObject<LotItem>(message.Text);
                var timeNow = DateTime.UtcNow;

                Console.WriteLine(timeNow.Subtract(item.Date).ToString("g"));
            }
        }

    }
}
