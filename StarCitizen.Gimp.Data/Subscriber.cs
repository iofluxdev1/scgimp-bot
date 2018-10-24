using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StarCitizen.Gimp.Data
{
    public class Subscriber
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SubscriberId { get; set; }

        [Required]
        [MaxLength(250)]
        [EmailAddress]
        [RegularExpression(@"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$")]
        public string Email { get; set; }

        [Required]
        public DateTimeOffset CreatedAt { get; set; }

        [Required]
        public DateTimeOffset UpdatedAt { get; set; }

        public DateTimeOffset? DeletedAt { get; set; }

        public ICollection<SubscriberAudit> SubscriberAudits { get; set; }
    }
}
