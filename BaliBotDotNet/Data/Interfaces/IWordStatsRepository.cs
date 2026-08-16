using BaliBotDotNet.Models;

namespace BaliBotDotNet.Data.Interfaces
{
    public interface IWordStatsRepository
    {
        GuildLanguageModel GetLanguageModel(ulong guildID);
        void SaveLanguageModel(GuildLanguageModel model);
        void DeleteLanguageModel(ulong guildID);
    }
}
