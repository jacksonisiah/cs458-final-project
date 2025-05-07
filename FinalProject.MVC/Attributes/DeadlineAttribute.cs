using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Serilog;

namespace FinalProject.MVC.Attributes;

public class DeadlineAttribute : ValidationAttribute, IClientModelValidator
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return ValidationResult.Success;
        }

        var date = (DateTime)value;
        var today = DateTime.Today;
        return date > today ? ValidationResult.Success : new ValidationResult(ErrorMessage);
    }

    public void AddValidation(ClientModelValidationContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }
        MergeAttribute(context.Attributes, "data-val", "true");
        MergeAttribute(
            context.Attributes,
            "data-val-deadline",
            ErrorMessage ?? "Date must be in the future"
        );
        MergeAttribute(context.Attributes, "data-val-date", "Please enter a valid date");
    }

    private bool MergeAttribute(IDictionary<string, string> attributes, string key, string value)
    {
        if (attributes.ContainsKey(key))
        {
            return false;
        }
        attributes.Add(key, value);
        return true;
    }
}
