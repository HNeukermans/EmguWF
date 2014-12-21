using System.Activities.Presentation.Validation;
using System.Collections.Generic;

namespace EmguWF.Extensions
{
    /// <summary>
    /// This is used to report validation errors 
    /// from the workflow into our UI
    /// </summary>
    public class ValidationErrorService : IValidationErrorService
    {
        private readonly IList<string> _errorList;
        public ValidationErrorService(IList<string> errorList)
        {
            _errorList = errorList;
        }

        public void ShowValidationErrors(IList<ValidationErrorInfo> errors)
        {
            _errorList.Clear();
            foreach (var error in errors)
            {
                _errorList.Add(error.Message);
            }
        }
    }
}
