using LinkUp.Models.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LinkUp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
    }

    /// <summary>
    /// Création d'un compte utilisateur
    /// </summary>
    /// <returns>Compte utilisateur crée</returns>
    /// <response code="200">Retourne le compte utilisateur</response>
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest model)
    {
        var user = new ApplicationUser
        {
            UserName = model.Username,
            Email = model.Email,
            IsPrivate = model.IsPrivate
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            return Ok(new { Message = "User registered successfully" });
        }

        return BadRequest(result.Errors);
    }

    /// <summary>
    /// Connexion d'un compte utilisateur
    /// </summary>
    /// <returns>Compte utilisateur connecté</returns>
    /// <response code="200">Retourne les informations de connexion</response>
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            return BadRequest("Invalid email or password");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
        if (!result.Succeeded)
        {
            return BadRequest("Invalid email or password");
        }

        var token = GenerateJwtToken(user);
        return Ok(new { Token = token });
    }

    private string GenerateJwtToken(ApplicationUser user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWTKey"]));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtIssuer"],
            audience: _configuration["JwtAudience"],
            claims: claims,
            expires: DateTime.Now.AddHours(3),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    /// <summary>
    /// Mise à jour des informations de profil de l'utilisateur
    /// </summary>
    /// <param name="model">Modèle contenant les informations à mettre à jour</param>
    /// <returns>Retourne un message de succès ou d'échec</returns>
    /// <response code="200">Profil mis à jour avec succès</response>
    /// <response code="400">Échec de la mise à jour du profil</response>
    [HttpPut("update-profile")]
    public async Task<IActionResult> UpdateProfile(UpdateProfileRequest model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized("Utilisateur non authentifié");
        }
        
        if (!string.IsNullOrEmpty(model.Email) && model.Email != user.Email)
        {
            user.Email = model.Email;
        }

        if (model.IsPrivate != null && model.IsPrivate != user.IsPrivate)
        {
            user.IsPrivate = model.IsPrivate.Value;
        }

        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            return Ok(new { Message = "Profil mis à jour avec succès" });
        }

        return BadRequest(result.Errors);
    }
    
    /// <summary>
    /// Récupère les informations de l'utilisateur connecté
    /// </summary>
    /// <returns>Informations de l'utilisateur connecté</returns>
    /// <response code="200">Retourne les informations de l'utilisateur</response>
    /// <response code="401">Utilisateur non authentifié</response>
    [HttpGet("user-info")]
    public async Task<IActionResult> GetUserInfo()
    {
        var user = await _userManager.GetUserAsync(User); // Récupère l'utilisateur courant à partir du JWT
        if (user == null)
        {
            return Unauthorized("Utilisateur non authentifié");
        }

        var userInfo = new
        {
            user.Id,
            user.UserName,
            user.Email,
            user.IsPrivate
        };

        return Ok(userInfo);
    }
    
    /// <summary>
    /// Modifie le mot de passe de l'utilisateur connecté
    /// </summary>
    /// <param name="model">Modèle contenant l'ancien mot de passe et le nouveau mot de passe</param>
    /// <returns>Message de succès ou d'échec</returns>
    /// <response code="200">Mot de passe modifié avec succès</response>
    /// <response code="400">Échec de la modification du mot de passe</response>
    /// <response code="401">Utilisateur non authentifié</response>
    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized("Utilisateur non authentifié");
        }
        
        var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
        if (result.Succeeded)
        {
            return Ok(new { Message = "Mot de passe modifié avec succès" });
        }
        
        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
        return BadRequest(new { Message = "Erreur lors du changement de mot de passe", Errors = errors });
    }
    
}