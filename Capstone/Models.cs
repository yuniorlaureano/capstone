public record UserDto(string UserName, string Email, string Password, string Role);
public record LoginDto(string UserName, string Password);

public record RoleAssignDto(string Role);