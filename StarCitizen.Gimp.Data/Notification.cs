using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StarCitizen.Gimp.Data
{
    public class Notification
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long NotificationId { get; set; }

        [Required]
        [MaxLength(50)]
        public string NotificationType { get; set; }

        [Required]
        [MaxLength(50)]
        public string Medium { get; set; }

        [Required]
        // This will overflow very quickly
        public string Recipients { get; set; }

        [Required]
        public string Body { get; set; }

        [Required]
        public string Items { get; set; }

        [Required]
        public DateTimeOffset CreatedAt { get; set; }
    }
}
