using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using System.Text;
using System.Linq;
using Mirinae.Services.Ai;
using Mirinae.Services.Database;

namespace Mirinae.Services.DiscordBot
{
    public class KoreanModal : IModal
    {
        public string Title => "Long AI Request";

        [InputLabel("Your detailed question / text to analyze")]
        [ModalTextInput("long_question_input", TextInputStyle.Paragraph, placeholder: "Type here...", minLength: 10, maxLength: 1500)]
        public string Question { get; set; }
    }

    public class InteractionModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly AiService _aiService;
        private readonly DatabaseService _db;

        private const string PremiumRoleName = "Chinsu Premium";

        public InteractionModule(AiService aiService, DatabaseService db)
        {
            _aiService = aiService;
            _db = db;
        }

        [SlashCommand("help", "Display the comprehensive guide and list of all available Minji's Academy commands")]
        public async Task HelpCommand()
        {
            var userProfile = _db.GetOrCreateUser(Context.User.Id);
            bool isPremium = userProfile.IsPremium;

            var embed = new EmbedBuilder()
                .WithTitle("🎓 Minji's Korean Academy — The Complete Guide")
                .WithDescription(
                    $"안녕하세요, {Context.User.Username}! I am Minji, your dedicated virtual Korean tutor. " +
                    $"Here is a full breakdown of every command available to assist you in your language and cultural journey!" +
                    $"\n\n**Your Current Status:** {(isPremium ? "⭐ **Premium Student**" : "📝 Regular Student")}")

                .AddField("📊 1. Progress & Academy Systems",
                    "• `/profile` — Check your current academic level, XP tracker, and energy (**⚡**).\n" +
                    "• `/lesson` — Take a level-appropriate mini-lesson to learn grammar or vocabulary. *(+20 XP)*\n" +
                    "• `/exam` — Challenge yourself with a level-up test once your XP threshold is met!")

                .AddField("📓 2. Personal Vocabulary Notebook",
                    "• `/vocab_add [word] [translation]` — Save a new phrase to your private flashcard deck. *(+5 XP)*\n" +
                    "• `/vocab_list` — Review the last 15 words you have saved in your notebook.\n" +
                    "• `/word_review` — Play a quick mental quiz using a random word from your flashcards.")

                .AddField("💬 3. AI Interactivity & AI Chat",
                    "• `/ask [question]` — Chat freely with Minji or ask language questions. *(Max 250 chars, maintains memory)*\n" +
                    "• `/ask_long` — Open a text modal to submit an extended question or essay up to 1500 chars. *(⭐ Premium Only)*\n" +
                    "• `/clear` — Instantly wipe Minji's short-term conversation memory for this specific text channel.")

                .AddField("🌍 4. Cultural Insights & Media",
                    "• `/grammar [pattern]` — Lookup a quick blueprint/explanation for any specific Korean grammar structure.\n" +
                    "• `/quiz` — Test your knowledge with random trivia questions about Korea and its language.\n" +
                    "• `/k-pop` — Discover a new K-pop track recommendation.\n" +
                    "• `/k-food` — Get a delicious random dish recommendation from Korean cuisine.\n" +
                    "• `/kdrama [genre]` — Get a top-tier recommendation for a Korean television drama series.")

                .AddField("⚙️ 5. Utilities & Premium",
                    "• `/check_premium` — Check your Premium status and view active subscription benefits.\n" +
                    "• `/avatar [user]` — Display the full-size profile avatar of yourself or another user.\n" +
                    "• `/server-status` — Check up-to-date member counts, boosts, and roles statistics for this Discord server.")

                .AddField("⚡ Energy & Cooldown Rules",
                    $"• **Regular Students:** Lessons cost **25 ⚡** with a **3-hour** cooldown.\n" +
                    $"• **Premium Students:** Lessons cost **10 ⚡** with a **10-minute** cooldown.\n" +
                    "• *Energy naturally regenerates at a rate of +10 ⚡ per hour.*")

                .WithFooter("Tip: Mix daily lessons and cultural quizzes to maximize your Korean fluency effectively!")
                .WithColor(isPremium ? Color.Purple : Color.Blue)
                .WithCurrentTimestamp()
                .Build();

            await RespondAsync(embed: embed);
        }

