using System.ComponentModel.DataAnnotations;

namespace CareNest_SePay.Application.Exceptions
{
    public class ValidationException : Exception
    {
        public List<ValidationResult> ValidationResults { get; }

        public ValidationException(List<ValidationResult> validationResults) 
            : base("Validation failed")
        {
            ValidationResults = validationResults;
        }

        public ValidationException(string message) 
            : base(message)
        {
            ValidationResults = new List<ValidationResult> { new ValidationResult(message) };
        }
    }
}
