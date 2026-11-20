namespace TaskApi.DTOs;

public record LoginRequest(
    string Email,
    string Password
);