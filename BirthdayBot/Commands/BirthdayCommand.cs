using BirthdayBot.Services;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BirthdayBot.Commands
{
    public class BirthdayCommand : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly BirthdayService _birthdayService;

        public BirthdayCommand(BirthdayService birthdayService)
        {
            _birthdayService = birthdayService;
        }

        [SlashCommand("birthday", "Set your birthday")]
        public async Task SetBirthday(int day, int month)
        {
            _birthdayService.AddBirthday(Context.User.Id, day, month);

            await RespondAsync("🎂 Dein Geburtstag wurde gespeichert!");
        }
    }
}
