using LinkUp.Models.Auth;
using LinkUp.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using LinkUp.Models;

namespace LinkUp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class SubscriptionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SubscriptionController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Abonne un utilisateur à un autre utilisateur.
        /// </summary>
        /// <param name="subscribedUserId">L'ID de l'utilisateur auquel s'abonner.</param>
        /// <returns>Message de succès ou d'erreur.</returns>
        [HttpPost("subscribe/{subscribedUserId}")]
        public async Task<IActionResult> Subscribe(string subscribedUserId)
        {
            var subscriber = await _userManager.GetUserAsync(User);
            if (subscriber == null)
            {
                return Unauthorized("Utilisateur non authentifié");
            }

            var subscribedUser = await _userManager.FindByIdAsync(subscribedUserId);
            if (subscribedUser == null)
            {
                return NotFound("Utilisateur cible non trouvé");
            }
            
            var existingSubscription = await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.SubscriberId == subscriber.Id && s.SubscribedId == subscribedUser.Id);
            if (existingSubscription != null)
            {
                return BadRequest("Vous êtes déjà abonné à cet utilisateur");
            }

            var subscription = new Subscription
            {
                SubscriberId = subscriber.Id,
                SubscribedId = subscribedUser.Id
            };

            _context.Subscriptions.Add(subscription);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Abonnement réussi" });
        }

        /// <summary>
        /// Désabonne un utilisateur d'un autre utilisateur.
        /// </summary>
        /// <param name="subscribedUserId">L'ID de l'utilisateur à se désabonner.</param>
        /// <returns>Message de succès ou d'erreur.</returns>
        [HttpDelete("unsubscribe/{subscribedUserId}")]
        public async Task<IActionResult> Unsubscribe(string subscribedUserId)
        {
            var subscriber = await _userManager.GetUserAsync(User);
            if (subscriber == null)
            {
                return Unauthorized("Utilisateur non authentifié");
            }

            var subscribedUser = await _userManager.FindByIdAsync(subscribedUserId);
            if (subscribedUser == null)
            {
                return NotFound("Utilisateur cible non trouvé");
            }

            var subscription = await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.SubscriberId == subscriber.Id && s.SubscribedId == subscribedUser.Id);

            if (subscription == null)
            {
                return NotFound("Vous n'êtes pas abonné à cet utilisateur");
            }

            _context.Subscriptions.Remove(subscription);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Désabonnement réussi" });
        }

        /// <summary>
        /// Vérifie si un utilisateur est abonné à un autre utilisateur.
        /// </summary>
        /// <param name="subscribedUserId">L'ID de l'utilisateur à vérifier.</param>
        /// <returns>Résultat de l'abonnement.</returns>
        [HttpGet("is-subscribed/{subscribedUserId}")]
        public async Task<IActionResult> IsSubscribed(string subscribedUserId)
        {
            var subscriber = await _userManager.GetUserAsync(User);
            if (subscriber == null)
            {
                return Unauthorized("Utilisateur non authentifié");
            }

            var subscribedUser = await _userManager.FindByIdAsync(subscribedUserId);
            if (subscribedUser == null)
            {
                return NotFound("Utilisateur cible non trouvé");
            }

            var isSubscribed = await _context.Subscriptions
                .AnyAsync(s => s.SubscriberId == subscriber.Id && s.SubscribedId == subscribedUser.Id);

            return Ok(new { IsSubscribed = isSubscribed });
        }

        /// <summary>
        /// Récupère la liste des abonnements de l'utilisateur authentifié.
        /// </summary>
        /// <returns>Liste des abonnements de l'utilisateur.</returns>
        [HttpGet("subscriptions")]
        public async Task<IActionResult> GetSubscriptions()
        {
            var subscriber = await _userManager.GetUserAsync(User);
            if (subscriber == null)
            {
                return Unauthorized("Utilisateur non authentifié");
            }

            var subscriptions = await _context.Subscriptions
                .Where(s => s.SubscriberId == subscriber.Id)
                .Include(s => s.Subscribed)
                .ToListAsync();

            var result = subscriptions.Select(s => new 
            {
                Username = s.Subscribed.UserName,
                IsPrivate = s.Subscribed.IsPrivate,
            });

            return Ok(result);
        }

    }
}
