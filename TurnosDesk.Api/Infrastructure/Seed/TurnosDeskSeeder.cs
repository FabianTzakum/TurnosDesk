using Microsoft.EntityFrameworkCore;
using TurnosDesk.Api.Data;
using TurnosDesk.Api.Domain.Entities;
using TurnosDesk.Api.Domain.Enums;

namespace TurnosDesk.Api.Infrastructure.Seed;

public sealed class TurnosDeskSeeder
{
    private readonly TurnosDeskDbContext _dbContext;
    private readonly ILogger<TurnosDeskSeeder> _logger;

    public TurnosDeskSeeder(
        TurnosDeskDbContext dbContext,
        ILogger<TurnosDeskSeeder> logger
    )
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        var databaseReady = await EnsureDatabaseIsReadyAsync();

        if (!databaseReady)
        {
            return;
        }

        if (await _dbContext.Branches.AnyAsync())
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var branch = new Branch
        {
            Code = "MER-CENTRO",
            Name = "Sucursal Mérida Centro",
            Address = "Calle 60 Centro, Mérida, Yucatán",
            PhoneNumber = "9991234567",
            Status = BranchStatus.Active,
            CreatedAt = now
        };

        _dbContext.Branches.Add(branch);
        await _dbContext.SaveChangesAsync();

        var cashierArea = new ServiceArea
        {
            BranchId = branch.Id,
            Code = "CAJAS",
            Name = "Cajas",
            Description = "Área para pagos, cobros y operaciones rápidas.",
            Status = ServiceAreaStatus.Active,
            CreatedAt = now
        };

        var supportArea = new ServiceArea
        {
            BranchId = branch.Id,
            Code = "SOPORTE",
            Name = "Soporte",
            Description = "Área para resolución de incidencias y atención especializada.",
            Status = ServiceAreaStatus.Active,
            CreatedAt = now
        };

        var receptionArea = new ServiceArea
        {
            BranchId = branch.Id,
            Code = "RECEPCION",
            Name = "Recepción",
            Description = "Área para orientación inicial y canalización de clientes.",
            Status = ServiceAreaStatus.Active,
            CreatedAt = now
        };

        _dbContext.ServiceAreas.AddRange(cashierArea, supportArea, receptionArea);
        await _dbContext.SaveChangesAsync();

        var cashierModule = new ServiceModule
        {
            BranchId = branch.Id,
            ServiceAreaId = cashierArea.Id,
            Code = "C01",
            Name = "Caja 1",
            Type = ServiceModuleType.Cashier,
            Status = ServiceModuleStatus.Active,
            Description = "Caja principal para pagos y operaciones rápidas.",
            CreatedAt = now
        };

        var windowModule = new ServiceModule
        {
            BranchId = branch.Id,
            ServiceAreaId = receptionArea.Id,
            Code = "V01",
            Name = "Ventanilla 1",
            Type = ServiceModuleType.Window,
            Status = ServiceModuleStatus.Active,
            Description = "Ventanilla para atención general.",
            CreatedAt = now
        };

        var supportModule = new ServiceModule
        {
            BranchId = branch.Id,
            ServiceAreaId = supportArea.Id,
            Code = "S01",
            Name = "Módulo de soporte",
            Type = ServiceModuleType.SupportModule,
            Status = ServiceModuleStatus.Active,
            Description = "Módulo para casos de soporte o revisión especializada.",
            CreatedAt = now
        };

        _dbContext.ServiceModules.AddRange(cashierModule, windowModule, supportModule);
        await _dbContext.SaveChangesAsync();

        var paymentService = new ServiceType
        {
            ServiceAreaId = cashierArea.Id,
            Code = "PAGO",
            Prefix = "P",
            Name = "Pago de servicio",
            Description = "Atención para pagos, cobros y operaciones rápidas.",
            EstimatedMinutes = 8,
            Priority = ServicePriority.Normal,
            Status = ServiceTypeStatus.Active,
            CreatedAt = now
        };

        var supportService = new ServiceType
        {
            ServiceAreaId = supportArea.Id,
            Code = "SOPORTE",
            Prefix = "S",
            Name = "Soporte técnico",
            Description = "Atención especializada para incidencias o revisión de casos.",
            EstimatedMinutes = 20,
            Priority = ServicePriority.High,
            Status = ServiceTypeStatus.Active,
            CreatedAt = now
        };

        var informationService = new ServiceType
        {
            ServiceAreaId = receptionArea.Id,
            Code = "INFO",
            Prefix = "I",
            Name = "Información general",
            Description = "Orientación rápida para clientes o visitantes.",
            EstimatedMinutes = 5,
            Priority = ServicePriority.Low,
            Status = ServiceTypeStatus.Active,
            CreatedAt = now
        };

