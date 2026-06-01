using TurnosDesk.Api.Domain.Enums;
using TurnosDesk.Api.DTOs.Branches;
using TurnosDesk.Api.Support.Pagination;
using TurnosDesk.Api.Support.Responses;

namespace TurnosDesk.Api.Services.Interfaces;

public interface IBranchService
{
    Task<PagedResponse<BranchResponse>> GetPagedAsync(PaginationParams paginationParams, BranchStatus? status);

    Task<BranchResponse?> GetByIdAsync(int id);

    Task<ApiResponse<BranchResponse>> CreateAsync(CreateBranchRequest request);

    Task<ApiResponse<BranchResponse>> UpdateAsync(int id, UpdateBranchRequest request);

    Task<ApiResponse<BranchResponse>> ChangeStatusAsync(int id, BranchStatus status);
}
