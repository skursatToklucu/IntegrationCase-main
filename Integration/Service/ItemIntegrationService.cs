
using Integration.Common;
using Integration.Backend;
using StackExchange.Redis;

namespace Integration.Service
{
    public sealed class ItemIntegrationService : IDisposable
    {
        private readonly ItemOperationBackend ItemIntegrationBackend;

        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _database;

        public ItemIntegrationService(string redisConnectionString)
        {
            ItemIntegrationBackend = new ItemOperationBackend();
            _redis = ConnectionMultiplexer.Connect(redisConnectionString);
            _database = _redis.GetDatabase();
        }

        public async Task<Result> SaveItemAsync(string itemContent)
        {
            string lockKey = $"lock:item:{itemContent}";
            TimeSpan lockTimeout = TimeSpan.FromSeconds(10);
            string lockValue = Guid.NewGuid().ToString();

            bool lockAcquired = await _database.LockTakeAsync(lockKey, lockValue, lockTimeout);
            if (!lockAcquired)
            {
                return new Result(false, $"Unable to acquire lock for content {itemContent}.");
            }

            try
            {
                if (ItemIntegrationBackend.FindItemsWithContent(itemContent).Count != 0)
                    return new Result(false, $"Duplicate item received with content {itemContent}.");


                var item = ItemIntegrationBackend.SaveItem(itemContent);
                return new Result(true, $"Item with content {itemContent} saved with id {item.Id}");
            }
            finally
            {
                await _database.LockReleaseAsync(lockKey, lockValue);
            }
        }

        public List<Item> GetAllItems()
        {
            return ItemIntegrationBackend.GetAllItems();
        }

        public void Dispose()
        {
            _redis?.Dispose();
        }
    }
}
