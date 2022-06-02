using BaliBotDotNet.Data.Interfaces;
using BaliBotDotNet.Services;
using Discord;
using Discord.Interactions;
using System;
using System.Threading.Tasks;
using System.Timers;
using RunMode = Discord.Interactions.RunMode;

namespace BaliBotDotNet.Modules
{
    public class ReminderModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly IReminderRepository _reminderRepository;
        public InteractionService Commands { get; set; }

        private InteractionHandler _handler;
        public ReminderModule(IReminderRepository reminderRepository, InteractionHandler handler)
        {
            _reminderRepository = reminderRepository;
            _handler = handler;
            Timer t = new(1000 * 60);
            t.Elapsed += CheckForReminders;
            t.Start();
        }

        [SlashCommand("deletereminder","Deletes a specific reminder", runMode: RunMode.Async)]
        public async Task DeleteReminderAsync(int reminderID)
        {
            var reminder = _reminderRepository.GetReminder(reminderID);
            if (reminder == null)
            {
                await ReplyAsync("There is no reminder with that ID!");
                return;
            }
            if(Context.User.Id != reminder.AuthorID)
            {
                await ReplyAsync("You cannot delete someone else's reminder!");
                return;
            }
            _reminderRepository.DeleteReminder(reminderID);
            await ReplyAsync($"Deleted reminder \"{reminder.ReminderText}\"");
        }

        //[SlashCommand("reminder", "Sets a reminder in a fixed amount of time. Example usage: $reminder 10 hours \"dentist\". Possible units are minutes, hours or days.", runMode : RunMode.Async)]
        //public async Task CreateReminderAsync(int amount, string unit, params string[] text)
        //{
        //    DateTime remindDate = DateTime.Now;
        //    switch (unit)
        //    {
        //        case "m":
        //        case "minute":
        //        case "minutes":
        //            remindDate = remindDate.AddMinutes(amount);
        //            break;
        //        case "d":
        //        case "days":
        //        case "day":
        //            remindDate = remindDate.AddDays(amount);
        //            break;
        //        case "h":
        //        case "hour":
        //        case "hours":
        //            remindDate = remindDate.AddHours(amount);
        //            break;
        //        case "y":
        //        case "year":
        //        case "years":
        //            remindDate = remindDate.AddYears(amount);
        //            break;
        //        default:
        //            await ReplyAsync("Available time units are years, days, hours, minutes.");
        //            return;

        //    }
        //    int id = _reminderRepository.InsertReminder(Context.User.Id, Context.Channel.Id, remindDate, string.Join(" ",text));
        //    await ReplyAsync($"I will remind you of this in {amount} {unit}. If you want to delete it in the future, use `$deletereminder {id}`.");

        //}

        private async void CheckForReminders(object sender, ElapsedEventArgs e)
        {
            var reminders = _reminderRepository.CheckForReminders();
            foreach (var reminder in reminders)
            {
                var channel = Context.Guild.GetChannel(reminder.ChannelID) as IMessageChannel;
                await channel.SendMessageAsync($"{MentionUtils.MentionUser(reminder.AuthorID)} you wanted to be reminded of : \"{reminder.ReminderText}\"");
                _reminderRepository.SetReminderDone(reminder.ReminderID);
            }
        }
    }
}