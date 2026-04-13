using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BirthdayBot.Database.Models
{
    public class GuildConfig
    {
        public ulong GuildId { get; set; }

        public ulong BirthdayChannelId { get; set; }
        public ulong BirthdayRoleId { get; set; }

    }

}
