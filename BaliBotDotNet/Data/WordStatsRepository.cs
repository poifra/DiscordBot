using BaliBotDotNet.Data.Interfaces;
using BaliBotDotNet.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace BaliBotDotNet.Data
{
    public class WordStatsRepository(BaliBotDbContext dbContext) : IWordStatsRepository
    {
        private readonly BaliBotDbContext _db = dbContext;

        public GuildLanguageModel GetLanguageModel(ulong guildID)
        {
            return _db.GuildLanguageModels.AsNoTracking().FirstOrDefault(x => x.GuildID == guildID);
        }

        public void SaveLanguageModel(GuildLanguageModel model)
        {
            var existing = _db.GuildLanguageModels.FirstOrDefault(x => x.GuildID == model.GuildID);
            if (existing == null)
            {
                _db.GuildLanguageModels.Add(model);
            }
            else
            {
                existing.BuiltAt = model.BuiltAt;
                existing.Data = model.Data;
            }
            _db.SaveChanges();
        }

        public void DeleteLanguageModel(ulong guildID)
        {
            var existing = _db.GuildLanguageModels.FirstOrDefault(x => x.GuildID == guildID);
            if (existing != null)
            {
                _db.GuildLanguageModels.Remove(existing);
                _db.SaveChanges();
            }
        }
    }
}
