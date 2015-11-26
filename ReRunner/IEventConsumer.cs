namespace ReRunner
{
    public interface IEventConsumer<TEventModel> 
    {
        void Consume(TEventModel eventArgs);
    }
}