using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BaliBotDotNet.Models;

public partial class BaliBotDbContext : DbContext
{
    public BaliBotDbContext()
    {
    }

    public BaliBotDbContext(DbContextOptions<BaliBotDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AlternativeFact> AlternativeFacts { get; set; }

    public virtual DbSet<Author> Authors { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<Reminder> Reminders { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlite("Data Source=F:\\Users\\Francois\\Source\\Repos\\BaliBotDotNet\\BaliBotDotNet\\bin\\Debug\\net8.0\\BaliBotDB.sqlite");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AlternativeFact>(entity =>
        {
            entity.ToTable("AlternativeFact");

            entity.Property(e => e.AlternativeFactID).ValueGeneratedNever();
            entity.Property(e => e.AuthorID).HasColumnName("AuthorID");
            entity.Property(e => e.Description).IsRequired();

            entity.HasOne(d => d.Author).WithMany(p => p.AlternativeFacts)
                .HasForeignKey(d => d.AuthorID)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Author>(entity =>
        {
            entity.ToTable("Author");

            entity.Property(e => e.AuthorID)
                .ValueGeneratedNever()
                .HasColumnName("AuthorID");
            entity.Property(e => e.IsQuotable)
                .HasDefaultValue(true)
                .HasColumnType("bit");
            entity.Property(e => e.Username).IsRequired();
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.ToTable("Message");

            entity.Property(e => e.MessageID)
                .ValueGeneratedNever()
                .HasColumnName("MessageID");
            entity.Property(e => e.AuthorID).HasColumnName("AuthorID");
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.DateSent).IsRequired();
            entity.Property(e => e.GuildID).HasColumnName("GuildID");

            entity.HasOne(d => d.Author).WithMany(p => p.Messages)
                .HasForeignKey(d => d.AuthorID)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Reminder>(entity =>
        {
            entity.ToTable("Reminder");

            entity.Property(e => e.ReminderID)
                .ValueGeneratedOnAdd()
                .HasColumnName("ReminderID");
            entity.Property(e => e.AuthorID).HasColumnName("AuthorID");
            entity.Property(e => e.ChannelID).HasColumnName("ChannelID");
            entity.Property(e => e.ReminderText).IsRequired();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
