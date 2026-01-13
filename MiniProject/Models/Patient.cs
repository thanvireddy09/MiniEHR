using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniProject.Models
{
    [Table("Patient", Schema = "Healthcare")]
    public class Patient
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Patient name is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 50 characters")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Name can contain only letters and spaces")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date of Birth is required")]
        [DataType(DataType.Date)]
        [CustomValidation(typeof(Patient), nameof(ValidateDateOfBirth))]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        [RegularExpression("^(Male|Female|Other)$", ErrorMessage = "Gender must be Male, Female, or Other")]
        [StringLength(10)]
        public string Gender { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [StringLength(100)]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Phone number must be a valid 10-digit Indian number")]
        public string Phone { get; set; } = string.Empty;

        // Navigation Property
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

        // ? Custom DOB validation (no future date, min age 1)
        public static ValidationResult? ValidateDateOfBirth(DateTime dob, ValidationContext context)
        {
            if (dob > DateTime.Today)
            {
                return new ValidationResult("Date of Birth cannot be in the future");
            }

            int age = DateTime.Today.Year - dob.Year;
            if (dob > DateTime.Today.AddYears(-age)) age--;

            if (age < 1)
            {
                return new ValidationResult("Patient age must be at least 1 year");
            }

            return ValidationResult.Success;
        }
    }
}
