using LinkUp.Models.Auth;
using System.ComponentModel.DataAnnotations;

namespace LinkUp.Models;

public class Subscription
{
    public int Id { get; set; }
    public string SubscriberId { get; set; }
    public string SubscribedId { get; set; }
    
    public ApplicationUser Subscriber { get; set; }
    public ApplicationUser Subscribed { get; set; }
}
