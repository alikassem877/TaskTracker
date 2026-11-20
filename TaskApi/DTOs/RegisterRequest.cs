namespace TaskApi.DTOs;

public record RegisterRequest(
    string Email,
    string Password
);