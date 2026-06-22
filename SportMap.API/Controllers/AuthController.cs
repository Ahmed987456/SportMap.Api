using Microsoft.AspNetCore.Mvc;
using SportMap.Application.DTOs.Auth;
using SportMap.Application.DTOs.Common;
using SportMap.Application.Interfaces;

namespace SportMap.API.Controllers;

/// <summary>
/// 🔓 كل الـ Endpoints دي متاحة للكل بدون Login
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// 🔓 متاح للكل — تسجيل لاعب جديد
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.RegisterAsync(request);

        return Ok(ApiResponse<AuthResponse>.Ok(result, "Registered successfully"));
    }

    /// <summary>
    /// 🔓 متاح للكل — تسجيل صاحب ملعب جديد (محتاج Invite Code من الأدمن)
    /// </summary>
    [HttpPost("register-owner")]
    public async Task<IActionResult> RegisterOwner(RegisterOwnerRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.RegisterOwnerAsync(request);

        return Ok(ApiResponse<AuthResponse>.Ok(result, "Owner registered successfully"));
    }

    /// <summary>
    /// 🔓 متاح للكل — تسجيل دخول لأي مستخدم
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        return Ok(ApiResponse<AuthResponse>.Ok(result, "Logged in successfully"));
    }

    /// <summary>
    /// 🔓 متاح للكل — تجديد الـ Access Token بالـ Refresh Token
    /// </summary>
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
    {
        var result = await _authService.RefreshTokenAsync(refreshToken);
        return Ok(ApiResponse<AuthResponse>.Ok(result));
    }

    /// <summary>
    /// 🔓 متاح للكل — تسجيل خروج وإلغاء الـ Refresh Token
    /// </summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] string refreshToken)
    {
        await _authService.LogoutAsync(refreshToken);
        return Ok(ApiResponse<object>.Ok(null!, "Logged out successfully"));
    }
}