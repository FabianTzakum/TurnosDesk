namespace TurnosDesk.Api.Support.Pagination;

public class PaginationParams
{
    private const int MaxPageSize = 100;
    private int _page = 1;
    private int _pageSize = 10;

    public int Page
    {
        get => _page;
        set => _page = value <= 0 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set
        {
            if (value <= 0)
            {
                _pageSize = 10;
                return;
            }

            _pageSize = value > MaxPageSize ? MaxPageSize : value;
        }
    }

    public string? Search { get; set; }

    public int Skip => (Page - 1) * PageSize;
}
