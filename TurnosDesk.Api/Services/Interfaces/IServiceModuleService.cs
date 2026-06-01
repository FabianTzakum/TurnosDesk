using TurnosDesk.Api.Domain.Enums;
using TurnosDesk.Api.DTOs.ServiceModules;
using TurnosDesk.Api.Support.Pagination;
using TurnosDesk.Api.Support.Responses;

namespace TurnosDesk.Api.Services.Interfaces;

public interface IServiceModuleService
{
    Task<PagedResponse<ServiceModuleResponse>> GetPagedAsync(
        PaginationParams paginationParams,
        int? branchId,
        int? serviceAreaId,
        ServiceModuleType? type,
        ServiceModuleStatus? status
    );

    Task<ServiceModuleResponse?> GetByIdAsync(int id);

    Task<ApiResponse<ServiceModuleResponse>> CreateAsync(CreateServiceModuleRequest request);

    Task<ApiResponse<ServiceModuleResponse>> UpdateAsync(int id, UpdateServiceModuleRequest request);

    Task<ApiResponse<ServiceModuleResponse>> ChangeStatusAsync(int id, ServiceModuleStatus status);
}
