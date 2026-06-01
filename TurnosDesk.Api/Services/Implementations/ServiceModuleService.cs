using Microsoft.EntityFrameworkCore;
using TurnosDesk.Api.Data;
using TurnosDesk.Api.Domain.Entities;
using TurnosDesk.Api.Domain.Enums;
using TurnosDesk.Api.DTOs.ServiceModules;
using TurnosDesk.Api.Services.Interfaces;
using TurnosDesk.Api.Support.Pagination;
using TurnosDesk.Api.Support.Responses;

namespace TurnosDesk.Api.Services.Implementations;

public sealed class ServiceModuleService : IServiceModuleService
{
    private readonly TurnosDeskDbContext _dbContext;

    public ServiceModuleService(TurnosDeskDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResponse<ServiceModuleResponse>> GetPagedAsync(
        PaginationParams paginationParams,
        int? branchId,
        int? serviceAreaId,
        ServiceModuleType? type,
        ServiceModuleStatus? status
    )
    {
        var query = _dbContext.ServiceModules
            .AsNoTracking()
            .Include(module => module.Branch)
            .Include(module => module.ServiceArea)
            .AsQueryable();

        if (branchId.HasValue)
        {
            query = query.Where(module => module.BranchId == branchId.Value);
        }

        if (serviceAreaId.HasValue)
        {
            query = query.Where(module => module.ServiceAreaId == serviceAreaId.Value);
        }

        if (type.HasValue)
        {
            query = query.Where(module => module.Type == type.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(module => module.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(paginationParams.Search))
        {
            var search = paginationParams.Search.Trim();

            query = query.Where(module =>
                module.Code.Contains(search) ||
                module.Name.Contains(search) ||
                (module.Description != null && module.Description.Contains(search)) ||
                (module.Branch != null && module.Branch.Name.Contains(search)) ||
                (module.ServiceArea != null && module.ServiceArea.Name.Contains(search))
            );
        }

        var totalItems = await query.CountAsync();

        var items = await query
            .OrderBy(module => module.Branch!.Name)
            .ThenBy(module => module.ServiceArea != null ? module.ServiceArea.Name : string.Empty)
            .ThenBy(module => module.Name)
            .Skip(paginationParams.Skip)
            .Take(paginationParams.PageSize)
            .Select(module => ToResponse(module))
            .ToListAsync();

        return PagedResponse<ServiceModuleResponse>.Create(
            items,
            paginationParams.Page,
            paginationParams.PageSize,
            totalItems
        );
    }

    public async Task<ServiceModuleResponse?> GetByIdAsync(int id)
    {
        var module = await _dbContext.ServiceModules
            .AsNoTracking()
            .Include(item => item.Branch)
            .Include(item => item.ServiceArea)
            .FirstOrDefaultAsync(item => item.Id == id);

        return module is null ? null : ToResponse(module);
    }

    public async Task<ApiResponse<ServiceModuleResponse>> CreateAsync(CreateServiceModuleRequest request)
    {
        var branchExists = await _dbContext.Branches
            .AnyAsync(branch => branch.Id == request.BranchId);

        if (!branchExists)
        {
            return ApiResponse<ServiceModuleResponse>.Fail(
                "No se pudo crear el módulo de atención.",
                new[] { "La sucursal indicada no existe." }
            );
        }

        if (request.ServiceAreaId.HasValue)
        {
            var areaBelongsToBranch = await _dbContext.ServiceAreas
                .AnyAsync(area =>
                    area.Id == request.ServiceAreaId.Value &&
                    area.BranchId == request.BranchId
                );

            if (!areaBelongsToBranch)
            {
                return ApiResponse<ServiceModuleResponse>.Fail(
                    "No se pudo crear el módulo de atención.",
                    new[] { "El área indicada no existe o no pertenece a la sucursal seleccionada." }
                );
            }
        }

        var normalizedCode = NormalizeCode(request.Code);
        var normalizedName = NormalizeText(request.Name);

        var codeExists = await _dbContext.ServiceModules
            .AnyAsync(module =>
                module.BranchId == request.BranchId &&
                module.Code == normalizedCode
            );

        if (codeExists)
        {
            return ApiResponse<ServiceModuleResponse>.Fail(
                "No se pudo crear el módulo de atención.",
                new[] { "Ya existe un módulo con el mismo código dentro de la sucursal." }
            );
        }

        var module = new ServiceModule
        {
            BranchId = request.BranchId,
            ServiceAreaId = request.ServiceAreaId,
            Code = normalizedCode,
            Name = normalizedName,
            Type = request.Type,
            Status = ServiceModuleStatus.Active,
            Description = NormalizeOptionalText(request.Description),
            CreatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.ServiceModules.Add(module);
        await _dbContext.SaveChangesAsync();

        var createdModule = await _dbContext.ServiceModules
            .AsNoTracking()
            .Include(item => item.Branch)
            .Include(item => item.ServiceArea)
            .FirstAsync(item => item.Id == module.Id);

        return ApiResponse<ServiceModuleResponse>.Ok(
            ToResponse(createdModule),
            "Módulo de atención creado correctamente."
        );
    }

    public async Task<ApiResponse<ServiceModuleResponse>> UpdateAsync(int id, UpdateServiceModuleRequest request)
    {
        var module = await _dbContext.ServiceModules
            .Include(item => item.Branch)
            .Include(item => item.ServiceArea)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (module is null)
        {
            return ApiResponse<ServiceModuleResponse>.Fail(
                "No se pudo actualizar el módulo de atención.",
                new[] { "El módulo de atención solicitado no existe." }
            );
        }

        var branchExists = await _dbContext.Branches
            .AnyAsync(branch => branch.Id == request.BranchId);

        if (!branchExists)
        {
            return ApiResponse<ServiceModuleResponse>.Fail(
                "No se pudo actualizar el módulo de atención.",
                new[] { "La sucursal indicada no existe." }
            );
        }

        if (request.ServiceAreaId.HasValue)
        {
            var areaBelongsToBranch = await _dbContext.ServiceAreas
                .AnyAsync(area =>
                    area.Id == request.ServiceAreaId.Value &&
                    area.BranchId == request.BranchId
                );

            if (!areaBelongsToBranch)
            {
                return ApiResponse<ServiceModuleResponse>.Fail(
                    "No se pudo actualizar el módulo de atención.",
                    new[] { "El área indicada no existe o no pertenece a la sucursal seleccionada." }
                );
            }
        }

        var normalizedCode = NormalizeCode(request.Code);
        var normalizedName = NormalizeText(request.Name);

        var codeExists = await _dbContext.ServiceModules
            .AnyAsync(item =>
                item.Id != id &&
                item.BranchId == request.BranchId &&
                item.Code == normalizedCode
            );

        if (codeExists)
        {
            return ApiResponse<ServiceModuleResponse>.Fail(
                "No se pudo actualizar el módulo de atención.",
                new[] { "Ya existe otro módulo con el mismo código dentro de la sucursal." }
            );
        }

        module.BranchId = request.BranchId;
        module.ServiceAreaId = request.ServiceAreaId;
        module.Code = normalizedCode;
        module.Name = normalizedName;
        module.Type = request.Type;
        module.Status = request.Status;
        module.Description = NormalizeOptionalText(request.Description);
        module.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();

        var updatedModule = await _dbContext.ServiceModules
            .AsNoTracking()
            .Include(item => item.Branch)
            .Include(item => item.ServiceArea)
            .FirstAsync(item => item.Id == module.Id);

        return ApiResponse<ServiceModuleResponse>.Ok(
            ToResponse(updatedModule),
            "Módulo de atención actualizado correctamente."
        );
    }

    public async Task<ApiResponse<ServiceModuleResponse>> ChangeStatusAsync(int id, ServiceModuleStatus status)
    {
        var module = await _dbContext.ServiceModules
            .Include(item => item.Branch)
            .Include(item => item.ServiceArea)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (module is null)
        {
            return ApiResponse<ServiceModuleResponse>.Fail(
                "No se pudo cambiar el estado del módulo de atención.",
                new[] { "El módulo de atención solicitado no existe." }
            );
        }

        module.Status = status;
        module.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();

        return ApiResponse<ServiceModuleResponse>.Ok(
            ToResponse(module),
            $"Estado del módulo actualizado a {status}."
        );
    }

    private static ServiceModuleResponse ToResponse(ServiceModule module)
    {
        return new ServiceModuleResponse(
            module.Id,
            module.BranchId,
            module.Branch?.Name ?? "Sucursal no disponible",
            module.ServiceAreaId,
            module.ServiceArea?.Name,
            module.Code,
            module.Name,
            module.Type,
            module.Status,
            module.Description,
            module.CreatedAt,
            module.UpdatedAt
        );
    }

    private static string NormalizeCode(string value)
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
