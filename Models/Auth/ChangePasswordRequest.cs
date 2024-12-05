using System.ComponentModel.DataAnnotations;

namespace LinkUp.Models.Auth;

/// <summary>
/// Modèle pour mettre à jour son mot de passe
/// </summary>
public class ChangePasswordRequest
{
    public string OldPassword { get; set; }
    public string NewPassword { get; set; }
}