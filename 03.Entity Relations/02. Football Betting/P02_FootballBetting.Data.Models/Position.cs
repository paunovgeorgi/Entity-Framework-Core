using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using P02_FootballBetting.Data.Common;

namespace P02_FootballBetting.Data.Models;

public class Position
{
    public Position()
    {
        this.Players = new HashSet<Player>();
    }

    [Key]
    public int PositionId { get; set; }

    [Required]
    [MaxLength(ValidationConstants.PositionMaxLength)]
    public string Name { get; } = null!;

    public virtual ICollection<Player> Players { get; set; }
}

