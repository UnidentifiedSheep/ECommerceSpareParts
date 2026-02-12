using System.Net;
using BulkValidation.Core.Configuration;
using BulkValidation.Core.Enums;
using BulkValidation.Core.Extensions;
using Exceptions.Base;
using Pricing.Abstractions.Constants;
using Pricing.Entities;

namespace Pricing.Application.Configs;

public static class ValidationConfiguration
{
    public static void Configure()
    {
        ConfigureMarkupGroup();
    }
    
    private static void ConfigureMarkupGroup()
    {
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateMarkupGroupExistsId, KeyValueType.Single, 
            config => config.WithErrorName(ApplicationErrors.MarkupGroupNotFound)
                .WithMessageTemplate("Не удалось найти группу наценки.")
                .WithErrorType(typeof(NotFoundException))
                .WithErrorCode((int)HttpStatusCode.NotFound));
        
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateMarkupGroupExistsId, KeyValueType.MultipleKeys, 
            config => config.WithErrorName(ApplicationErrors.MarkupGroupNotFound)
                .WithMessageTemplate("Не удалось найти группы наценок.")
                .WithErrorType(typeof(NotFoundException))
                .WithErrorCode((int)HttpStatusCode.NotFound));
    }
}