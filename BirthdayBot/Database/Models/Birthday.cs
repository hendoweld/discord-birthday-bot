using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BirthdayBot.Database.Models
{
    public class Birthday
    {
        public ulong UserId { get; set; }
        public int Day { get; set; }
        public int Month { get; set; }
    }
}
