using BaliBotDotNet.Data.Interfaces;
using BaliBotDotNet.Services;
using BaliBotDotNet.Utilities.ExtensionMethods;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using System.Timers;
using RunMode = Discord.Interactions.RunMode;

namespace BaliBotDotNet.Modules
{
    public class ReminderModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly IServiceScopeFactory _serviceProvider;
        private DiscordSocketClient _client { get; set; }
        public ReminderModule(DiscordSocketClient client, IServiceScopeFactory serviceProvider)
        {
            _client = client;
            _serviceProvider = serviceProvider;
            Timer t = new(1000 * 60);
            t.Elapsed += CheckForReminders;
            t.Start();
        }

        [SlashCommand("deletereminder","Deletes a specific reminder", runMode: RunMode.Async)]
        public async Task DeleteReminderAsync(int reminderID)
        {
            using var scope = _serviceProvider.CreateScope();
            var reminderRepository = scope.ServiceProvider.GetRequiredService<IReminderRepository>();
            var reminder = reminderRepository.GetReminder(reminderID);
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
            reminderRepository.DeleteReminder(reminderID);
            await RespondAsync($"Deleted reminder \"{reminder.ReminderText}\"");
        }

        public enum ReminderUnits
        { 
            Minutes,Hours,Days,Years
        }

        public enum ReminderTimezones
        {
            EST,
            CST,
            PST,
            CEST,
            BST,
            GMT
        }

        public enum ReminderMonths
        {
            JAN = 1,
            FEB = 2,
            MAR = 3,
            APR = 4,
            MAY = 5,
            JUN = 6,
            JUL = 7,
            AUG = 8,
            SEP = 9,
            OCT = 10,
            NOV = 11,
            DEC = 12
        }

        [SlashCommand("reminder", "Sets a reminder that pings you in a fixed amount of time.", runMode: RunMode.Async)]
        public async Task CreateReminderAsync(int amount, ReminderUnits time, string text)
        {
            using var scope = _serviceProvider.CreateScope();
            var reminderRepository = scope.ServiceProvider.GetRequiredService<IReminderRepository>();
            DateTime remindDate = DateTime.Now;
            switch (time)
            {
                case ReminderUnits.Minutes:
                    remindDate = remindDate.AddMinutes(amount);
                    break;
                case ReminderUnits.Days:
                    remindDate = remindDate.AddDays(amount);
                    break;
                case ReminderUnits.Hours:
                    remindDate = remindDate.AddHours(amount);
                    break;
                case ReminderUnits.Years:
                    remindDate = remindDate.AddYears(amount);
                    break;
                default:
                    await ReplyAsync("Available time units are years, days, hours, minutes.");
                    return;

            }
            int id = reminderRepository.InsertReminder(Context.User.Id, Context.Channel.Id, remindDate, text);
            await RespondAsync($"I will remind you of this in {amount} {time}. If you want to delete it in the future, use `/deletereminder {id}`.");

        }

        [SlashCommand("datereminder", "Sets a reminder that pings you at a fixed date and time.", runMode: RunMode.Async)]
        public async Task CreateDateReminderAsync(ReminderTimezones timezone, int year, ReminderMonths month, int day, int hour, int minute, string text)
        {
            using var scope = _serviceProvider.CreateScope();
            var reminderRepository = scope.ServiceProvider.GetRequiredService<IReminderRepository>();

            TimeZoneInfo localTimeZone = TimeZoneInfo.Local;
            TimeZoneInfo sourceTimeZone = timezone.ToTimeZoneInfo();

            DateTime sourceLocalTime;
            try
            {
                sourceLocalTime = new DateTime(
                    year,
                    (int)month,
                    day,
                    hour,
                    minute,
                    0,
                    DateTimeKind.Unspecified);
            }
            catch (ArgumentOutOfRangeException)
            {
                await ReplyAsync("Please provide a valid date and time.");
                return;
            }

            DateTime targetReminderTime = TimeZoneInfo.ConvertTime(
                sourceLocalTime,
                sourceTimeZone,
                localTimeZone);

            int id = reminderRepository.InsertReminder(Context.User.Id, Context.Channel.Id, targetReminderTime, text);
            await RespondAsync(
                $"I will remind you of this on {targetReminderTime:yyyy-MM-dd HH:mm} ({localTimeZone.StandardName}). " +
                $"If you want to delete it in the future, use `/deletereminder {id}`."
            );
        }

        private async void CheckForReminders(object sender, ElapsedEventArgs e)
        {
            using var scope = _serviceProvider.CreateScope();
            var reminderRepository = scope.ServiceProvider.GetRequiredService<IReminderRepository>();
            var reminders = reminderRepository.CheckForReminders();
            foreach (var reminder in reminders)
            {
                var channel = _client.GetChannel(reminder.ChannelID) as IMessageChannel;
                await channel.SendMessageAsync($"{MentionUtils.MentionUser(reminder.AuthorID)} you wanted to be reminded of : \"{reminder.ReminderText}\"");
                reminderRepository.SetReminderDone(reminder.ReminderID);
            }
          //  scope.Dispose();
        }
    }
}