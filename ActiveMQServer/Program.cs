using System;
using ActiveMQCommon;
using Apache.NMS;
using Apache.NMS.Util;
using Newtonsoft.Json;

namespace ActiveMQServer
{
    /// <summary>
    /// https://activemq.apache.org/components/nms/examples/nms-simple-synchronous-consumer-example
    /// </summary>
    internal class Program
    {
        private static void Main()
        {

            Console.Title = "Server";

            var uri = new Uri("activemq:tcp://IT29:61616");
            var factory = new NMSConnectionFactory(uri);

            using var connection = factory.CreateConnection();
            using var session = connection.CreateSession();
            var destination = SessionUtil.GetDestination(session, "queue://lot-bid");
            using var producer = session.CreateProducer(destination);
            var timeout = TimeSpan.FromSeconds(10);

            connection.Start();
            producer.DeliveryMode = MsgDeliveryMode.Persistent;
            producer.RequestTimeout = timeout;

            while (true)
            {
                try
                {
                    var id = new Random().Next(1, 100000);
                    var message = JsonConvert.SerializeObject(new LotItem
                    {
                        Contract = id,
                        Date = DateTime.UtcNow,
                        Lot = id,
                        Price = 100m * id
                    });
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
