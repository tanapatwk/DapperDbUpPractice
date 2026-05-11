namespace TaskAPI.DTO;

public record TaskResponse(
    int Id,
    string Title,
    bool IsDone,
    string CreatedAt,
    string? CategoryName
);