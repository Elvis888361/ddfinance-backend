using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace InsuranceAPI.Models
{
    public class Policy
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        [RegularExpression(@"^[A-Z0-9]{8,}$")]
        public string PolicyNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string HolderName { get; set; } = string.Empty;

        [Required]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PolicyType Type { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Premium { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PolicyType
    {
        Life,
        Health,
        Vehicle,
        Property
    }
} 