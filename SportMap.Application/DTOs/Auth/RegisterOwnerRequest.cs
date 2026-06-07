namespace SportMap.Application.DTOs.Auth;

public class RegisterOwnerRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    // الكود السري اللي بتديه لصاحب الملعب
    public string InviteCode { get; set; } = string.Empty;
}