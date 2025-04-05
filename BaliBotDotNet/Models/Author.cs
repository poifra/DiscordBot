using System;
using System.Collections.Generic;

namespace BaliBotDotNet.Models;

public class Author
{
    public ulong AuthorID { get; set; }

    public string Username { get; set; }

    public bool IsQuotable { get; set; }

    public virtual ICollection<AlternativeFact> AlternativeFacts { get; set; } = new List<AlternativeFact>();

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}
