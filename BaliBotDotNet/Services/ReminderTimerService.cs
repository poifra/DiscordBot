using BaliBotDotNet.Data.Interfaces;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System.Timers;

namespace BaliBotDotNet.Services
{
    public class ReminderTimerService(DiscordSocketClient client, IServiceScopeFactory scopeFactory)
    {
        private readonly DiscordSocketClient _client = client;
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;

        public void Start()
        {
            Timer t = new(1000 * 60);
            t.Elapsed += CheckForReminders;
            t.Start();
        }

        private async void CheckForReminders(object sender, ElapsedEventArgs e)
        {
            using var scope = _scopeFactory.CreateScope();
            var reminderRepository = scope.ServiceProvider.GetRequiredService<IReminderRepository>();
            var reminders = reminderRepository.CheckForReminders();
            foreach (var reminder in reminders)
            {
                var channel = _client.GetChannel(reminder.ChannelID) as IMessageChannel;
                await channel.SendMessageAsync($"{MentionUtils.MentionUser(reminder.AuthorID)} you wanted to be reminded of : \"{reminder.ReminderText}\"");
                reminderRepository.SetReminderDone(reminder.ReminderID);
            }
        }
    }
}
