namespace Chase.Nexus.Reference.Store
{
    public sealed class NexusItem<T>
    {
        public DateTime ReceiptTime { get; }

        public T Data { get; }

        public NexusItem(DateTime receiptTime, T data)
        {
            this.ReceiptTime = receiptTime;
            this.Data = data;
        }

        public NexusItem(T data) : this(DateTime.UtcNow, data)
        { }
    }
}
