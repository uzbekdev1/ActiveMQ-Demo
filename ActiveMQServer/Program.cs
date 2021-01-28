using System;
using System.Threading;
using ActiveMQCommon;
using Apache.NMS;
using Apache.NMS.Util;
using Newtonsoft.Json;

namespace ActiveMQServer
{
    /// <summary>
    /// https://activemq.apache.org/components/nms/examples/nms-simple-asynchronous-consumer-example
    /// </summary>
    internal class Program
    {

        private static readonly AutoResetEvent _waiter = new AutoResetEvent(false);

        private static void Main()
        {

            Console.Title = "Server";

            var uri = new Uri("activemq:tcp://localhost:61616");
            var factory = new NMSConnectionFactory(uri);

            using var connection = factory.CreateConnection();
            using var session = connection.CreateSession();
            var destination = SessionUtil.GetDestination(session, "queue://lot-bid");
            using var producer = session.CreateProducer(destination);
            using var consumer = session.CreateConsumer(destination);

            connection.Start();
            producer.DeliveryMode = MsgDeliveryMode.Persistent;

            consumer.Listener += OnMessage;

            _waiter.WaitOne();

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
