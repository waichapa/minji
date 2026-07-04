using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ClientModel;
using System.Threading.Tasks;
using OpenAI;
using OpenAI.Chat;

namespace Mirinae.Services.Ai
{
    public class AiService
    {
        private readonly ChatClient _client;
        private const string ModelName = "YOUR MODEL";
        private readonly ConcurrentDictionary<ulong, List<ChatMessage>> _channelHistory = new();
        private const int MaxHistoryMessages = 12;

        public AiService()
        {
            string apiKey = "API_KEY";

            var options = new OpenAIClientOptions();
            options.Endpoint = new Uri("https://api.proxyapi.ru/openai/v1");

            var openAiClient = new OpenAIClient(new ApiKeyCredential(apiKey), options);
            _client = openAiClient.GetChatClient(ModelName);
        }

        public async Task<string> AskAiWithContextAsync(ulong channelId, string userPrompt, string customSystem = null)
        {
            try
            {
                var history = _channelHistory.GetOrAdd(channelId, _ => new List<ChatMessage>());

                lock (history)
                {
                    if (history.Count == 0 || !string.IsNullOrEmpty(customSystem))
                    {
                        history.Clear();
                        string systemPrompt = customSystem ??
                            "You are Minji (민지), a friendly, cute, and passionate 18-year-old Korean girl (born Feb 20, 2008) who acts as a language guide. " +
                            "PREMIUM INFO: 'Chinsu Premium' is a special student rank. Regular students have a 3-hour cooldown and pay 25 energy per lesson. " +
                            "Premium students get a 10-minute cooldown, pay only 10 energy, and unlock the `/ask_long` command. " +
                            "CRITICAL LANGUAGE RULE: When a user speaks in English, reply entirely in English but naturally include useful Korean words, phrases, or full sentences in parentheses with their hangul/romanization to help them learn. " +
                            "When a user speaks in Korean, reply entirely in Korean to maintain a natural conversation practice. " +
                            "NEVER use Russian under any circumstances. " +
                            "Structure your responses beautifully using Markdown and emojis. Keep answers concise and under 600 characters.";
                        history.Add(new SystemChatMessage(systemPrompt));
                    }

                    history.Add(new UserChatMessage(userPrompt));
                }

                ChatCompletion completion = await _client.CompleteChatAsync(history);
                string aiResponse = completion.Content[0].Text;

                lock (history)
                {
                    history.Add(new AssistantChatMessage(aiResponse));

                    while (history.Count > MaxHistoryMessages)
                    {
                        if (history.Count > 2)
                        {
                            history.RemoveAt(1);
                            history.RemoveAt(1);
                        }
                        else break;
                    }
                }

                return aiResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка AI: {ex.Message}");
                return "Sorry, Minji couldn't connect to the AI network right now. (Please try again later!)";
            }
        }

        public void ClearContext(ulong channelId)
        {
            if (_channelHistory.TryGetValue(channelId, out var history))
            {
                lock (history)
                {
                    history.Clear();
                }
            }
        }
    }
}