using System;
using static BaliBotDotNet.Modules.ReminderModule;

namespace BaliBotDotNet.Utilities.ExtensionMethods
{
    public static class ReminderTimezoneExtensions
    {
        public static TimeZoneInfo ToTimeZoneInfo(this ReminderTimezones timezone)
        {
            return timezone switch
            {
                ReminderTimezones.EST =>
                    TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"),

                ReminderTimezones.PST =>
                    TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"),

                ReminderTimezones.CST =>
                    TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"),

                ReminderTimezones.CEST =>
                    TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"),

                ReminderTimezones.BST or ReminderTimezones.GMT =>
                    TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time"),


                _ => throw new ArgumentOutOfRangeException(
                    nameof(timezone),
                    timezone,
                    "Unsupported timezone.")
            };
        }
    }
}
