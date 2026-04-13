using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BirthdayBot.Database.Models
{
    public class Birthday
    {
        public int Id { get; set; }
        public ulong UserId { get; set; }
        public DateTime Date { get; set; }
    }
}
