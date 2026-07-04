using Discord;
using Discord.Interactions;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;

namespace Mirinae.Services.DiscordBot
{
    public class CooldownAttribute : PreconditionAttribute
    {
        private readonly int _seconds;
        private static readonly MemoryCache Cache = new(new MemoryCacheOptions());

        public CooldownAttribute(int seconds)
        {
            _seconds = seconds;
        }

        public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
        {
            string key = $"{context.User.Id}:{commandInfo.Name}";

            if (Cache.TryGetValue(key, out _))
            {
                return Task.FromResult(PreconditionResult.FromError($"cooldown:{_seconds}"));
            }

            Cache.Set(key, true, TimeSpan.FromSeconds(_seconds));
            return Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}