        [SlashCommand("check_premium", "View your current Premium Student status and its benefits")]
        public async Task CheckPremiumCommand()
        {
            var userProfile = _db.GetOrCreateUser(Context.User.Id);
            bool isPremium = userProfile.IsPremium;

            var embed = new EmbedBuilder()
                .WithTitle("💎 Minji's Academy Premium Status")
                .WithThumbnailUrl(Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl())
                .WithCurrentTimestamp();

            if (isPremium)
            {
                embed.WithColor(Color.Purple)
                     .WithDescription($"✨ **안녕하세요, {Context.User.Username}! You are a Premium Student!**\nAll exclusive academy features are fully unlocked for your account.")
                     .AddField("🚀 Your Active Benefits:",
                         "• **Reduced Cooldowns:** Only **10 minutes** between lessons (instead of 3 hours)!\n" +
                         "• **Energy Discount:** Lessons cost just **10 ⚡** (instead of 25 ⚡).\n" +
                         "• **Advanced AI Access:** Unlocked `/ask_long` command for advanced analysis (up to 1500 chars).")
                     .WithFooter("Thank you for supporting Minji's Academy! 💖");
            }
            else
            {
                embed.WithColor(Color.LightGrey)
                     .WithDescription($"📝 **안녕하세요, {Context.User.Username}! You are currently a Regular Student.**\nYou can still use all basic features, but with standard academic limits.")
                     .AddField("🔒 Locked Premium Features:",
                         "• ⚡ Lessons cost **25 energy** with a **3-hour** cooldown.\n" +
                         "• ❌ Access to the extended `/ask_long` command is locked.")
                     .AddField("❓ How to get Premium?",
                         $"Premium status can be granted by the Bot Administrator or via your server's custom active system.")
                     .WithFooter("Upgrade your status to speed up your Korean learning! 📚");
            }

            await RespondAsync(embed: embed.Build());
        }

        [SlashCommand("clear", "Reset Minji's conversation memory for this channel")]
        public async Task ClearCommand()
        {
            _aiService.ClearContext(Context.Channel.Id);
            await RespondAsync("🧹 Channel memory cleared! Let's start fresh!", ephemeral: false);
        }

        [SlashCommand("avatar", "Display a user's avatar")]
        public async Task AvatarCommand(IUser user = null)
        {
            var targetUser = user ?? Context.User;
            var avatarUrl = targetUser.GetAvatarUrl(size: 1024) ?? targetUser.GetDefaultAvatarUrl();

            var embed = new EmbedBuilder()
                .WithTitle($"✨ {targetUser.Username}'s Avatar")
                .WithImageUrl(avatarUrl)
                .WithColor(Color.Purple)
                .Build();

            await RespondAsync(embed: embed);
        }

        [SlashCommand("server-status", "Check the server statistics")]
        public async Task ServerStatusCommand()
        {
            var embed = new EmbedBuilder()
                .WithTitle($"🏰 {Context.Guild.Name} Status")
                .AddField("Members", Context.Guild.MemberCount, true)
                .AddField("Roles", Context.Guild.Roles.Count, true)
                .AddField("Premium Boosts", Context.Guild.PremiumSubscriptionCount, true)
                .WithColor(Color.Green)
                .WithCurrentTimestamp()
                .Build();

            await RespondAsync(embed: embed);
        }

        [SlashCommand("profile", "Show your Korean learning progress profile")]
        public async Task ProfileCommand()
        {
            var user = _db.GetOrCreateUser(Context.User.Id);
            int xpNeeded = user.Level * 100;

            int filledBlocks = user.Energy / 10;
            string energyBar = new string('▰', filledBlocks) + new string('▱', 10 - filledBlocks);

            var embed = new EmbedBuilder()
                .WithTitle($"🎓 {Context.User.Username}'s Academy Profile")
                .WithDescription($"Welcome back! Keep studying to accumulate XP and level up." +
                                 $"\nStatus: {(user.IsPremium ? "⭐ **Premium Student**" : "📝 Regular Student")}")
                .AddField("📊 Level", $"**Lvl {user.Level}**", true)
                .AddField("✨ Experience Points", $"*{user.Xp} / {xpNeeded} XP*", true)
                .AddField("⚡ Energy / Stamina", $"**{user.Energy}/100**\n`[{energyBar}]`", false)
                .WithThumbnailUrl(Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl())
                .WithColor(user.IsPremium ? Color.Purple : Color.Orange)
                .Build();

            await RespondAsync(embed: embed);
        }

