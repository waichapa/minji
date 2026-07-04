🌸 Minji — Your Personal AI Korean Tutor in Discord
=====================================

Minji is an interactive Discord bot that turns learning Korean into an exciting RPG adventure. Instead of boring textbooks — live AI dialogues, leveling up, quizzes based on your own words, and real TOPIK exams right in your chat.

✨ Key Features
---------------------
- 🧠 Personalized AI Lessons — the bot adapts to your level and generates unique exercises.
- 📚 Smart Vocabulary — save words with /vocab_add, and Minji creates custom quizzes for you.
- 📝 TOPIK Exams — test your skills with the /exam command.
- 💬 Live Chat with a Tutor — speak Korean with Minji, she corrects mistakes and maintains context.
- 🎭 RPG Progression System — earn XP for lessons, level up, but spend energy (recharges over time).
- 🇰🇷 K-Culture Guide — get K-Pop, K-Food, and K-Drama recommendations on demand.

⚙️ For Server Admins
-------------------------------
- Zero Configuration — the bot works immediately after being invited.
- Security — uses only slash commands, minimal permissions required.
- Boost Engagement — quizzes and leveling keep members active and involved.

🚀 Quick Start
----------------
1. Invite the bot to your server using the invitation link.
2. Use /help to see all available commands.
3. Start with /profile — create your game profile.
4. Try /vocab_add word translation — add your first word to the dictionary.
5. Run /quiz — and test how well you remember it.

🛠️ Tech Stack
--------------
- .NET 10
- C#
- Discord.Net — Discord API wrapper
- Microsoft.Data.Sqlite — lightweight local database
- OpenAI — AI-powered conversations and lesson generation

📂 Installation (for Developers)
--------------------------------
git clone https://github.com/waichapa/minji.git
cd minji-bot

# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Configure appsettings.json with your tokens:
# - DiscordToken
# - OpenAIApiKey

# Run the bot
dotnet run

---
Add Minji to your server and start learning Korean with fun! 🇰🇷✨