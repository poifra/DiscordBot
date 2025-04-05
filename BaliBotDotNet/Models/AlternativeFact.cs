using System;
using System.Collections.Generic;

namespace BaliBotDotNet.Models;

public class AlternativeFact
{
    public int AlternativeFactID { get; set; }

    public string Description { get; set; }

    public ulong AuthorID { get; set; }

    public virtual Author Author { get; set; }
}
