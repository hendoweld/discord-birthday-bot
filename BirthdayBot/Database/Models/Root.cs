using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BirthdayBot.Database.Models
{
    public class Root
    {
        public List<Birthday> Birthdays { get; set; } = new();
        public List<Guild> Guilds { get; set; } = new();
    }
}
