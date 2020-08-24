using Nancy.Validation;
using System.Collections.Generic;

namespace Lern_API.DataTransferObjects
{
    public class ValidationFailedResponse
    {
        public List<string> Messages { get; } = new List<string>();

        public ValidationFailedResponse()
        {

        }

        public ValidationFailedResponse(ModelValidationResult validationResult)
        {
            ErrorsToStrings(validationResult);
        }

        public ValidationFailedResponse(string message)
        {
            Messages = new List<string>
            {
                message
            };
        }

        private void ErrorsToStrings(ModelValidationResult validationResult)
        {
            foreach (var errorGroup in validationResult.Errors)
            {
                foreach (var error in errorGroup.Value)
                {
                    Messages.Add(error.ErrorMessage);
                }
            }
        }
    }
}
