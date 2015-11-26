using System.Reflection;

using log4net;

namespace ReRunner
{
    public class Host<TEventModel>
    {
        private static ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.FullName);

        public Host(
            IEventProducer<TEventModel> eventProducer,
            IEventConsumer<TEventModel> eventConsumer)
        {
            this.EventProducer = eventProducer;
            this.EventConsumer = eventConsumer;
        }

        public IEventProducer<TEventModel> EventProducer { get; protected set; }
        public IEventConsumer<TEventModel> EventConsumer { get; protected set; }

        public void Run()
        {
            log.Info("Host is running...");
            this.EventProducer.Start(this.EventConsumer);
        }
    }
}