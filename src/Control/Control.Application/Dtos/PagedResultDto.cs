namespace Control.Application.Dtos;

public record PagedResultDto<T>(IReadOnlyCollection<T> Items, int PageNumber, int PageSize, int TotalCount);
