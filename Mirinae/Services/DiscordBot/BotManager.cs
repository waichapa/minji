using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Extensions.DependencyInjection;
using Mirinae.Services.Ai;
using Mirinae.Services.Database;

namespace Mirinae.Services.DiscordBot
{
    internal class BotManager
    {
        private DiscordSocketClient _client;
        private InteractionService _interactionService;
        private IServiceProvider _services;
        private System.Timers.Timer _energyTimer;

        public async Task StartAsync()
        {
            var config = new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.AllUnprivileged
            };

            _client = new DiscordSocketClient(config);
            _interactionService = new InteractionService(_client.Rest);

            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_interactionService)
                .AddSingleton<DatabaseService>()
                .AddSingleton<AiService>()
                .BuildServiceProvider();

            _client.Log += Log;
            _interactionService.Log += Log;

            _client.Ready += ReadyAsync;
            _client.InteractionCreated += HandleInteraction;

            _energyTimer = new System.Timers.Timer(60 * 60 * 1000);
            _energyTimer.Elapsed += OnEnergyTimerElapsed;
            _energyTimer.AutoReset = true;
            _energyTimer.Enabled = true;

            string token = "TOKEN";

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private void OnEnergyTimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                var db = _services.GetRequiredService<DatabaseService>();
                db.RegenerateEnergy();
                Console.WriteLine($"[{DateTime.Now}] Energy restoration completed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during energy restoration: {ex.Message}");
            }
        }

        private async Task ReadyAsync()
        {
            await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            await _interactionService.RegisterCommandsGloballyAsync();
            Console.WriteLine("Минжи готова обучать!");
        }

        private async Task HandleInteraction(SocketInteraction interaction)
        {
            try
            {
                var context = new SocketInteractionContext(_client, interaction);
                var result = await _interactionService.ExecuteCommandAsync(context, _services);

                if (!result.IsSuccess)
                {
                    if (result.Error == InteractionCommandError.UnmetPrecondition && result.ErrorReason.StartsWith("cooldown:"))
                    {
                        await interaction.RespondAsync("❌ Wait a bit! 천천히 하세요!", ephemeral: true);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка взаимодействия: {ex}");
            }
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}