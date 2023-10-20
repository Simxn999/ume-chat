using Microsoft.EntityFrameworkCore;
using Ume_Chat_Models.Data.FeedbackData;
using Ume_Chat_Utilities;

namespace Ume_Chat_Data_Feedback;

public class FeedbackContext : DbContext
{
    public FeedbackContext() { }

    public FeedbackContext(DbContextOptions<FeedbackContext> options) : base(options) { }

    public virtual DbSet<Category> Categories { get; set; } = default!;

    public virtual DbSet<Citation> Citations { get; set; } = default!;

    public virtual DbSet<FeedbackSubmission> FeedbackSubmissions { get; set; } = default!;

    public virtual DbSet<Message> Messages { get; set; } = default!;

    public virtual DbSet<Status> Statuses { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        InitializeStructure(modelBuilder);
        InitializeSeedData(modelBuilder);
    }

    /// <summary>
    ///     Initialize structure of database.
    /// </summary>
    /// <param name="modelBuilder">ModelBuilder</param>
    private static void InitializeStructure(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.Property(e => e.ID).ValueGeneratedOnAdd();
            entity.Property(e => e.Description).HasMaxLength(1024);
            entity.Property(e => e.Title).HasMaxLength(128);
        });

        modelBuilder.Entity<Citation>(entity =>
        {
            entity.Property(e => e.ID).ValueGeneratedNever();
            entity.Property(e => e.TextID).HasMaxLength(6).IsUnicode(false);

            entity.HasOne(d => d.Message)
                  .WithMany(p => p.Citations)
                  .HasForeignKey(d => d.MessageID)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("FK_Citations_Message");
        });

        modelBuilder.Entity<FeedbackSubmission>(entity =>
        {
            entity.Property(e => e.ID).ValueGeneratedNever();
            entity.Property(e => e.Date).HasPrecision(0);
            entity.Property(e => e.Title).HasMaxLength(128);

            entity.HasOne(d => d.Status)
                  .WithMany(p => p.FeedbackSubmissions)
                  .HasForeignKey(d => d.StatusID)
                  .OnDelete(DeleteBehavior.ClientSetNull)
                  .HasConstraintName("FK_FeedbackSubmissions_Statuses");

            entity.HasMany(d => d.Categories)
                  .WithMany(p => p.FeedbackSubmissions)
                  .UsingEntity<Dictionary<string, object>>("FeedbackSubmissionCategory",
                                                           r => r.HasOne<Category>()
                                                                 .WithMany()
                                                                 .HasForeignKey("CategoryID")
                                                                 .OnDelete(DeleteBehavior.ClientSetNull)
                                                                 .HasConstraintName("FK_FeedbackSubmissionCategories_Categories"),
                                                           l => l.HasOne<FeedbackSubmission>()
                                                                 .WithMany()
                                                                 .HasForeignKey("FeedbackSubmissionID")
                                                                 .OnDelete(DeleteBehavior.Cascade)
                                                                 .HasConstraintName("FK_FeedbackSubmissionCategories_FeedbackSubmissions"),
                                                           j =>
                                                           {
                                                               j.HasKey("FeedbackSubmissionID", "CategoryID");
                                                               j.ToTable("FeedbackSubmissionCategories");
                                                               j.IndexerProperty<Guid>("FeedbackSubmissionID");
                                                               j.IndexerProperty<int>("CategoryID");
                                                           });
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.Property(e => e.ID).ValueGeneratedNever();
            entity.Property(e => e.Role).HasMaxLength(10);

            entity.HasOne(d => d.FeedbackSubmission)
                  .WithMany(p => p.Messages)
                  .HasForeignKey(d => d.FeedbackSubmissionID)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("FK_Messages_FeedbackSubmissions");
        });

        modelBuilder.Entity<Status>(entity =>
        {
            entity.Property(e => e.ID).ValueGeneratedOnAdd();
            entity.Property(e => e.Title).HasMaxLength(64);
        });
    }

    /// <summary>
    ///     Initialize default data.
    /// </summary>
    /// <param name="modelBuilder">ModelBuilder</param>
    private static void InitializeSeedData(ModelBuilder modelBuilder)
    {
        var statuses = DataParser.LoadJson<List<Status>>("Data/Statuses.json");
        modelBuilder.Entity<Status>().HasData(statuses);

        var categories = DataParser.LoadJson<List<Category>>("Data/Categories.json");
        modelBuilder.Entity<Category>().HasData(categories);
    }
}
