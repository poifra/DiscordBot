using Dapper;
using Discord;

namespace BaliBotDotNet.Model
{
    public class MessageRepository : SqlLiteBaseRepository
    {
        public MessageRepository() : base()
        {

        }

        public void InsertMessage(IMessage discordMessage)
        {
            using var con = SqlLiteConnexion();
            con.Open();
            con.Query<long>(@"INSERT INTO Message");
        }
    }
}
