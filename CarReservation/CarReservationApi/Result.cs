namespace CarReservationApi;

public record Result (Status Status, string? Message = null);
public record Result<T> (Status Status, T? Value = default, string? Message = null);

public enum Status
{
    Success,
    ValidationFailed,
    NotFound,
    Conflict
}
