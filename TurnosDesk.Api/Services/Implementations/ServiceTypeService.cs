using Microsoft.EntityFrameworkCore;
using TurnosDesk.Api.Data;
using TurnosDesk.Api.Domain.Entities;
using TurnosDesk.Api.Domain.Enums;
using TurnosDesk.Api.DTOs.ServiceTypes;
using TurnosDesk.Api.Services.Interfaces;
using TurnosDesk.Api.Support.Pagination;
using TurnosDesk.Api.Support.Responses;

namespace TurnosDesk.Api.Services.Implementations;

public sealed class ServiceTypeService : IServiceTypeService
{
    private readonly TurnosDeskDbContext _dbContext;

    public ServiceTypeService(TurnosDeskDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResponse<ServiceTypeResponse>> GetPagedAsync(
        PaginationParams paginationParams,
        int? serviceAreaId,
        ServicePriority? priority,
        ServiceTypeStatus? status
    )
    {
        var query = _dbContext.ServiceTypes
            .AsNoTracking()
            .Include(serviceType => serviceType.ServiceArea)
            .AsQueryable();

        if (serviceAreaId.HasValue)
        {
            query = query.Where(serviceType => serviceType.ServiceAreaId == serviceAreaId.Value);
        }

        if (priority.HasValue)
        {
            query = query.Where(serviceType => serviceType.Priority == priority.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(serviceType => serviceType.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(paginationParams.Search))
        {
            var search = paginationParams.Search.Trim();

            query = query.Where(serviceType =>
                serviceType.Code.Contains(search) ||
                serviceType.Prefix.Contains(search) ||
                serviceType.Name.Contains(search) ||
                (serviceType.Description != null && serviceType.Description.Contains(search)) ||
                (serviceType.ServiceArea != null && serviceType.ServiceArea.Name.Contains(search))
            );
        }

        var totalItems = await query.CountAsync();

        var items = await query
            .OrderBy(serviceType => serviceType.ServiceArea != null ? serviceType.ServiceArea.Name : string.Empty)
            .ThenBy(serviceType => serviceType.Name)
            .Skip(paginationParams.Skip)
            .Take(paginationParams.PageSize)
            .Select(serviceType => ToResponse(serviceType))
            .ToListAsync();

        return PagedResponse<ServiceTypeResponse>.Create(
            items,
            paginationParams.Page,
            paginationParams.PageSize,
            totalItems
        );
    }

    public async Task<ServiceTypeResponse?> GetByIdAsync(int id)
    {
        var serviceType = await _dbContext.ServiceTypes
            .AsNoTracking()
            .Include(item => item.ServiceArea)
            .FirstOrDefaultAsync(item => item.Id == id);

        return serviceType is null ? null : ToResponse(serviceType);
    }

    public async Task<ApiResponse<ServiceTypeResponse>> CreateAsync(CreateServiceTypeRequest request)
    {
        if (request.ServiceAreaId.HasValue)
        {
            var areaExists = await _dbContext.ServiceAreas
                .AnyAsync(area => area.Id == request.ServiceAreaId.Value);

            if (!areaExists)
            {
                return ApiResponse<ServiceTypeResponse>.Fail(
                    "No se pudo crear el servicio.",
                    new[] { "El área de atención indicada no existe." }
                );
            }
        }

        var normalizedCode = NormalizeCode(request.Code);
        var normalizedPrefix = NormalizePrefix(request.Prefix);
        var normalizedName = NormalizeText(request.Name);

        var codeExists = await _dbContext.ServiceTypes
            .AnyAsync(serviceType => serviceType.Code == normalizedCode);

        if (codeExists)
        {
            return ApiResponse<ServiceTypeResponse>.Fail(
                "No se pudo crear el servicio.",
                new[] { "Ya existe un servicio con el mismo código." }
            );
        }

        var prefixExists = await _dbContext.ServiceTypes
            .AnyAsync(serviceType => serviceType.Prefix == normalizedPrefix);

        if (prefixExists)
        {
            return ApiResponse<ServiceTypeResponse>.Fail(
                "No se pudo crear el servicio.",
                new[] { "Ya existe un servicio con el mismo prefijo para generación de folios." }
            );
        }

        var serviceType = new ServiceType
        {
            ServiceAreaId = request.ServiceAreaId,
            Code = normalizedCode,
            Prefix = normalizedPrefix,
            Name = normalizedName,
            Description = NormalizeOptionalText(request.Description),
            EstimatedMinutes = request.EstimatedMinutes,
            Priority = request.Priority,
            Status = ServiceTypeStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.ServiceTypes.Add(serviceType);
        await _dbContext.SaveChangesAsync();

        var createdServiceType = await _dbContext.ServiceTypes
            .AsNoTracking()
            .Include(item => item.ServiceArea)
            .FirstAsync(item => item.Id == serviceType.Id);

        return ApiResponse<ServiceTypeResponse>.Ok(
            ToResponse(createdServiceType),
            "Servicio creado correctamente."
        );
    }

    public async Task<ApiResponse<ServiceTypeResponse>> UpdateAsync(int id, UpdateServiceTypeRequest request)
    {
        var serviceType = await _dbContext.ServiceTypes
            .Include(item => item.ServiceArea)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (serviceType is null)
        {
            return ApiResponse<ServiceTypeResponse>.Fail(
                "No se pudo actualizar el servicio.",
                new[] { "El servicio solicitado no existe." }
            );
        }

        if (request.ServiceAreaId.HasValue)
        {
            var areaExists = await _dbContext.ServiceAreas
                .AnyAsync(area => area.Id == request.ServiceAreaId.Value);

            if (!areaExists)
            {
                return ApiResponse<ServiceTypeResponse>.Fail(
                    "No se pudo actualizar el servicio.",
                    new[] { "El área de atención indicada no existe." }
                );
            }
        }

        var normalizedCode = NormalizeCode(request.Code);
        var normalizedPrefix = NormalizePrefix(request.Prefix);
        var normalizedName = NormalizeText(request.Name);

        var codeExists = await _dbContext.ServiceTypes
            .AnyAsync(item => item.Id != id && item.Code == normalizedCode);

        if (codeExists)
        {
            return ApiResponse<ServiceTypeResponse>.Fail(
                "No se pudo actualizar el servicio.",
                new[] { "Ya existe otro servicio con el mismo código." }
            );
        }

        var prefixExists = await _dbContext.ServiceTypes
            .AnyAsync(item => item.Id != id && item.Prefix == normalizedPrefix);

        if (prefixExists)
        {
            return ApiResponse<ServiceTypeResponse>.Fail(
                "No se pudo actualizar el servicio.",
                new[] { "Ya existe otro servicio con el mismo prefijo para generación de folios." }
            );
        }

        serviceType.ServiceAreaId = request.ServiceAreaId;
        serviceType.Code = normalizedCode;
        serviceType.Prefix = normalizedPrefix;
        serviceType.Name = normalizedName;
        serviceType.Description = NormalizeOptionalText(request.Description);
        serviceType.EstimatedMinutes = request.EstimatedMinutes;
        serviceType.Priority = request.Priority;
        serviceType.Status = request.Status;
        serviceType.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();

        var updatedServiceType = await _dbContext.ServiceTypes
            .AsNoTracking()
            .Include(item => item.ServiceArea)
            .FirstAsync(item => item.Id == serviceType.Id);

        return ApiResponse<ServiceTypeResponse>.Ok(
            ToResponse(updatedServiceType),
            "Servicio actualizado correctamente."
        );
    }

    public async Task<ApiResponse<ServiceTypeResponse>> ChangeStatusAsync(int id, ServiceTypeStatus status)
    {
        var serviceType = await _dbContext.ServiceTypes
            .Include(item => item.ServiceArea)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (serviceType is null)
        {
            return ApiResponse<ServiceTypeResponse>.Fail(
                "No se pudo cambiar el estado del servicio.",
                new[] { "El servicio solicitado no existe." }
            );
        }

        serviceType.Status = status;
        serviceType.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();

        return ApiResponse<ServiceTypeResponse>.Ok(
            ToResponse(serviceType),
            status == ServiceTypeStatus.Active
                ? "Servicio activado correctamente."
                : "Servicio desactivado correctamente."
        );
    }

    private static ServiceTypeResponse ToResponse(ServiceType serviceType)
    {
        return new ServiceTypeResponse(
            serviceType.Id,
            serviceType.ServiceAreaId,
            serviceType.ServiceArea?.Name,
            serviceType.Code,
            serviceType.Prefix,
            serviceType.Name,
            serviceType.Description,
            serviceType.EstimatedMinutes,
            serviceType.Priority,
            serviceType.Status,
            serviceType.CreatedAt,
            serviceType.UpdatedAt
        );
    }

    private static string NormalizeCode(string value)
    {
        return value.Trim().ToUpperInvariant();
    }

    private static string NormalizePrefix(string value)
    {
        return value.Trim().ToUpperInvariant();
    }

    private static string NormalizeText(string value)
    {
        return value.Trim();
    }

    private static string? NormalizeOptionalText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }
}
