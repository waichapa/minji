using System;

namespace Mirinae.Services.Database
{
    public class UserProfile
    {
        public ulong UserId { get; set; }
        public int Level { get; set; }
        public int Xp { get; set; }
        public int Energy { get; set; }
        public DateTime LastLessonTime { get; set; }
        public bool IsPremium { get; set; }
    }
}