        _dbContext.ServiceTypes.AddRange(paymentService, supportService, informationService);
        await _dbContext.SaveChangesAsync();

        var pendingTicket = new QueueTicket
        {
            BranchId = branch.Id,
            ServiceTypeId = paymentService.Id,
            Folio = "P001",
            DailySequence = 1,
            Status = QueueTicketStatus.Pending,
            CustomerName = "Cliente demo pendiente",
            CustomerReference = "DEMO-001",
            Notes = "Turno demo generado por seed.",
            ServiceDate = today,
            CreatedAt = now.AddMinutes(-20)
        };

        var calledTicket = new QueueTicket
        {
            BranchId = branch.Id,
            ServiceTypeId = informationService.Id,
            ServiceModuleId = windowModule.Id,
            Folio = "I002",
            DailySequence = 2,
            Status = QueueTicketStatus.Called,
            CustomerName = "Cliente demo llamado",
            CustomerReference = "DEMO-002",
            Notes = "Turno demo llamado.",
            ServiceDate = today,
            CreatedAt = now.AddMinutes(-15),
            CalledAt = now.AddMinutes(-10)
        };

        var completedTicket = new QueueTicket
        {
            BranchId = branch.Id,
            ServiceTypeId = supportService.Id,
            ServiceModuleId = supportModule.Id,
            Folio = "S003",
            DailySequence = 3,
            Status = QueueTicketStatus.Completed,
            CustomerName = "Cliente demo atendido",
            CustomerReference = "DEMO-003",
            Notes = "Turno demo finalizado.",
            ServiceDate = today,
            CreatedAt = now.AddMinutes(-45),
            CalledAt = now.AddMinutes(-35),
            ServiceStartedAt = now.AddMinutes(-30),
            ServiceCompletedAt = now.AddMinutes(-12)
        };

        _dbContext.QueueTickets.AddRange(pendingTicket, calledTicket, completedTicket);
        await _dbContext.SaveChangesAsync();

        var events = new List<TicketEvent>
        {
            new()
            {
                QueueTicketId = pendingTicket.Id,
                EventType = TicketEventType.Created,
                Description = $"Turno {pendingTicket.Folio} generado correctamente.",
                CreatedAt = pendingTicket.CreatedAt,
                CreatedBy = "Sistema"
            },
            new()
            {
                QueueTicketId = calledTicket.Id,
                EventType = TicketEventType.Created,
                Description = $"Turno {calledTicket.Folio} generado correctamente.",
                CreatedAt = calledTicket.CreatedAt,
                CreatedBy = "Sistema"
            },
            new()
            {
                QueueTicketId = calledTicket.Id,
                ServiceModuleId = windowModule.Id,
                EventType = TicketEventType.Called,
                Description = $"Turno {calledTicket.Folio} llamado en {windowModule.Name}.",
                CreatedAt = calledTicket.CalledAt ?? now.AddMinutes(-10),
                CreatedBy = "Operador demo"
            },
            new()
            {
                QueueTicketId = completedTicket.Id,
                EventType = TicketEventType.Created,
                Description = $"Turno {completedTicket.Folio} generado correctamente.",
                CreatedAt = completedTicket.CreatedAt,
                CreatedBy = "Sistema"
            },
            new()
            {
                QueueTicketId = completedTicket.Id,
                ServiceModuleId = supportModule.Id,
                EventType = TicketEventType.Called,
                Description = $"Turno {completedTicket.Folio} llamado en {supportModule.Name}.",
                CreatedAt = completedTicket.CalledAt ?? now.AddMinutes(-35),
                CreatedBy = "Operador demo"
            },
            new()
            {
                QueueTicketId = completedTicket.Id,
                ServiceModuleId = supportModule.Id,
                EventType = TicketEventType.ServiceStarted,
                Description = $"Atención iniciada para el turno {completedTicket.Folio}.",
                CreatedAt = completedTicket.ServiceStartedAt ?? now.AddMinutes(-30),
                CreatedBy = "Operador demo"
            },
            new()
            {
                QueueTicketId = completedTicket.Id,
                ServiceModuleId = supportModule.Id,
                EventType = TicketEventType.ServiceCompleted,
                Description = $"Atención finalizada para el turno {completedTicket.Folio}.",
                CreatedAt = completedTicket.ServiceCompletedAt ?? now.AddMinutes(-12),
                CreatedBy = "Operador demo"
            }
        };

        _dbContext.TicketEvents.AddRange(events);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Datos demo de TurnosDesk cargados correctamente.");
    }

    private async Task<bool> EnsureDatabaseIsReadyAsync()
    {
        try
        {
            return await _dbContext.Database.CanConnectAsync();
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "No se pudieron cargar los datos demo porque la base de datos no está disponible."
            );

            return false;
        }
    }
}
