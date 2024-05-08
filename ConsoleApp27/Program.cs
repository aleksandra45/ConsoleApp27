using System;
using System.Text;
using System.Threading.Tasks;
using ConsoleApp25;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;


namespace UtilityBot
{
    static class Program
    {
        public static async Task Main()
        {
            Console.OutputEncoding = Encoding.Unicode;

            var host = new HostBuilder()
                .ConfigureServices((hostContext, services) => ConfigureServices(services))
                .UseConsoleLifetime()
                .Build();

            Console.WriteLine("Сервис запущен");
            // Запускаем сервис
            await host.RunAsync();
            Console.WriteLine("Сервис остановлен");

        }

        static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<Storage>();

            services.AddSingleton<ITelegramBotClient>(provider => new TelegramBotClient("6522502182:AAEvJ3-iyqhE74MWSeGCUFWrrKFFX-buU9k"));
            services.AddHostedService<Bot>();
        }
    }
}