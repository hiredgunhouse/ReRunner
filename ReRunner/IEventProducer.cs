namespace ReRunner
{
    public interface IEventProducer<TEventModel>
    {
        void Start(IEventConsumer<TEventModel> eventConsumer);
    }
}