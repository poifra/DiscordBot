using System;

namespace BaliBotDotNet.Models;

public class GuildLanguageModel
{
    public ulong GuildID { get; set; }

    public DateTime BuiltAt { get; set; }

    public byte[] Data { get; set; }
}
