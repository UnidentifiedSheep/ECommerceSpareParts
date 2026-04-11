using System.Net;
using BulkValidation.Core.Configuration;
using BulkValidation.Core.Enums;
using BulkValidation.Core.Extensions;
using Exceptions.Base;
using Main.Abstractions.Constants;
using Main.Entities;

namespace Main.Application.Configs;

public static class ValidationConfiguration
{
    public static void Configure()
    {
        ConfigureArticles();
        ConfigureProducer();
        ConfigureUser();
        ConfigureUserEmail();
        ConfigureTransaction();
        ConfigureStorage();
        ConfigurePermission();
        ConfigureRole();
        ConfigureCurrency();
        ConfigureProducerOtherNames();
        ConfigureCart();
        ConfigureStorageRoutes();
        ConfigureStorageOwners();
        ConfigureStorageContents();
    }

    private static void ConfigureStorageContents()
    {
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateStorageContentExistsId, KeyValueType.Single,
            config => config.WithErrorName(ApplicationErrors.StorageContentNotFound)
                .WithMessageTemplate("storage.content.not.found")
                .WithErrorType(typeof(NotFoundException))
                .WithErrorCode((int)HttpStatusCode.NotFound));

        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateStorageContentExistsId, KeyValueType.MultipleKeys,
            config => config.WithErrorName(ApplicationErrors.StorageContentNotFound)
                .WithMessageTemplate("storage.content.not.found")
                .WithErrorType(typeof(NotFoundException))
                .WithErrorCode((int)HttpStatusCode.NotFound));
    }

    private static void ConfigureStorageOwners()
    {
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateStorageOwnerNotExistsPK, KeyValueType.Tuple,
            config => config.WithErrorName(ApplicationErrors.StorageOwnerAlreadyExist)
                .WithMessageTemplate("storage.already.belongs.user")
                .WithErrorType(typeof(ConflictException))
                .WithErrorCode((int)HttpStatusCode.Conflict));

        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateStorageOwnerExistsPK, KeyValueType.Tuple,
            config => config.WithErrorName(ApplicationErrors.StorageOwnerNotFound)
                .WithMessageTemplate("storage.not.found.in.user")
                .WithErrorType(typeof(NotFoundException))
                .WithErrorCode((int)HttpStatusCode.NotFound));
    }

    private static void ConfigureStorageRoutes()
    {
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateStorageRouteExistsId, KeyValueType.Single,
            config => config.WithErrorName(ApplicationErrors.StorageRouteNotFound)
                .WithMessageTemplate("storage.route.not.found.by.id")
                .WithErrorType(typeof(NotFoundException))
                .WithErrorCode((int)HttpStatusCode.NotFound));

        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateStorageRouteExistsId, KeyValueType.MultipleKeys,
            config => config.WithErrorName(ApplicationErrors.StorageRouteNotFound)
                .WithMessageTemplate("storage.route.not.found.by.id")
                .WithErrorType(typeof(NotFoundException))
                .WithErrorCode((int)HttpStatusCode.NotFound));

        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateStorageRouteExistsFromTo, KeyValueType.Tuple,
            config => config.WithErrorName(ApplicationErrors.StorageRouteNotFound)
                .WithMessageTemplate("storage.route.not.found.by.names")
                .WithErrorType(typeof(NotFoundException))
                .WithErrorCode((int)HttpStatusCode.NotFound));

        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateStorageRouteNotExistsFromTo, KeyValueType.Tuple,
            config => config.WithErrorName(ApplicationErrors.StorageRouteAlreadyExist)
                .WithMessageTemplate("storage.route.already.exists")
                .WithErrorType(typeof(ConflictException))
                .WithErrorCode((int)HttpStatusCode.Conflict));
    }

    private static void ConfigureCart()
    {
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateCartExistsPK, KeyValueType.Tuple,
            config => config.WithErrorName(ApplicationErrors.CartItemNotFound)
                .WithMessageTemplate("cart.item.not.found")
                .WithErrorType(typeof(NotFoundException))
                .WithErrorCode((int)HttpStatusCode.NotFound));

        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateCartNotExistsPK, KeyValueType.Tuple,
            config => config.WithErrorName(ApplicationErrors.CartItemAlreadyExist)
                .WithMessageTemplate("item.already.in.cart")
                .WithErrorType(typeof(ConflictException))
                .WithErrorCode((int)HttpStatusCode.Conflict));
    }

    private static void ConfigureProducerOtherNames()
    {
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateProducersOtherNameExistsPK, KeyValueType.Tuple,
            config => config.WithErrorName(ApplicationErrors.ProducerOtherNameNotFound)
                .WithMessageTemplate("producer.other.name.not.found")
                .WithErrorType(typeof(NotFoundException))
                .WithErrorCode((int)HttpStatusCode.NotFound));

        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateProducersOtherNameNotExistsPK, KeyValueType.Tuple,
            config => config.WithErrorName(ApplicationErrors.ProducerOtherNameAlreadyTaken)
                .WithMessageTemplate("producer.other.name.already.taken")
                .WithErrorType(typeof(ConflictException))
                .WithErrorCode((int)HttpStatusCode.Conflict));
    }

    private static void ConfigureCurrency()
    {
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateCurrencyExistsId, KeyValueType.Single,
            config => config.WithErrorName(ApplicationErrors.CurrencyNotFound)
                .WithMessageTemplate("currency.not.found")
                .WithErrorType(typeof(NotFoundException))
                .WithErrorCode((int)HttpStatusCode.NotFound));

        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateCurrencyExistsId, KeyValueType.MultipleKeys,
            config => config.WithErrorName(ApplicationErrors.CurrencyNotFound)
                .WithMessageTemplate("currency.not.found")
                .WithErrorType(typeof(NotFoundException))
                .WithErrorCode((int)HttpStatusCode.NotFound));

        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateCurrencyExistsCode, KeyValueType.Single,
            config => config.WithErrorName(ApplicationErrors.CurrencyNotFound)
                .WithMessageTemplate("currency.not.found.by.code")
                .WithErrorType(typeof(NotFoundException))
                .WithErrorCode((int)HttpStatusCode.NotFound));

        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateCurrencyExistsCode, KeyValueType.MultipleKeys,
            config => config.WithErrorName(ApplicationErrors.CurrencyNotFound)
                .WithMessageTemplate("currency.not.found.by.code")
                .WithErrorType(typeof(NotFoundException))
                .WithErrorCode((int)HttpStatusCode.NotFound));

        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateCurrencyNotExistsCode, KeyValueType.Single,
            config => config.WithErrorName(ApplicationErrors.CurrencyCodeAlreadyTaken)
                .WithMessageTemplate("currency.code.already.take")
                .WithErrorType(typeof(ConflictException))
                .WithErrorCode((int)HttpStatusCode.Conflict));

        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateCurrencyNotExistsCode, KeyValueType.MultipleKeys,
            config => config.WithErrorName(ApplicationErrors.CurrencyCodeAlreadyTaken)
                .WithMessageTemplate("currency.code.already.take")
                .WithErrorType(typeof(ConflictException))
                .WithErrorCode((int)HttpStatusCode.Conflict));

        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateCurrencyNotExistsName, KeyValueType.Single,
            config => config.WithErrorName(ApplicationErrors.CurrencyNameAlreadyTaken)
                .WithMessageTemplate("currency.name.already.take")
                .WithErrorType(typeof(ConflictException))
                .WithErrorCode((int)HttpStatusCode.Conflict));

        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateCurrencyNotExistsName, KeyValueType.MultipleKeys,
            config => config.WithErrorName(ApplicationErrors.CurrencyNameAlreadyTaken)
                .WithMessageTemplate("currency.name.already.take")
                .WithErrorType(typeof(ConflictException))
                .WithErrorCode((int)HttpStatusCode.Conflict));

        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateCurrencyNotExistsShortName, KeyValueType.Single,
            config => config.WithErrorName(ApplicationErrors.CurrencyShortNameAlreadyTaken)
                .WithMessageTemplate("currency.short.name.already.take")
                .WithErrorType(typeof(ConflictException))
                .WithErrorCode((int)HttpStatusCode.Conflict));

        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateCurrencyNotExistsShortName,
            KeyValueType.MultipleKeys,
            config => config.WithErrorName(ApplicationErrors.CurrencyShortNameAlreadyTaken)
                .WithMessageTemplate("currency.short.name.already.take")
                .WithErrorType(typeof(ConflictException))
                .WithErrorCode((int)HttpStatusCode.Conflict));

        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateCurrencyNotExistsCurrencySign, KeyValueType.Single,
            config => config.WithErrorName(ApplicationErrors.CurrencySignAlreadyTaken)
                .WithMessageTemplate("currency.sign.already.take")
                .WithErrorType(typeof(ConflictException))
                .WithErrorCode((int)HttpStatusCode.Conflict));

        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateCurrencyNotExistsCurrencySign,
            KeyValueType.MultipleKeys,
            config => config.WithErrorName(ApplicationErrors.CurrencySignAlreadyTaken)
                .WithMessageTemplate("currency.sign.already.take")
                .WithErrorType(typeof(ConflictException))
                .WithErrorCode((int)HttpStatusCode.Conflict));
    }

    private static void ConfigureRole()
    {
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateRoleExistsNormalizedName, KeyValueType.Single,
            config => config.WithErrorName(ApplicationErrors.RoleNotFound)
                .WithMessageTemplate("role.not.found.with.role.name")
                .WithErrorType(typeof(NotFoundException))
                .WithErrorCode((int)HttpStatusCode.NotFound));

        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateRoleExistsNormalizedName, KeyValueType.MultipleKeys,
            config => config.WithErrorName(ApplicationErrors.RoleNotFound)
                .WithMessageTemplate("role.not.found.with.role.name")
                .WithErrorType(typeof(NotFoundException))
                .WithErrorCode((int)HttpStatusCode.NotFound));

        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateRoleNotExistsNormalizedName, KeyValueType.Single,
            config => config.WithErrorName(ApplicationErrors.RoleNameAlreadyTaken)
                .WithMessageTemplate("role.already.exists")
                .WithErrorType(typeof(ConflictException))
                .WithErrorCode((int)HttpStatusCode.Conflict));

        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateRoleNotExistsNormalizedName,
            KeyValueType.MultipleKeys,
            config => config.WithErrorName(ApplicationErrors.RoleNameAlreadyTaken)
                .WithMessageTemplate("role.already.exists")
                .WithErrorType(typeof(ConflictException))
                .WithErrorCode((int)HttpStatusCode.Conflict));
    }

    private static void ConfigurePermission()
    {
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidatePermissionExistsName, KeyValueType.Single,
            config => config.WithErrorName(ApplicationErrors.PermissionNotFound)
                .WithMessageTemplate("permission.not.found")
                .WithErrorType(typeof(NotFoundException))
                .WithErrorCode((int)HttpStatusCode.NotFound));

        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidatePermissionExistsName, KeyValueType.MultipleKeys,
            config => config.WithErrorName(ApplicationErrors.PermissionNotFound)
                .WithMessageTemplate("permission.not.found")
                .WithErrorType(typeof(NotFoundException))
                .WithErrorCode((int)HttpStatusCode.NotFound));

        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidatePermissionNotExistsName, KeyValueType.Single,
            config => config.WithErrorName(ApplicationErrors.PermissionAlreadyExists)
                .WithMessageTemplate("permission.name.taken")
                .WithErrorType(typeof(ConflictException))
                .WithErrorCode((int)HttpStatusCode.Conflict));

        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidatePermissionNotExistsName, KeyValueType.MultipleKeys,
            config => config.WithErrorName(ApplicationErrors.PermissionAlreadyExists)
                .WithMessageTemplate("permission.name.taken")
                .WithErrorType(typeof(ConflictException))
                .WithErrorCode((int)HttpStatusCode.Conflict));
    }

    private static void ConfigureStorage()
    {
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateStorageExistsName, KeyValueType.Single,
            config => config.WithErrorName(ApplicationErrors.StoragesNotFound)
                .WithMessageTemplate("storage.not.found")
                .WithErrorType(typeof(NotFoundException))
                .WithErrorCode((int)HttpStatusCode.NotFound));

        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateStorageExistsName, KeyValueType.MultipleKeys,
            config => config.WithErrorName(ApplicationErrors.StoragesNotFound)
                .WithMessageTemplate("storage.not.found")
                .WithErrorType(typeof(NotFoundException))
                .WithErrorCode((int)HttpStatusCode.NotFound));

        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateStorageNotExistsName, KeyValueType.Single,
            config => config.WithErrorName(ApplicationErrors.StoragesNameAlreadyTaken)
                .WithMessageTemplate("storage.name.taken")
                .WithErrorType(typeof(ConflictException))
                .WithErrorCode((int)HttpStatusCode.Conflict));

        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateStorageNotExistsName, KeyValueType.MultipleKeys,
            config => config.WithErrorName(ApplicationErrors.StoragesNameAlreadyTaken)
                .WithMessageTemplate("storage.name.taken")
                .WithErrorType(typeof(ConflictException))
                .WithErrorCode((int)HttpStatusCode.Conflict));
    }

    private static void ConfigureArticles()
    {
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateProductExistsId, KeyValueType.Single,
            config => config.WithErrorName(ApplicationErrors.ArticlesNotFound)
                .WithMessageTemplate("article.not.found")
                .WithErrorType(typeof(NotFoundException))
                .WithErrorCode((int)HttpStatusCode.NotFound));

        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateProductExistsId, KeyValueType.MultipleKeys,
            config => config.WithErrorName(ApplicationErrors.ArticlesNotFound)
                .WithMessageTemplate("articles.not.found")
                .WithErrorType(typeof(NotFoundException))
                .WithErrorCode((int)HttpStatusCode.NotFound));
    }

    private static void ConfigureProducer()
    {
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateProducerExistsId, KeyValueType.Single,
            config => config.WithErrorName(ApplicationErrors.ProducersNotFound)
                .WithMessageTemplate("producer.not.found")
                .WithErrorCode((int)HttpStatusCode.NotFound)
                .WithErrorType(typeof(NotFoundException)));

        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateProducerExistsId, KeyValueType.MultipleKeys,
            config => config.WithErrorName(ApplicationErrors.ProducersNotFound)
                .WithMessageTemplate("producers.not.found")
                .WithErrorCode((int)HttpStatusCode.NotFound)
                .WithErrorType(typeof(NotFoundException)));

        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateProducerExistsName, KeyValueType.Single,
            config => config.WithErrorName(ApplicationErrors.ProducersNotFound)
                .WithMessageTemplate("producer.not.found")
                .WithErrorCode((int)HttpStatusCode.NotFound)
                .WithErrorType(typeof(NotFoundException)));

        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateProducerExistsName, KeyValueType.MultipleKeys,
            config => config.WithErrorName(ApplicationErrors.ProducersNotFound)
                .WithMessageTemplate("producers.not.found")
                .WithErrorCode((int)HttpStatusCode.NotFound)
                .WithErrorType(typeof(NotFoundException)));
    }

    private static void ConfigureUser()
    {
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateUserExistsId, KeyValueType.Single,
            config => config.WithErrorName(ApplicationErrors.UsersNotFound)
                .WithMessageTemplate("user.not.found")
                .WithErrorCode((int)HttpStatusCode.NotFound)
                .WithErrorType(typeof(NotFoundException)));

        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateUserExistsId, KeyValueType.MultipleKeys,
            config => config.WithErrorName(ApplicationErrors.UsersNotFound)
                .WithMessageTemplate("user.not.found")
                .WithErrorCode((int)HttpStatusCode.NotFound)
                .WithErrorType(typeof(NotFoundException)));

        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateUserNotExistsNormalizedUserName,
            KeyValueType.Single,
            config => config.WithErrorName(ApplicationErrors.UserNameAlreadyTaken)
                .WithMessageTemplate("user.name.already.taken")
                .WithErrorCode((int)HttpStatusCode.Conflict)
                .WithErrorType(typeof(ConflictException)));

        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateUserNotExistsNormalizedUserName,
            KeyValueType.MultipleKeys,
            config => config.WithErrorName(ApplicationErrors.UserNameAlreadyTaken)
                .WithMessageTemplate("user.name.already.taken")
                .WithErrorCode((int)HttpStatusCode.Conflict)
                .WithErrorType(typeof(ConflictException)));
    }

    private static void ConfigureUserEmail()
    {
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateUserEmailExistsNormalizedEmail, KeyValueType.Single,
            config => config.WithErrorName(ApplicationErrors.UserEmailNotFound)
                .WithMessageTemplate("user.email.not.found")
                .WithErrorCode((int)HttpStatusCode.NotFound)
                .WithErrorType(typeof(NotFoundException)));

        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateUserEmailExistsNormalizedEmail,
            KeyValueType.MultipleKeys,
            config => config.WithErrorName(ApplicationErrors.UserEmailNotFound)
                .WithMessageTemplate("user.email.not.found")
                .WithErrorCode((int)HttpStatusCode.NotFound)
                .WithErrorType(typeof(NotFoundException)));

        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateUserEmailNotExistsNormalizedEmail,
            KeyValueType.Single,
            config => config.WithErrorName(ApplicationErrors.UserEmailAlreadyTaken)
                .WithMessageTemplate("user.email.already.in.use")
                .WithErrorCode((int)HttpStatusCode.Conflict)
                .WithErrorType(typeof(ConflictException)));

        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateUserEmailNotExistsNormalizedEmail,
            KeyValueType.MultipleKeys,
            config => config.WithErrorName(ApplicationErrors.UserEmailAlreadyTaken)
                .WithMessageTemplate("user.email.already.in.use")
                .WithErrorCode((int)HttpStatusCode.Conflict)
                .WithErrorType(typeof(ConflictException)));
    }

    private static void ConfigureTransaction()
    {
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateTransactionExistsId, KeyValueType.Single,
            config => config.WithErrorName(ApplicationErrors.TransactionsNotFound)
                .WithMessageTemplate("transaction.not.found")
                .WithErrorCode((int)HttpStatusCode.NotFound)
                .WithErrorType(typeof(NotFoundException)));

        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateTransactionExistsId, KeyValueType.MultipleKeys,
            config => config.WithErrorName(ApplicationErrors.TransactionsNotFound)
                .WithMessageTemplate("transaction.not.found")
                .WithErrorCode((int)HttpStatusCode.NotFound)
                .WithErrorType(typeof(NotFoundException)));
    }
}