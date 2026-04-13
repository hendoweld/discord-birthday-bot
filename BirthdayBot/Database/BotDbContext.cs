using BirthdayBot.Database.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BirthdayBot.Database
{
    public class BotDbContext : DbContext
    {
        public DbSet<Birthday> Birthdays { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseNpgsql("Host=localhost;Database=bot;Username=bot;Password=pass");
    }
}
