using Microsoft.EntityFrameworkCore;
using TurnosDesk.Api.Domain.Entities;
using TurnosDesk.Api.Domain.Enums;

namespace TurnosDesk.Api.Data;

public class TurnosDeskDbContext : DbContext
{
    public TurnosDeskDbContext(DbContextOptions<TurnosDeskDbContext> options)
        : base(options)
    {
    }

    public DbSet<Branch> Branches => Set<Branch>();

    public DbSet<ServiceArea> ServiceAreas => Set<ServiceArea>();

    public DbSet<ServiceModule> ServiceModules => Set<ServiceModule>();

    public DbSet<ServiceType> ServiceTypes => Set<ServiceType>();

    public DbSet<QueueTicket> QueueTickets => Set<QueueTicket>();

    public DbSet<TicketEvent> TicketEvents => Set<TicketEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureBranches(modelBuilder);
        ConfigureServiceAreas(modelBuilder);
        ConfigureServiceModules(modelBuilder);
        ConfigureServiceTypes(modelBuilder);
        ConfigureQueueTickets(modelBuilder);
        ConfigureTicketEvents(modelBuilder);
    }

    private static void ConfigureBranches(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Branch>(entity =>
        {
            entity.ToTable("Branches");

            entity.HasKey(branch => branch.Id);

            entity.Property(branch => branch.Code)
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(branch => branch.Name)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(branch => branch.Address)
                .HasMaxLength(250);

            entity.Property(branch => branch.PhoneNumber)
                .HasMaxLength(30);

            entity.Property(branch => branch.Status)
                .HasConversion<string>()
                .HasMaxLength(30)
                .HasDefaultValue(BranchStatus.Active)
                .IsRequired();

            entity.Property(branch => branch.CreatedAt)
                .IsRequired();

            entity.Property(branch => branch.UpdatedAt);

            entity.HasIndex(branch => branch.Code)
                .IsUnique();

            entity.HasIndex(branch => branch.Name);
        });
    }

    private static void ConfigureServiceAreas(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ServiceArea>(entity =>
        {
            entity.ToTable("ServiceAreas");

            entity.HasKey(area => area.Id);

            entity.Property(area => area.Code)
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(area => area.Name)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(area => area.Description)
                .HasMaxLength(300);

            entity.Property(area => area.Status)
                .HasConversion<string>()
                .HasMaxLength(30)
                .HasDefaultValue(ServiceAreaStatus.Active)
                .IsRequired();

            entity.Property(area => area.CreatedAt)
                .IsRequired();

            entity.Property(area => area.UpdatedAt);

            entity.HasOne(area => area.Branch)
                .WithMany(branch => branch.ServiceAreas)
                .HasForeignKey(area => area.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(area => new { area.BranchId, area.Code })
                .IsUnique();

            entity.HasIndex(area => new { area.BranchId, area.Name });
        });
    }

    private static void ConfigureServiceModules(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ServiceModule>(entity =>
        {
            entity.ToTable("ServiceModules");

            entity.HasKey(module => module.Id);

            entity.Property(module => module.Code)
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(module => module.Name)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(module => module.Type)
                .HasConversion<string>()
                .HasMaxLength(30)
                .HasDefaultValue(ServiceModuleType.Window)
                .IsRequired();

            entity.Property(module => module.Status)
                .HasConversion<string>()
                .HasMaxLength(30)
                .HasDefaultValue(ServiceModuleStatus.Active)
                .IsRequired();

            entity.Property(module => module.Description)
                .HasMaxLength(300);

            entity.Property(module => module.CreatedAt)
                .IsRequired();

            entity.Property(module => module.UpdatedAt);

            entity.HasOne(module => module.Branch)
                .WithMany(branch => branch.ServiceModules)
                .HasForeignKey(module => module.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(module => module.ServiceArea)
                .WithMany(area => area.ServiceModules)
                .HasForeignKey(module => module.ServiceAreaId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(module => new { module.BranchId, module.Code })
                .IsUnique();

            entity.HasIndex(module => new { module.BranchId, module.Name });
        });
    }

    private static void ConfigureServiceTypes(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ServiceType>(entity =>
        {
            entity.ToTable("ServiceTypes");

            entity.HasKey(serviceType => serviceType.Id);

            entity.Property(serviceType => serviceType.Code)
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(serviceType => serviceType.Prefix)
                .HasMaxLength(5)
                .IsRequired();

            entity.Property(serviceType => serviceType.Name)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(serviceType => serviceType.Description)
                .HasMaxLength(300);

            entity.Property(serviceType => serviceType.EstimatedMinutes)
                .HasDefaultValue(10)
                .IsRequired();

            entity.Property(serviceType => serviceType.Priority)
                .HasConversion<string>()
                .HasMaxLength(30)
                .HasDefaultValue(ServicePriority.Normal)
                .IsRequired();

            entity.Property(serviceType => serviceType.Status)
                .HasConversion<string>()
                .HasMaxLength(30)
                .HasDefaultValue(ServiceTypeStatus.Active)
                .IsRequired();

            entity.Property(serviceType => serviceType.CreatedAt)
                .IsRequired();

            entity.Property(serviceType => serviceType.UpdatedAt);

            entity.HasOne(serviceType => serviceType.ServiceArea)
                .WithMany(area => area.ServiceTypes)
                .HasForeignKey(serviceType => serviceType.ServiceAreaId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(serviceType => serviceType.Code)
                .IsUnique();

            entity.HasIndex(serviceType => serviceType.Name);
        });
    }

    private static void ConfigureQueueTickets(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<QueueTicket>(entity =>
        {
            entity.ToTable("QueueTickets");

            entity.HasKey(ticket => ticket.Id);

            entity.Property(ticket => ticket.Folio)
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(ticket => ticket.DailySequence)
                .IsRequired();

            entity.Property(ticket => ticket.Status)
                .HasConversion<string>()
                .HasMaxLength(30)
                .HasDefaultValue(QueueTicketStatus.Pending)
                .IsRequired();

            entity.Property(ticket => ticket.CustomerName)
                .HasMaxLength(150);

            entity.Property(ticket => ticket.CustomerReference)
                .HasMaxLength(100);

            entity.Property(ticket => ticket.Notes)
                .HasMaxLength(500);

            entity.Property(ticket => ticket.ServiceDate)
                .IsRequired();

            entity.Property(ticket => ticket.CreatedAt)
                .IsRequired();

            entity.Property(ticket => ticket.CalledAt);

            entity.Property(ticket => ticket.ServiceStartedAt);

            entity.Property(ticket => ticket.ServiceCompletedAt);

            entity.Property(ticket => ticket.CancelledAt);

            entity.Property(ticket => ticket.NoShowAt);

            entity.HasOne(ticket => ticket.Branch)
                .WithMany(branch => branch.QueueTickets)
                .HasForeignKey(ticket => ticket.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(ticket => ticket.ServiceType)
                .WithMany(serviceType => serviceType.QueueTickets)
                .HasForeignKey(ticket => ticket.ServiceTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(ticket => ticket.ServiceModule)
                .WithMany(module => module.QueueTickets)
                .HasForeignKey(ticket => ticket.ServiceModuleId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(ticket => new { ticket.BranchId, ticket.ServiceDate, ticket.DailySequence })
                .IsUnique();

            entity.HasIndex(ticket => new { ticket.BranchId, ticket.ServiceDate, ticket.Folio })
                .IsUnique();

            entity.HasIndex(ticket => new { ticket.BranchId, ticket.ServiceDate, ticket.Status });
        });
    }

    private static void ConfigureTicketEvents(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TicketEvent>(entity =>
        {
            entity.ToTable("TicketEvents");

            entity.HasKey(ticketEvent => ticketEvent.Id);

            entity.Property(ticketEvent => ticketEvent.EventType)
                .HasConversion<string>()
                .HasMaxLength(40)
                .IsRequired();

            entity.Property(ticketEvent => ticketEvent.Description)
                .HasMaxLength(300)
                .IsRequired();

            entity.Property(ticketEvent => ticketEvent.CreatedAt)
                .IsRequired();

            entity.Property(ticketEvent => ticketEvent.CreatedBy)
                .HasMaxLength(100);

            entity.HasOne(ticketEvent => ticketEvent.QueueTicket)
                .WithMany(ticket => ticket.Events)
                .HasForeignKey(ticketEvent => ticketEvent.QueueTicketId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ticketEvent => ticketEvent.ServiceModule)
                .WithMany(module => module.TicketEvents)
                .HasForeignKey(ticketEvent => ticketEvent.ServiceModuleId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(ticketEvent => new { ticketEvent.QueueTicketId, ticketEvent.CreatedAt });
        });
    }
}
