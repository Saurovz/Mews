using FluentValidation.Results;

namespace Mews.Job.Scheduler.Extensions;

public static class ValidationExtensions
{
    /// <summary>
    /// Retrieve formatted validation errors for validation problem result
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    public static Dictionary<string, string[]> GetValidationProblems(this ValidationResult result)
    {
        return result.Errors
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());
    }
}