using System.ComponentModel.DataAnnotations;

namespace AppointmentSender.Models
{
    public class Employee
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; }

        public string Email { get; set; }

        public decimal CTC { get; set; }

        public string Breakdown { get; set; } 
    }
}
