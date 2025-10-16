using System.ComponentModel.DataAnnotations;
using Api.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Integration.Tests.Util;

public static class ValidationExtensions
{
    public static void Validate(this ControllerBase controller, object model)
    {
        var ctx = new ValidationContext(model);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, ctx, results, validateAllProperties: true);
        
        controller.ControllerContext = new ControllerContext();
        controller.ModelState.Clear();
        
        foreach (var result in results)
        {
            controller.ModelState.AddModelError(result.MemberNames.First(), result.ErrorMessage ?? string.Empty);
        }
    }
}