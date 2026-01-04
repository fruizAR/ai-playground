using Microsoft.EntityFrameworkCore;
using GrapeAI.Models;

public partial class AppDBContext : DbContext
{
    public DbSet<SystemParameter> SystemsParameters { get; set; }
    public DbSet<SystemIntegration> SystemsIntegrations { get; set; }
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<BotConversation> BotConversations { get; set; }
    public AppDBContext(DbContextOptions options) : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Evitar delete en cascada
        foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
        {
            relationship.DeleteBehavior = DeleteBehavior.Restrict;
        }

        // #region Auth
        // modelBuilder.Entity<User>(entity =>
        // {
        //     entity.HasIndex(e => e.Identifier).IsUnique();
        //     entity.HasIndex(e => e.ExternalID);
        // });
        // #endregion

        // #region Enums
        // modelBuilder
        //     .Entity<ConvenioCobro>()
        //     .Property(e => e.FormaPago)
        //     .HasConversion<string>();
        // #endregion

        #region Views
        // modelBuilder.Entity<EstadoFinancieroResult>(e => e.ToView("EstadoFinanciero").HasNoKey());
        // modelBuilder.Entity<CuentaCorrienteResult>(e => e.ToView("CuentaCorriente").HasNoKey());
        #endregion

        // Índices para BotConversation (coinciden con el SQL provisto)
        modelBuilder.Entity<BotConversation>(entity =>
        {
            entity.HasIndex(e => e.CreatedAt).HasDatabaseName("IX_bot_conversation_created_at");
            entity.HasIndex(e => e.Role).HasDatabaseName("IX_bot_conversation_role");
            entity.HasIndex(e => e.ThreadId).HasDatabaseName("IX_bot_conversation_threadid");
            entity.HasIndex(e => e.MessageId).HasDatabaseName("IX_bot_conversation_messageid");
        });
    }


}