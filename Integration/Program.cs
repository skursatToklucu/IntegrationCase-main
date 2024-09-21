using Integration.Service;

namespace Integration
{
    public abstract class Program
    {
        public static async Task Main(string[] args)
        {
            string redisConnectionString = "localhost:6379";
            using var service = new ItemIntegrationService(redisConnectionString);

            var tasks = new[]
            {
                Task.Run(async () =>
                {
                    var result = await service.SaveItemAsync("a");
                    Console.WriteLine(result.Message);
                }),
                Task.Run(async () =>
                {
                    var result = await service.SaveItemAsync("b");
                    Console.WriteLine(result.Message);
                }),
                Task.Run(async () =>
                {
                    var result = await service.SaveItemAsync("c");
                    Console.WriteLine(result.Message);
                })
            };

            await Task.WhenAll(tasks);

            await Task.Delay(500);

            var duplicateTasks = new[]
            {
                Task.Run(async () =>
                {
                    var result = await service.SaveItemAsync("a");
                    Console.WriteLine(result.Message);
                }),
                Task.Run(async () =>
                {
                    var result = await service.SaveItemAsync("b");
                    Console.WriteLine(result.Message);
                }),
                Task.Run(async () =>
                {
                    var result = await service.SaveItemAsync("c");
                    Console.WriteLine(result.Message);
                })
            };

            await Task.WhenAll(duplicateTasks);

            await Task.Delay(5000);

            Console.WriteLine("Everything recorded:");

            service.GetAllItems().ForEach(Console.WriteLine);

            Console.ReadLine();
        }
    }
}
