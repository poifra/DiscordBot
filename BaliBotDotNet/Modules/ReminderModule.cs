using BaliBotDotNet.Data.Interfaces;
using BaliBotDotNet.Services;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using System.Timers;
using RunMode = Discord.Interactions.RunMode;

namespace BaliBotDotNet.Modules
{
    public class ReminderModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly IReminderRepository _reminderRepository;
        private DiscordSocketClient _client { get; set; }
        public ReminderModule(IReminderRepository reminderRepository, DiscordSocketClient client)
        {
            _reminderRepository = reminderRepository;
            _client = client;
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
                await RespondAsync("There is no reminder with that ID!");
                return;
            }
            if(Context.User.Id != reminder.AuthorID)
            {
                await RespondAsync("You cannot delete someone else's reminder!");
                return;
            }
            _reminderRepository.DeleteReminder(reminderID);
            await RespondAsync($"Deleted reminder \"{reminder.ReminderText}\"");
        }

        public enum ReminderUnits
        { 
            Minutes,Hours,Days,Years
        }

        [SlashCommand("reminder", "Sets a reminder that pings you in a fixed amount of time.", runMode: RunMode.Async)]
        public async Task CreateReminderAsync(int amount, ReminderUnits time, string text)
        {
            DateTime remindDate = DateTime.Now;
            string unit = time.ToString();
            switch (unit)
            {
                case "Minutes":
                    remindDate = remindDate.AddMinutes(amount);
                    break;
                case "Days":
                    remindDate = remindDate.AddDays(amount);
                    break;
                case "Hours":
                    remindDate = remindDate.AddHours(amount);
                    break;
                case "Years":
                    remindDate = remindDate.AddYears(amount);
                    break;
                default:
                    await ReplyAsync("Available time units are years, days, hours, minutes.");
                    return;

            }
            int id = _reminderRepository.InsertReminder(Context.User.Id, Context.Channel.Id, remindDate, text);
            await RespondAsync($"I will remind you of this in {amount} {unit}. If you want to delete it in the future, use `$deletereminder {id}`.");

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