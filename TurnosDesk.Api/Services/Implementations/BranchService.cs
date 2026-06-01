using Microsoft.EntityFrameworkCore;
using TurnosDesk.Api.Data;
using TurnosDesk.Api.Domain.Entities;
using TurnosDesk.Api.Domain.Enums;
using TurnosDesk.Api.DTOs.Branches;
using TurnosDesk.Api.Services.Interfaces;
using TurnosDesk.Api.Support.Pagination;
using TurnosDesk.Api.Support.Responses;

namespace TurnosDesk.Api.Services.Implementations;

public sealed class BranchService : IBranchService
{
    private readonly TurnosDeskDbContext _dbContext;

    public BranchService(TurnosDeskDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResponse<BranchResponse>> GetPagedAsync(
        PaginationParams paginationParams,
        BranchStatus? status
    )
    {
        var query = _dbContext.Branches
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(paginationParams.Search))
        {
            var search = paginationParams.Search.Trim();

            query = query.Where(branch =>
                branch.Code.Contains(search) ||
                branch.Name.Contains(search) ||
                (branch.Address != null && branch.Address.Contains(search)) ||
                (branch.PhoneNumber != null && branch.PhoneNumber.Contains(search))
            );
        }

        if (status.HasValue)
        {
            query = query.Where(branch => branch.Status == status.Value);
        }

        var totalItems = await query.CountAsync();

        var items = await query
            .OrderBy(branch => branch.Name)
            .Skip(paginationParams.Skip)
            .Take(paginationParams.PageSize)
            .Select(branch => ToResponse(branch))
            .ToListAsync();

        return PagedResponse<BranchResponse>.Create(
            items,
            paginationParams.Page,
            paginationParams.PageSize,
            totalItems
        );
    }

    public async Task<BranchResponse?> GetByIdAsync(int id)
    {
        var branch = await _dbContext.Branches
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id);

        return branch is null ? null : ToResponse(branch);
    }

    public async Task<ApiResponse<BranchResponse>> CreateAsync(CreateBranchRequest request)
    {
        var normalizedCode = NormalizeCode(request.Code);
        var normalizedName = NormalizeText(request.Name);

        var codeExists = await _dbContext.Branches
            .AnyAsync(branch => branch.Code == normalizedCode);

        if (codeExists)
        {
            return ApiResponse<BranchResponse>.Fail(
                "No se pudo crear la sucursal.",
                new[] { "Ya existe una sucursal con el mismo código." }
            );
        }

        var branch = new Branch
        {
            Code = normalizedCode,
            Name = normalizedName,
            Address = NormalizeOptionalText(request.Address),
            PhoneNumber = NormalizeOptionalText(request.PhoneNumber),
            Status = BranchStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.Branches.Add(branch);
        await _dbContext.SaveChangesAsync();

        return ApiResponse<BranchResponse>.Ok(
            ToResponse(branch),
            "Sucursal creada correctamente."
        );
    }

    public async Task<ApiResponse<BranchResponse>> UpdateAsync(int id, UpdateBranchRequest request)
    {
        var branch = await _dbContext.Branches
            .FirstOrDefaultAsync(item => item.Id == id);

        if (branch is null)
        {
            return ApiResponse<BranchResponse>.Fail(
                "No se pudo actualizar la sucursal.",
                new[] { "La sucursal solicitada no existe." }
            );
        }

        var normalizedCode = NormalizeCode(request.Code);
        var normalizedName = NormalizeText(request.Name);

        var codeExists = await _dbContext.Branches
            .AnyAsync(item => item.Id != id && item.Code == normalizedCode);

        if (codeExists)
        {
            return ApiResponse<BranchResponse>.Fail(
                "No se pudo actualizar la sucursal.",
                new[] { "Ya existe otra sucursal con el mismo código." }
            );
        }

        branch.Code = normalizedCode;
        branch.Name = normalizedName;
        branch.Address = NormalizeOptionalText(request.Address);
        branch.PhoneNumber = NormalizeOptionalText(request.PhoneNumber);
        branch.Status = request.Status;
        branch.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();

        return ApiResponse<BranchResponse>.Ok(
            ToResponse(branch),
            "Sucursal actualizada correctamente."
        );
    }

    public async Task<ApiResponse<BranchResponse>> ChangeStatusAsync(int id, BranchStatus status)
    {
        var branch = await _dbContext.Branches
            .FirstOrDefaultAsync(item => item.Id == id);

        if (branch is null)
        {
            return ApiResponse<BranchResponse>.Fail(
                "No se pudo cambiar el estado de la sucursal.",
                new[] { "La sucursal solicitada no existe." }
            );
        }

        branch.Status = status;
        branch.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();

        return ApiResponse<BranchResponse>.Ok(
            ToResponse(branch),
            status == BranchStatus.Active
                ? "Sucursal activada correctamente."
                : "Sucursal desactivada correctamente."
        );
    }

    private static BranchResponse ToResponse(Branch branch)
    {
        return new BranchResponse(
            branch.Id,
            branch.Code,
            branch.Name,
            branch.Address,
            branch.PhoneNumber,
            branch.Status,
            branch.CreatedAt,
            branch.UpdatedAt
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
