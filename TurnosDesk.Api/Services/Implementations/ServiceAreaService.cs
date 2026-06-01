using Microsoft.EntityFrameworkCore;
using TurnosDesk.Api.Data;
using TurnosDesk.Api.Domain.Entities;
using TurnosDesk.Api.Domain.Enums;
using TurnosDesk.Api.DTOs.ServiceAreas;
using TurnosDesk.Api.Services.Interfaces;
using TurnosDesk.Api.Support.Pagination;
using TurnosDesk.Api.Support.Responses;

namespace TurnosDesk.Api.Services.Implementations;

public sealed class ServiceAreaService : IServiceAreaService
{
    private readonly TurnosDeskDbContext _dbContext;

    public ServiceAreaService(TurnosDeskDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResponse<ServiceAreaResponse>> GetPagedAsync(
        PaginationParams paginationParams,
        int? branchId,
        ServiceAreaStatus? status
    )
    {
        var query = _dbContext.ServiceAreas
            .AsNoTracking()
            .Include(area => area.Branch)
            .AsQueryable();

        if (branchId.HasValue)
        {
            query = query.Where(area => area.BranchId == branchId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(area => area.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(paginationParams.Search))
        {
            var search = paginationParams.Search.Trim();

            query = query.Where(area =>
                area.Code.Contains(search) ||
                area.Name.Contains(search) ||
                (area.Description != null && area.Description.Contains(search)) ||
                (area.Branch != null && area.Branch.Name.Contains(search))
            );
        }

        var totalItems = await query.CountAsync();

        var items = await query
            .OrderBy(area => area.Branch!.Name)
            .ThenBy(area => area.Name)
            .Skip(paginationParams.Skip)
            .Take(paginationParams.PageSize)
            .Select(area => ToResponse(area))
            .ToListAsync();

        return PagedResponse<ServiceAreaResponse>.Create(
            items,
            paginationParams.Page,
            paginationParams.PageSize,
            totalItems
        );
    }

    public async Task<ServiceAreaResponse?> GetByIdAsync(int id)
    {
        var area = await _dbContext.ServiceAreas
            .AsNoTracking()
            .Include(item => item.Branch)
            .FirstOrDefaultAsync(item => item.Id == id);

        return area is null ? null : ToResponse(area);
    }

    public async Task<ApiResponse<ServiceAreaResponse>> CreateAsync(CreateServiceAreaRequest request)
    {
        var branchExists = await _dbContext.Branches
            .AnyAsync(branch => branch.Id == request.BranchId);

        if (!branchExists)
        {
            return ApiResponse<ServiceAreaResponse>.Fail(
                "No se pudo crear el área de atención.",
                new[] { "La sucursal indicada no existe." }
            );
        }

        var normalizedCode = NormalizeCode(request.Code);
        var normalizedName = NormalizeText(request.Name);

        var codeExists = await _dbContext.ServiceAreas
            .AnyAsync(area =>
                area.BranchId == request.BranchId &&
                area.Code == normalizedCode
            );

        if (codeExists)
        {
            return ApiResponse<ServiceAreaResponse>.Fail(
                "No se pudo crear el área de atención.",
                new[] { "Ya existe un área con el mismo código dentro de la sucursal." }
            );
        }

        var area = new ServiceArea
        {
            BranchId = request.BranchId,
            Code = normalizedCode,
            Name = normalizedName,
            Description = NormalizeOptionalText(request.Description),
            Status = ServiceAreaStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.ServiceAreas.Add(area);
        await _dbContext.SaveChangesAsync();

        var createdArea = await _dbContext.ServiceAreas
            .AsNoTracking()
            .Include(item => item.Branch)
            .FirstAsync(item => item.Id == area.Id);

        return ApiResponse<ServiceAreaResponse>.Ok(
            ToResponse(createdArea),
            "Área de atención creada correctamente."
        );
    }

    public async Task<ApiResponse<ServiceAreaResponse>> UpdateAsync(int id, UpdateServiceAreaRequest request)
    {
        var area = await _dbContext.ServiceAreas
            .Include(item => item.Branch)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (area is null)
        {
            return ApiResponse<ServiceAreaResponse>.Fail(
                "No se pudo actualizar el área de atención.",
                new[] { "El área de atención solicitada no existe." }
            );
        }

        var branchExists = await _dbContext.Branches
            .AnyAsync(branch => branch.Id == request.BranchId);

        if (!branchExists)
        {
            return ApiResponse<ServiceAreaResponse>.Fail(
                "No se pudo actualizar el área de atención.",
                new[] { "La sucursal indicada no existe." }
            );
        }

        var normalizedCode = NormalizeCode(request.Code);
        var normalizedName = NormalizeText(request.Name);

        var codeExists = await _dbContext.ServiceAreas
            .AnyAsync(item =>
                item.Id != id &&
                item.BranchId == request.BranchId &&
                item.Code == normalizedCode
            );

        if (codeExists)
        {
            return ApiResponse<ServiceAreaResponse>.Fail(
                "No se pudo actualizar el área de atención.",
                new[] { "Ya existe otra área con el mismo código dentro de la sucursal." }
            );
        }

        area.BranchId = request.BranchId;
        area.Code = normalizedCode;
        area.Name = normalizedName;
        area.Description = NormalizeOptionalText(request.Description);
        area.Status = request.Status;
        area.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();

        var updatedArea = await _dbContext.ServiceAreas
            .AsNoTracking()
            .Include(item => item.Branch)
            .FirstAsync(item => item.Id == area.Id);

        return ApiResponse<ServiceAreaResponse>.Ok(
            ToResponse(updatedArea),
            "Área de atención actualizada correctamente."
        );
    }

    public async Task<ApiResponse<ServiceAreaResponse>> ChangeStatusAsync(int id, ServiceAreaStatus status)
    {
        var area = await _dbContext.ServiceAreas
            .Include(item => item.Branch)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (area is null)
        {
            return ApiResponse<ServiceAreaResponse>.Fail(
                "No se pudo cambiar el estado del área de atención.",
                new[] { "El área de atención solicitada no existe." }
            );
        }

        area.Status = status;
        area.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();

        return ApiResponse<ServiceAreaResponse>.Ok(
            ToResponse(area),
            status == ServiceAreaStatus.Active
                ? "Área de atención activada correctamente."
                : "Área de atención desactivada correctamente."
        );
    }

    private static ServiceAreaResponse ToResponse(ServiceArea area)
    {
        return new ServiceAreaResponse(
            area.Id,
            area.BranchId,
            area.Branch?.Name ?? "Sucursal no disponible",
            area.Code,
            area.Name,
            area.Description,
            area.Status,
            area.CreatedAt,
            area.UpdatedAt
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
