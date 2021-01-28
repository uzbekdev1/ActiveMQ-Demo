using System;
using ActiveMQCommon;
using Apache.NMS;
using Apache.NMS.Util;
using Newtonsoft.Json;

namespace ActiveMQClient
{
    /// <summary>
    /// https://activemq.apache.org/components/nms/examples/nms-simple-synchronous-consumer-example
    /// </summary>
    internal class Program
    {
        private static void Main()
        {

            Console.Title = "Client";

            var uri = new Uri("activemq:tcp://localhost:61616");
            var factory = new NMSConnectionFactory(uri);

            using var connection = factory.CreateConnection();
            using var session = connection.CreateSession();
            var destination = SessionUtil.GetDestination(session, "queue://lot-bid");
            using var producer = session.CreateProducer(destination);

            connection.Start();
            producer.DeliveryMode = MsgDeliveryMode.Persistent;

            while (true)
            {
                try
                {
                    var counter = new Random().Next(1, 100000);
                    var item = new LotItem
                    {
                        Contract = counter,
                        Date = DateTime.UtcNow,
                        Lot = counter,
                        Price = 100m * counter,
                        User = $"{Guid.NewGuid()}"
                    };
                    var message = JsonConvert.SerializeObject(item);
                    var request = session.CreateTextMessage(message);

                    producer.Send(request);

                    Console.WriteLine(message);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }
            }
        }
    }
}