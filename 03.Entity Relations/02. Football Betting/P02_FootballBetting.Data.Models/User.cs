using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using P02_FootballBetting.Data.Common;

namespace P02_FootballBetting.Data.Models;

public class User
{
    public User()
    {
        this.Bets = new HashSet<Bet>();
    }

    [Key]
    public int UserId { get; set; }

    [Required]
    [MaxLength(ValidationConstants.UserUsernameMaxLength)]
    public string Username { get; set; } = null!;

    [Required]
    [MaxLength(ValidationConstants.UserPasswordLength)]
    public string Password { get; set; } = null!;

    [Required]
    [MaxLength(ValidationConstants.UserEmailMaxLength)]
    public string Email { get; set; } = null!;

    [Required]
    [MaxLength(ValidationConstants.UserNameMaxLength)]
    public string Name { get; set; }

    public decimal Balance { get; set; }

    public virtual ICollection<Bet> Bets { get; set; }


}

