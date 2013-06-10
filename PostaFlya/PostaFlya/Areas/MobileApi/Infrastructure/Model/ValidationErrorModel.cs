using System.Collections.Generic;

namespace PostaFlya.Areas.MobileApi.Infrastructure.Model
{
    public class ValidationErrorModel
    {
        public class ValidationErrorEntry
        {
            public string Property { get; set; }
            public string Message { get; set; }
        }

        public List<ValidationErrorEntry> Errors { get; set; }

        public static ResponseContent<ValidationErrorModel> SimpleValidationError(string propertyName, string errorMessageFmt, params object[] errorMessageParams)
        {
            return ResponseContent<ValidationErrorModel>.GetResponse(new ValidationErrorModel()
                    {
                        Errors = new List<ValidationErrorEntry>()
                                    {
                                        new ValidationErrorEntry()
                                            {
                                                Property = propertyName,
                                                Message = string.Format(errorMessageFmt, errorMessageParams)
                                            }
                                    }
                    }, ResponseContent.StatusEnum.ValidationError);
        }
    }


}