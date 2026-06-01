using TurnosDesk.Api.Domain.Enums;
using TurnosDesk.Api.DTOs.ServiceTypes;
using TurnosDesk.Api.Support.Pagination;
using TurnosDesk.Api.Support.Responses;

namespace TurnosDesk.Api.Services.Interfaces;

public interface IServiceTypeService
{
    Task<PagedResponse<ServiceTypeResponse>> GetPagedAsync(
        PaginationParams paginationParams,
        int? serviceAreaId,
        ServicePriority? priority,
        ServiceTypeStatus? status
    );

    Task<ServiceTypeResponse?> GetByIdAsync(int id);

    Task<ApiResponse<ServiceTypeResponse>> CreateAsync(CreateServiceTypeRequest request);

    Task<ApiResponse<ServiceTypeResponse>> UpdateAsync(int id, UpdateServiceTypeRequest request);

    Task<ApiResponse<ServiceTypeResponse>> ChangeStatusAsync(int id, ServiceTypeStatus status);
}
