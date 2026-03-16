using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApp.Domain.Entities;
using WebApp.Infrastructure.Domain.Entities;

namespace WebApp.Infrastructure.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<IdentityUser>(options)
{
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<ProfileImage> ProfileImages => Set<ProfileImage>();
    public DbSet<Like> Likes => Set<Like>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<GameInvite> GameInvites => Set<GameInvite>();
    public DbSet<GameSession> GameSessions => Set<GameSession>();
    public DbSet<Block> Blocks => Set<Block>();
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<DailyActionCount> DailyActionCounts => Set<DailyActionCount>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ── UserProfile ──────────────────────────────────────────────────────
        builder.Entity<UserProfile>(e =>
        {
            e.HasOne(p => p.User)
             .WithOne()
             .HasForeignKey<UserProfile>(p => p.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            e.Property(p => p.Status)
             .HasConversion<string>();
        });

        // ── ProfileImage ─────────────────────────────────────────────────────
        builder.Entity<ProfileImage>(e =>
        {
            e.HasOne(i => i.User)
             .WithMany()
             .HasForeignKey(i => i.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            // Max 6 images enforced in application layer (FR-15)
        });

        // ── Like ─────────────────────────────────────────────────────────────
        builder.Entity<Like>(e =>
        {
            // A user can only like/dislike another user once
            e.HasIndex(l => new { l.SenderId, l.ReceiverId }).IsUnique();

            e.HasOne(l => l.Sender)
             .WithMany()
             .HasForeignKey(l => l.SenderId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(l => l.Receiver)
             .WithMany()
             .HasForeignKey(l => l.ReceiverId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Match ─────────────────────────────────────────────────────────────
        builder.Entity<Match>(e =>
        {
            // No duplicate matches (FR-22)
            e.HasIndex(m => new { m.User1Id, m.User2Id }).IsUnique();

            e.HasOne(m => m.User1)
             .WithMany()
             .HasForeignKey(m => m.User1Id)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(m => m.User2)
             .WithMany()
             .HasForeignKey(m => m.User2Id)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(m => m.Conversation)
             .WithOne(c => c.Match)
             .HasForeignKey<Conversation>(c => c.MatchId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Message ───────────────────────────────────────────────────────────
        builder.Entity<Message>(e =>
        {
            // Exactly one of ConversationId or GameSessionId must be set —
            // enforced as a check constraint
            e.ToTable(t => t.HasCheckConstraint(
                "CK_Message_Context",
                "(\"ConversationId\" IS NOT NULL AND \"GameSessionId\" IS NULL) OR " +
                "(\"ConversationId\" IS NULL AND \"GameSessionId\" IS NOT NULL)"));

            e.HasOne(m => m.Conversation)
             .WithMany(c => c.Messages)
             .HasForeignKey(m => m.ConversationId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(m => m.GameSession)
             .WithMany(s => s.Messages)
             .HasForeignKey(m => m.GameSessionId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(m => m.Sender)
             .WithMany()
             .HasForeignKey(m => m.SenderId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── GameInvite ────────────────────────────────────────────────────────
        builder.Entity<GameInvite>(e =>
        {
            e.Property(i => i.Status).HasConversion<string>();

            e.HasOne(i => i.Sender)
             .WithMany()
             .HasForeignKey(i => i.SenderId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(i => i.Receiver)
             .WithMany()
             .HasForeignKey(i => i.ReceiverId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(i => i.GameSession)
             .WithOne(s => s.Invite)
             .HasForeignKey<GameSession>(s => s.InviteId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── GameSession ───────────────────────────────────────────────────────
        builder.Entity<GameSession>(e =>
        {
            e.Property(s => s.Status).HasConversion<string>();

            e.HasOne(s => s.Player1)
             .WithMany()
             .HasForeignKey(s => s.Player1Id)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(s => s.Player2)
             .WithMany()
             .HasForeignKey(s => s.Player2Id)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Block ─────────────────────────────────────────────────────────────
        builder.Entity<Block>(e =>
        {
            e.HasIndex(b => new { b.BlockerId, b.BlockedId }).IsUnique();

            e.HasOne(b => b.Blocker)
             .WithMany()
             .HasForeignKey(b => b.BlockerId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(b => b.Blocked)
             .WithMany()
             .HasForeignKey(b => b.BlockedId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Report ────────────────────────────────────────────────────────────
        builder.Entity<Report>(e =>
        {
            e.Property(r => r.Status).HasConversion<string>();

            e.HasOne(r => r.Reporter)
             .WithMany()
             .HasForeignKey(r => r.ReporterId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(r => r.Reported)
             .WithMany()
             .HasForeignKey(r => r.ReportedId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── DailyActionCount ──────────────────────────────────────────────────
        builder.Entity<DailyActionCount>(e =>
        {
            // One row per user + action + date
            e.HasIndex(d => new { d.UserId, d.ActionType, d.Date }).IsUnique();

            e.Property(d => d.ActionType).HasConversion<string>();

            e.HasOne(d => d.User)
             .WithMany()
             .HasForeignKey(d => d.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}