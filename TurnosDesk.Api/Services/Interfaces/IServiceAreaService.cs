using TurnosDesk.Api.Domain.Enums;
using TurnosDesk.Api.DTOs.ServiceAreas;
using TurnosDesk.Api.Support.Pagination;
using TurnosDesk.Api.Support.Responses;

namespace TurnosDesk.Api.Services.Interfaces;

public interface IServiceAreaService
{
    Task<PagedResponse<ServiceAreaResponse>> GetPagedAsync(
        PaginationParams paginationParams,
        int? branchId,
        ServiceAreaStatus? status
    );

    Task<ServiceAreaResponse?> GetByIdAsync(int id);

    Task<ApiResponse<ServiceAreaResponse>> CreateAsync(CreateServiceAreaRequest request);

    Task<ApiResponse<ServiceAreaResponse>> UpdateAsync(int id, UpdateServiceAreaRequest request);

    Task<ApiResponse<ServiceAreaResponse>> ChangeStatusAsync(int id, ServiceAreaStatus status);
}
