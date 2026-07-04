using System;
using System.Threading.Tasks;
using Mirinae.Services.DiscordBot;

class Program 
{
    static async Task Main(string[] args)
    {
        BotManager host = new BotManager();

        await host.StartAsync();
    }
}