using System.ComponentModel.DataAnnotations;

namespace LinkUp.Models.Auth;

/// <summary>
/// Modèle pour la mise à jour du profil
/// </summary>
public class UpdateProfileRequest
{
    public string Email { get; set; }
    public bool? IsPrivate { get; set; }
}