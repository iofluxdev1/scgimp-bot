using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StarCitizen.Gimp.Data
{
    public class SubscriberAudit
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SubscriberAuditId { get; set; }

        [ForeignKey("Subscriber")]
        [Required]
        public long SubscriberId { get; set; }

        public Subscriber Subscriber { get; set; }

        [MaxLength(16)]
        public string FormVersion { get; set; }

        [MaxLength(50)]
        public string FormName { get; set; }

        [MaxLength(200)]
        public string Action { get; set; }

        [Required]
        [MaxLength(39)]
        public string IpAddress { get; set; }

        [Required]
        [MaxLength(4000)]
        public string Headers { get; set; }

        [Required]
        public DateTimeOffset CreatedAt { get; set; }
    }
}