        [SlashCommand("lesson", "Get a customized Korean mini-lesson and earn XP!")]
        public async Task LessonCommand()
        {
            var userProfile = _db.GetOrCreateUser(Context.User.Id);
            bool isPremium = userProfile.IsPremium;

            TimeSpan cooldownTime = isPremium ? TimeSpan.FromMinutes(10) : TimeSpan.FromHours(3);
            int energyCost = isPremium ? 10 : 25;

            var timeSinceLastLesson = DateTime.UtcNow - userProfile.LastLessonTime;
            if (timeSinceLastLesson < cooldownTime)
            {
                var timeLeft = cooldownTime - timeSinceLastLesson;
                string formattedTime = timeLeft.Hours > 0
                    ? $"{timeLeft.Hours}h {timeLeft.Minutes}m"
                    : $"{timeLeft.Minutes}m {timeLeft.Seconds}s";

                string premNotice = isPremium ? "" : "\n⭐ *Premium users get a 10-minute cooldown instead of 3 hours!*";

                await RespondAsync($"❌ **Minji is resting!** Please wait **{formattedTime}** before taking another lesson.{premNotice}", ephemeral: true);
                return;
            }

            if (userProfile.Energy < energyCost)
            {
                await RespondAsync($"❌ **Not enough energy!** This lesson costs **{energyCost} ⚡**, but you only have **{userProfile.Energy}**. Energy restores naturally over time (+10/hour).", ephemeral: true);
                return;
            }

            await DeferAsync();

            int remainingEnergy = userProfile.Energy - energyCost;
            _db.UpdateEnergyAndLesson(Context.User.Id, remainingEnergy, DateTime.UtcNow);

            string prompt = $"Generate a high-quality, ultra-short structured lesson about the Korean language appropriate for a student at level {userProfile.Level}. " +
                            $"Pick a random topic: a crucial verb, a survival phrase, or an honorific particle. " +
                            $"Format: Give it a nice header, explain the concept using 3 brief bullet points, and show 2 practical dialogue examples (Hangul, Romanization, English).";

            string lessonContent = await _aiService.AskAiWithContextAsync(Context.Channel.Id, prompt);

            _db.AddXp(Context.User.Id, 20, out bool leveledUp, out int newLevel);

            var sb = new StringBuilder();
            sb.AppendLine($"📚 **Minji's Academy: Level {userProfile.Level} Lesson**");
            sb.AppendLine(lessonContent);
            sb.AppendLine("\n---");
            sb.AppendLine($"⚡ *Used {energyCost} energy. Remaining: {remainingEnergy}/100*");
            sb.AppendLine($"🎉 *You have completed the lesson and earned **+20 XP**!*");

            if (leveledUp)
            {
                sb.AppendLine($"🚀 **XP Threshold Reached!** Use `/exam` to officially level up to Level {newLevel}!");
            }

            var embed = new EmbedBuilder()
                .WithTitle("📚 Lesson Completed!")
                .WithDescription(sb.ToString())
                .WithColor(isPremium ? Color.Purple : Color.Blue)
                .Build();

            await FollowupAsync(embed: embed);
        }

        [SlashCommand("exam", "Take an official level-up test when you have enough XP")]
        public async Task ExamCommand()
        {
            var user = _db.GetOrCreateUser(Context.User.Id);
            int xpNeeded = user.Level * 100;

            if (user.Xp < xpNeeded)
            {
                await RespondAsync($"❌ **You are not ready!** You need **{user.Xp}/{xpNeeded} XP** to qualify for the Lvl {user.Level + 1} exam. Keep taking `/lesson`!", ephemeral: true);
                return;
            }

            if (user.Energy < 30)
            {
                await RespondAsync($"❌ **Too tired!** An official exam requires **30 ⚡**, you only have {user.Energy}.", ephemeral: true);
                return;
            }

            await DeferAsync();

            string prompt = $"Generate exactly ONE challenging multiple-choice question testing knowledge required to pass Level {user.Level} in Korean. " +
                            $"Provide 4 distinct options (A, B, C, D). Put the correct answer at the absolute bottom hidden behind a Discord spoiler tag like ||Answer: B||.";

            string examContent = await _aiService.AskAiWithContextAsync(Context.Channel.Id, prompt);

            _db.UpdateEnergyAndLesson(Context.User.Id, user.Energy - 30, user.LastLessonTime);
            _db.AddXp(Context.User.Id, -xpNeeded, out _, out _);

            var embed = new EmbedBuilder()
                .WithTitle($"🎓 Official Level-Up Exam (Lvl {user.Level} ➔ {user.Level + 1})")
                .WithDescription(examContent)
                .WithFooter("Click the spoiler to check your answer. If you got it right, welcome to your new level!")
                .WithColor(Color.Red)
                .Build();

            await FollowupAsync(embed: embed);
        }

