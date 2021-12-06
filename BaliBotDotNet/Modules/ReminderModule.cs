using BaliBotDotNet.Data.Interfaces;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using System.Timers;

namespace BaliBotDotNet.Modules
{
    public class ReminderModule : ModuleBase<SocketCommandContext>
    {
        private readonly IReminderRepository _reminderRepository;
        private readonly DiscordSocketClient _client;
        public ReminderModule(IReminderRepository reminderRepository, DiscordSocketClient client)
        {
            _reminderRepository = reminderRepository;
            _client = client;
            Timer t = new Timer(1000 * 60);
            t.Elapsed += CheckForReminders;
            t.Start();
        }

        [Command("deletereminder", RunMode = RunMode.Async)]
        public async Task DeleteReminderAsync(int reminderID)
        {
            var reminder = _reminderRepository.GetReminder(reminderID);
            if (reminder == null)
            {
                await ReplyAsync("There is no reminder with that ID!");
                return;
            }
            if(Context.Message.Author.Id != reminder.AuthorID)
            {
                await ReplyAsync("You cannot delete someone else's reminder!");
                return;
            }
            _reminderRepository.DeleteReminder(reminderID);
            await ReplyAsync($"Deleted reminder \"{reminder.ReminderText}\"");
        }

        [Command("reminder", RunMode = RunMode.Async)]
        public async Task CreateReminderAsync(int amount, string unit, string text)
        {
            DateTime remindDate = DateTime.Now;
            switch (unit)
            {
                case "m":
                case "minute":
                case "minutes":
                    remindDate = remindDate.AddMinutes(amount);
                    break;
                case "d":
                case "days":
                case "day":
                    remindDate = remindDate.AddDays(amount);
                    break;
                case "h":
                case "hour":
                case "hours":
                    remindDate = remindDate.AddHours(amount);
                    break;
                default:
                    await ReplyAsync("Available time units are days, hours and minutes.");
                    return;

            }
            int id = _reminderRepository.InsertReminder(Context.Message.Author.Id, Context.Channel.Id, remindDate, text);
            await ReplyAsync($"I will remind you of this in {amount} {unit}. If you want to delete it in the future, use `$deletereminder {id}`.");

        }

        private async void CheckForReminders(object sender, ElapsedEventArgs e)
        {
            var reminders = _reminderRepository.CheckForReminders();
            foreach (var reminder in reminders)
            {
                var channel = _client.GetChannel(reminder.ChannelID) as IMessageChannel;
                await channel.SendMessageAsync($"{MentionUtils.MentionUser(reminder.AuthorID)} you wanted to be reminded of : \"{reminder.ReminderText}\"");
                _reminderRepository.SetReminderDone(reminder.ReminderID);
            }
        }
    }
}