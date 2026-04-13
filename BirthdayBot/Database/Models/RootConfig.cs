using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BirthdayBot.Database.Models
{
    public class RootConfig
    {
        public string Token { get; set; }
        public List<GuildConfig> GuildConfigs { get; set; } = new();
    }
}