        [SlashCommand("vocab_add", "Save a new word to your personal learning dictionary")]
        public async Task VocabAddCommand(string koreanWord, string translation)
        {
            _db.SaveWord(Context.User.Id, koreanWord, translation);
            _db.AddXp(Context.User.Id, 5, out _, out _);

            var embed = new EmbedBuilder()
                .WithTitle("📝 Word Saved to Notebook!")
                .WithDescription($"Successfully recorded into your memory card bank. Minji proud of you! (+5 XP)")
                .AddField("🇰🇷 Korean", $"**{koreanWord}**", true)
                .AddField("🌐 Translation", translation, true)
                .WithColor(Color.Green)
                .Build();

            await RespondAsync(embed: embed);
        }

        [SlashCommand("vocab_list", "Review the last 15 words saved in your notebook")]
        public async Task VocabListCommand()
        {
            var list = _db.GetVocabulary(Context.User.Id);

            if (list.Count == 0)
            {
                await RespondAsync("Your vocabulary notebook is empty! Use `/vocab_add` to add your first Korean words.", ephemeral: true);
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine("📓 **Your Personal Korean Notebook (Last 15 words):**");
            sb.AppendLine("```all");
            foreach (var item in list)
            {
                sb.AppendLine($"• {item.Word}  -->  {item.Trans}");
            }
            sb.AppendLine("```");
            sb.AppendLine("*Review these regularly to master your conversations!* 🧠");

            await RespondAsync(sb.ToString());
        }

        [SlashCommand("word_review", "Test yourself on a random word from your personal notebook")]
        public async Task WordReviewCommand()
        {
            var list = _db.GetVocabulary(Context.User.Id);

            if (list.Count == 0)
            {
                await RespondAsync("❌ Your notebook is empty! Add words using `/vocab_add` before you can review them.", ephemeral: true);
                return;
            }

            var random = new Random();
            var targetWord = list[random.Next(list.Count)];

            var embed = new EmbedBuilder()
                .WithTitle("🧠 Personal Flashcard Review")
                .WithDescription($"Do you remember what this word means?\n\n🎒 **Korean Word:** || {targetWord.Word} ||")
                .AddField("Answer Key (Spoiler)", $"|| {targetWord.Trans} ||")
                .WithFooter("Hover/Click the bars to reveal the word and check if you remembered it correctly!")
                .WithColor(Color.Teal)
                .Build();

            await RespondAsync(embed: embed);
        }

        [SlashCommand("ask", "Ask Minji about Korean language or culture")]
        [Cooldown(5)]
        public async Task AskCommand(string question)
        {
            if (string.IsNullOrWhiteSpace(question) || question.Length > 250)
            {
                await RespondAsync("❌ Your question is too long or empty! Please keep it under 250 characters.", ephemeral: true);
                return;
            }

            await DeferAsync();

            string aiResponse = await _aiService.AskAiWithContextAsync(Context.Channel.Id, question);
            await FollowupAsync($"**❓ {Context.User.Username}:** {question}\n\n{aiResponse}");
        }

        [SlashCommand("ask_long", "Submit a long question (Requires Premium Role)")]
        [Cooldown(15)]
        public async Task AskLongCommand()
        {
            var userProfile = _db.GetOrCreateUser(Context.User.Id);

            if (!userProfile.IsPremium)
            {
                await RespondAsync($"❌ This command requires a **Premium Student** status!", ephemeral: true);
                return;
            }

            await RespondWithModalAsync<KoreanModal>("ask_long_modal");
        }

        [SlashCommand("grammar", "Look up a Korean grammar pattern")]
        [Cooldown(5)]
        public async Task GrammarCommand(string pattern)
        {
            await DeferAsync();
            string response = await _aiService.AskAiWithContextAsync(Context.Channel.Id, $"Explain the Korean grammar structure '{pattern}' simply in English. Show how it attaches to verbs/nouns, its core meaning, and 2 practical examples.");
            await FollowupAsync(response);
        }

        [SlashCommand("quiz", "Test your Korean language or culture knowledge")]
        [Cooldown(6)]
        public async Task QuizCommand()
        {
            await DeferAsync();
            string response = await _aiService.AskAiWithContextAsync(Context.Channel.Id, "Generate a fresh multiple-choice trivia question about Korean culture, history, or language. Provide 4 options (A, B, C, D) and hide the correct answer at the very bottom using a Discord spoiler tag like ||Answer: X||.");
            await FollowupAsync(response);
        }

        [SlashCommand("k-pop", "Get a random K-pop song recommendation")]
        [Cooldown(3)]
        public async Task KpopCommand()
        {
            await DeferAsync();
            string response = await _aiService.AskAiWithContextAsync(Context.Channel.Id, "Recommend one random awesome K-pop song. Format: **Artist - Song**. Give a short 1-sentence hype description.");
            await FollowupAsync(response);
        }

        [SlashCommand("k-food", "Get a random Korean food recommendation")]
        [Cooldown(3)]
        public async Task KfoodCommand()
        {
            await DeferAsync();
            string response = await _aiService.AskAiWithContextAsync(Context.Channel.Id, "Recommend one random delicious Korean dish. State its name in Korean and English, and briefly describe its flavor profile.");
            await FollowupAsync(response);
        }

        [SlashCommand("kdrama", "Get a top-tier Korean drama recommendation")]
        [Cooldown(4)]
        public async Task KdramaCommand(string genre = "any")
        {
            await DeferAsync();
            string response = await _aiService.AskAiWithContextAsync(Context.Channel.Id, $"Recommend one excellent Korean drama. The requested genre is: {genre}. Give its title, where to watch if known, and a brief compelling summary.");
            await FollowupAsync(response);
        }

        [SlashCommand("give_premium", "Admin Only: Grant or revoke premium status for a user by their Discord ID")]
        public async Task GivePremiumCommand(string targetUserIdStr, bool status)
        {
            if (Context.User.Id != 1440318879590121503)
            {
                await RespondAsync("❌ **Access Denied:** Only the Bot Administrator can run this command.", ephemeral: true);
                return;
            }

            if (!ulong.TryParse(targetUserIdStr, out ulong targetUserId))
            {
                await RespondAsync("❌ **Invalid ID:** Please provide a valid numerical Discord User ID.", ephemeral: true);
                return;
            }

            await DeferAsync();

            _db.GetOrCreateUser(targetUserId);
            _db.SetPremiumStatus(targetUserId, status);

            var embed = new EmbedBuilder()
                .WithTitle("👑 Admin Action: Premium Database Update")
                .WithDescription($"Premium status for user `<@{targetUserId}>` (ID: `{targetUserId}`) has been successfully updated.")
                .AddField("🆕 New Status", status ? "⭐ **Premium Student** (Enabled)" : "📝 **Regular Student** (Disabled)", true)
                .WithColor(status ? Color.Purple : Color.DarkGrey)
                .WithCurrentTimestamp()
                .Build();

            await FollowupAsync(embed: embed);
        }
    }

    public class InteractionComponentsModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly AiService _aiService;

        public InteractionComponentsModule(AiService aiService)
        {
            _aiService = aiService;
        }

        [ModalInteraction("ask_long_modal")]
        public async Task HandleLongQuestionModal(KoreanModal modal)
        {
            await DeferAsync();
            string question = modal.Question;
            string aiResponse = await _aiService.AskAiWithContextAsync(Context.Channel.Id, question);
            await FollowupAsync($"**📝 Long Inquiry from {Context.User.Username}:**\n> {question}\n\n{aiResponse}");
        }
    }
}