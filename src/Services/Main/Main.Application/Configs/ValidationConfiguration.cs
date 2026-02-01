using System.Net;
using BulkValidation.Core.Configuration;
using BulkValidation.Core.Enums;
using BulkValidation.Core.Extensions;
using Exceptions.Base;
using Main.Abstractions.Consts;
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
        ConfigureMarkupGroup();
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
                .WithMessageTemplate("Не удалось найти позицию на складе.")
                .WithErrorType(typeof(NotFoundException))
                .WithErrorCode((int)HttpStatusCode.NotFound));
        
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateStorageContentExistsId, KeyValueType.MultipleKeys,
            config => config.WithErrorName(ApplicationErrors.StorageContentNotFound)
                .WithMessageTemplate("Не удалось найти позицию на складе.")
                .WithErrorType(typeof(NotFoundException))
                .WithErrorCode((int)HttpStatusCode.NotFound));
    }

    private static void ConfigureStorageOwners()
    {
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateStorageOwnerNotExistsPK, KeyValueType.Tuple,
            config => config.WithErrorName(ApplicationErrors.StorageOwnerAlreadyExist)
                .WithMessageTemplate("Данные пользователь уже владеет данным складом.")
                .WithErrorType(typeof(ConflictException))
                .WithErrorCode((int)HttpStatusCode.Conflict));
        
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateStorageOwnerExistsPK, KeyValueType.Tuple,
            config => config.WithErrorName(ApplicationErrors.StorageOwnerNotFound)
                .WithMessageTemplate("Не удалось найти данный склад во владениях у пользователя.")
                .WithErrorType(typeof(NotFoundException))
                .WithErrorCode((int)HttpStatusCode.NotFound));
    }

    private static void ConfigureStorageRoutes()
    {
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateStorageRouteExistsId, KeyValueType.Single,
            config => config.WithErrorName(ApplicationErrors.StorageRouteNotFound)
                .WithMessageTemplate("Не удалось найти складской путь.")
                .WithErrorType(typeof(NotFoundException))
                .WithErrorCode((int)HttpStatusCode.NotFound));
        
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateStorageRouteExistsId, KeyValueType.MultipleKeys,
            config => config.WithErrorName(ApplicationErrors.StorageRouteNotFound)
                .WithMessageTemplate("Не удалось найти складские пути.")
                .WithErrorType(typeof(NotFoundException))
                .WithErrorCode((int)HttpStatusCode.NotFound));
        
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateStorageRouteExistsFromTo, KeyValueType.Tuple,
            config => config.WithErrorName(ApplicationErrors.StorageRouteNotFound)
                .WithMessageTemplate("Не удалось найти складской путь.")
                .WithErrorType(typeof(NotFoundException))
                .WithErrorCode((int)HttpStatusCode.NotFound));
        
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateStorageRouteNotExistsFromTo, KeyValueType.Tuple,
            config => config.WithErrorName(ApplicationErrors.StorageRouteAlreadyExist)
                .WithMessageTemplate("Такой складской путь уже существует.")
                .WithErrorType(typeof(ConflictException))
                .WithErrorCode((int)HttpStatusCode.Conflict));
    }

    private static void ConfigureCart()
    {
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateCartExistsPK, KeyValueType.Tuple,
            config => config.WithErrorName(ApplicationErrors.CartItemNotFound)
                .WithMessageTemplate("Не удалось найти позицию в корзине.")
                .WithErrorType(typeof(NotFoundException))
                .WithErrorCode((int)HttpStatusCode.NotFound));
        
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateCartNotExistsPK, KeyValueType.Tuple,
            config => config.WithErrorName(ApplicationErrors.CartItemAlreadyExist)
                .WithMessageTemplate("Позиция уже в корзине.")
                .WithErrorType(typeof(ConflictException))
                .WithErrorCode((int)HttpStatusCode.Conflict));
    }

    private static void ConfigureProducerOtherNames()
    {
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateProducersOtherNameExistsPK, KeyValueType.Tuple,
            config => config.WithErrorName(ApplicationErrors.ProducerOtherNameNotFound)
                .WithMessageTemplate("Не удалось найти альтернативное название производителя.")
                .WithErrorType(typeof(NotFoundException))
                .WithErrorCode((int)HttpStatusCode.NotFound));
        
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateProducersOtherNameNotExistsPK, KeyValueType.Tuple,
            config => config.WithErrorName(ApplicationErrors.ProducerOtherNameAlreadyTaken)
                .WithMessageTemplate("Данное альтернативное название производителя уже занято.")
                .WithErrorType(typeof(ConflictException))
                .WithErrorCode((int)HttpStatusCode.Conflict));
    }

    private static void ConfigureCurrency()
    {
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateCurrencyExistsId, KeyValueType.Single, 
            config => config.WithErrorName(ApplicationErrors.CurrencyNotFound)
                .WithMessageTemplate("Не удалось найти валюту.")
                .WithErrorType(typeof(NotFoundException))
                .WithErrorCode((int)HttpStatusCode.NotFound));
        
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateCurrencyExistsId, KeyValueType.MultipleKeys, 
            config => config.WithErrorName(ApplicationErrors.CurrencyNotFound)
                .WithMessageTemplate("Не удалось найти валюты.")
                .WithErrorType(typeof(NotFoundException))
                .WithErrorCode((int)HttpStatusCode.NotFound));
        
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateCurrencyExistsCode, KeyValueType.Single, 
            config => config.WithErrorName(ApplicationErrors.CurrencyNotFound)
                .WithMessageTemplate("Не удалось найти валюту.")
                .WithErrorType(typeof(NotFoundException))
                .WithErrorCode((int)HttpStatusCode.NotFound));
        
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateCurrencyExistsCode, KeyValueType.MultipleKeys, 
            config => config.WithErrorName(ApplicationErrors.CurrencyNotFound)
                .WithMessageTemplate("Не удалось найти валюты.")
                .WithErrorType(typeof(NotFoundException))
                .WithErrorCode((int)HttpStatusCode.NotFound));
        
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateCurrencyNotExistsCode, KeyValueType.Single, 
            config => config.WithErrorName(ApplicationErrors.CurrencyCodeAlreadyTaken)
                .WithMessageTemplate("Валюта с таким кодом уже существует.")
                .WithErrorType(typeof(ConflictException))
                .WithErrorCode((int)HttpStatusCode.Conflict));
        
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateCurrencyNotExistsCode, KeyValueType.MultipleKeys, 
            config => config.WithErrorName(ApplicationErrors.CurrencyCodeAlreadyTaken)
                .WithMessageTemplate("Валюты с таким кодом уже существуют.")
                .WithErrorType(typeof(ConflictException))
                .WithErrorCode((int)HttpStatusCode.Conflict));
        
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateCurrencyNotExistsName, KeyValueType.Single, 
            config => config.WithErrorName(ApplicationErrors.CurrencyNameAlreadyTaken)
                .WithMessageTemplate("Валюта с таким названием уже существует.")
                .WithErrorType(typeof(ConflictException))
                .WithErrorCode((int)HttpStatusCode.Conflict));
        
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateCurrencyNotExistsName, KeyValueType.MultipleKeys, 
            config => config.WithErrorName(ApplicationErrors.CurrencyNameAlreadyTaken)
                .WithMessageTemplate("Валюты с такими названиями уже существуют.")
                .WithErrorType(typeof(ConflictException))
                .WithErrorCode((int)HttpStatusCode.Conflict));
        
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateCurrencyNotExistsShortName, KeyValueType.Single, 
            config => config.WithErrorName(ApplicationErrors.CurrencyShortNameAlreadyTaken)
                .WithMessageTemplate("Валюта с таким коротким названием уже существует.")
                .WithErrorType(typeof(ConflictException))
                .WithErrorCode((int)HttpStatusCode.Conflict));
        
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateCurrencyNotExistsShortName, KeyValueType.MultipleKeys, 
            config => config.WithErrorName(ApplicationErrors.CurrencyShortNameAlreadyTaken)
                .WithMessageTemplate("Валюты с такими короткими названиями уже существуют.")
                .WithErrorType(typeof(ConflictException))
                .WithErrorCode((int)HttpStatusCode.Conflict));
        
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateCurrencyNotExistsCurrencySign, KeyValueType.Single, 
            config => config.WithErrorName(ApplicationErrors.CurrencySignAlreadyTaken)
                .WithMessageTemplate("Валюта с таким символом названием уже существует.")
                .WithErrorType(typeof(ConflictException))
                .WithErrorCode((int)HttpStatusCode.Conflict));
        
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateCurrencyNotExistsCurrencySign, KeyValueType.MultipleKeys, 
            config => config.WithErrorName(ApplicationErrors.CurrencySignAlreadyTaken)
                .WithMessageTemplate("Валюты с такими символами уже существуют.")
                .WithErrorType(typeof(ConflictException))
                .WithErrorCode((int)HttpStatusCode.Conflict));
    }

    private static void ConfigureRole()
    {
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateRoleExistsId, KeyValueType.Single, 
            config => config.WithErrorName(ApplicationErrors.RoleNotFound)
                .WithMessageTemplate("Не удалось найти роль.")
                .WithErrorType(typeof(NotFoundException))
                .WithErrorCode((int)HttpStatusCode.NotFound));
        
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateRoleExistsId, KeyValueType.MultipleKeys, 
            config => config.WithErrorName(ApplicationErrors.RoleNotFound)
                .WithMessageTemplate("Не удалось найти роли.")
                .WithErrorType(typeof(NotFoundException))
                .WithErrorCode((int)HttpStatusCode.NotFound));
        
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateRoleExistsNormalizedName, KeyValueType.Single, 
            config => config.WithErrorName(ApplicationErrors.RoleNotFound)
                .WithMessageTemplate("Не удалось найти роль.")
                .WithErrorType(typeof(NotFoundException))
                .WithErrorCode((int)HttpStatusCode.NotFound));
        
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateRoleExistsNormalizedName, KeyValueType.MultipleKeys, 
            config => config.WithErrorName(ApplicationErrors.RoleNotFound)
                .WithMessageTemplate("Не удалось найти роли.")
                .WithErrorType(typeof(NotFoundException))
                .WithErrorCode((int)HttpStatusCode.NotFound));
        
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateRoleNotExistsNormalizedName, KeyValueType.Single, 
            config => config.WithErrorName(ApplicationErrors.RoleNameAlreadyTaken)
                .WithMessageTemplate("Роль с таким названием уже существует.")
                .WithErrorType(typeof(ConflictException))
                .WithErrorCode((int)HttpStatusCode.Conflict));
        
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateRoleNotExistsNormalizedName, KeyValueType.MultipleKeys, 
            config => config.WithErrorName(ApplicationErrors.RoleNameAlreadyTaken)
                .WithMessageTemplate("Роли с таким названиями уже существует.")
                .WithErrorType(typeof(ConflictException))
                .WithErrorCode((int)HttpStatusCode.Conflict));
    }

    private static void ConfigurePermission()
    {
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidatePermissionExistsName, KeyValueType.Single, 
            config => config.WithErrorName(ApplicationErrors.PermissionNotFound)
                .WithMessageTemplate("Не удалось найти разрешение.")
                .WithErrorType(typeof(NotFoundException))
                .WithErrorCode((int)HttpStatusCode.NotFound));
        
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidatePermissionExistsName, KeyValueType.MultipleKeys, 
            config => config.WithErrorName(ApplicationErrors.PermissionNotFound)
                .WithMessageTemplate("Не удалось найти разрешения.")
                .WithErrorType(typeof(NotFoundException))
                .WithErrorCode((int)HttpStatusCode.NotFound));
        
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidatePermissionNotExistsName, KeyValueType.Single, 
            config => config.WithErrorName(ApplicationErrors.PermissionAlreadyExists)
                .WithMessageTemplate("Разрешение с таким названием уже существует.")
                .WithErrorType(typeof(ConflictException))
                .WithErrorCode((int)HttpStatusCode.Conflict));
        
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidatePermissionNotExistsName, KeyValueType.MultipleKeys, 
            config => config.WithErrorName(ApplicationErrors.PermissionAlreadyExists)
                .WithMessageTemplate("Разрешения с такими названиями уже существуют.")
                .WithErrorType(typeof(ConflictException))
                .WithErrorCode((int)HttpStatusCode.Conflict));
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

    private static void ConfigureStorage()
    {
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateStorageExistsName, KeyValueType.Single, 
            config => config.WithErrorName(ApplicationErrors.StoragesNotFound)
                .WithMessageTemplate("Не удалось найти склад.")
                .WithErrorType(typeof(NotFoundException))
                .WithErrorCode((int)HttpStatusCode.NotFound));
        
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateStorageExistsName, KeyValueType.MultipleKeys, 
            config => config.WithErrorName(ApplicationErrors.StoragesNotFound)
                .WithMessageTemplate("Не удалось найти склады.")
                .WithErrorType(typeof(NotFoundException))
                .WithErrorCode((int)HttpStatusCode.NotFound));
        
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateStorageNotExistsName, KeyValueType.Single, 
            config => config.WithErrorName(ApplicationErrors.StoragesNameAlreadyTaken)
                .WithMessageTemplate("Склад с таким названием уже существует.")
                .WithErrorType(typeof(ConflictException))
                .WithErrorCode((int)HttpStatusCode.Conflict));
        
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateStorageNotExistsName, KeyValueType.MultipleKeys, 
            config => config.WithErrorName(ApplicationErrors.StoragesNameAlreadyTaken)
                .WithMessageTemplate("Склады с такими названиями уже существуют.")
                .WithErrorType(typeof(ConflictException))
                .WithErrorCode((int)HttpStatusCode.Conflict));
    }

    private static void ConfigureArticles()
    {
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateArticleExistsId, KeyValueType.Single, 
            config => config.WithErrorName(ApplicationErrors.ArticlesNotFound)
                .WithMessageTemplate("Не удалось найти артикул.")
                .WithErrorType(typeof(NotFoundException))
                .WithErrorCode((int)HttpStatusCode.NotFound));

        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateArticleExistsId, KeyValueType.MultipleKeys,
            config => config.WithErrorName(ApplicationErrors.ArticlesNotFound)
                .WithMessageTemplate("Не удалось найти артикулы.")
                .WithErrorType(typeof(NotFoundException))
                .WithErrorCode((int)HttpStatusCode.NotFound));
    }
    private static void ConfigureProducer()
    {
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateProducerExistsId, KeyValueType.Single,
            config => config.WithErrorName(ApplicationErrors.ProducersNotFound)
                .WithMessageTemplate("Не удалось найти производителя.")
                .WithErrorCode((int)HttpStatusCode.NotFound)
                .WithErrorType(typeof(NotFoundException)));
        
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateProducerExistsId, KeyValueType.MultipleKeys,
            config => config.WithErrorName(ApplicationErrors.ProducersNotFound)
                .WithMessageTemplate("Не удалось найти производителей.")
                .WithErrorCode((int)HttpStatusCode.NotFound)
                .WithErrorType(typeof(NotFoundException)));
        
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateProducerExistsName, KeyValueType.Single,
            config => config.WithErrorName(ApplicationErrors.ProducersNotFound)
                .WithMessageTemplate("Не удалось найти производителя.")
                .WithErrorCode((int)HttpStatusCode.NotFound)
                .WithErrorType(typeof(NotFoundException)));
        
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateProducerExistsName, KeyValueType.MultipleKeys,
            config => config.WithErrorName(ApplicationErrors.ProducersNotFound)
                .WithMessageTemplate("Не удалось найти производителей.")
                .WithErrorCode((int)HttpStatusCode.NotFound)
                .WithErrorType(typeof(NotFoundException)));
    }

    private static void ConfigureUser()
    {
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateUserExistsId, KeyValueType.Single,
            config => config.WithErrorName(ApplicationErrors.UsersNotFound)
                .WithMessageTemplate("Не удалось найти пользователя.")
                .WithErrorCode((int)HttpStatusCode.NotFound)
                .WithErrorType(typeof(NotFoundException)));
        
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateUserExistsId, KeyValueType.MultipleKeys,
            config => config.WithErrorName(ApplicationErrors.UsersNotFound)
                .WithMessageTemplate("Не удалось найти пользователей.")
                .WithErrorCode((int)HttpStatusCode.NotFound)
                .WithErrorType(typeof(NotFoundException)));
        
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateUserNotExistsNormalizedUserName, KeyValueType.Single,
            config => config.WithErrorName(ApplicationErrors.UserNameAlreadyTaken)
                .WithMessageTemplate("Логин пользователя уже занят.")
                .WithErrorCode((int)HttpStatusCode.Conflict)
                .WithErrorType(typeof(ConflictException)));
        
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateUserNotExistsNormalizedUserName, KeyValueType.MultipleKeys,
            config => config.WithErrorName(ApplicationErrors.UserNameAlreadyTaken)
                .WithMessageTemplate("Логины пользователей уже занят.")
                .WithErrorCode((int)HttpStatusCode.Conflict)
                .WithErrorType(typeof(ConflictException)));
    }

    private static void ConfigureUserEmail()
    {
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateUserEmailExistsNormalizedEmail, KeyValueType.Single,
            config => config.WithErrorName(ApplicationErrors.UserEmailNotFound)
                .WithMessageTemplate("Не удалось найти почту.")
                .WithErrorCode((int)HttpStatusCode.NotFound)
                .WithErrorType(typeof(NotFoundException)));
        
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateUserEmailExistsNormalizedEmail, KeyValueType.MultipleKeys,
            config => config.WithErrorName(ApplicationErrors.UserEmailNotFound)
                .WithMessageTemplate("Не удалось найти почты.")
                .WithErrorCode((int)HttpStatusCode.NotFound)
                .WithErrorType(typeof(NotFoundException)));
        
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateUserEmailNotExistsNormalizedEmail, KeyValueType.Single,
            config => config.WithErrorName(ApplicationErrors.UserEmailAlreadyTaken)
                .WithMessageTemplate("Данная почта уже используется другим пользователем.")
                .WithErrorCode((int)HttpStatusCode.Conflict)
                .WithErrorType(typeof(ConflictException)));
        
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateUserEmailNotExistsNormalizedEmail, KeyValueType.MultipleKeys,
            config => config.WithErrorName(ApplicationErrors.UserEmailAlreadyTaken)
                .WithMessageTemplate("Данные почты уже используются другими пользователями.")
                .WithErrorCode((int)HttpStatusCode.Conflict)
                .WithErrorType(typeof(ConflictException)));
    }

    private static void ConfigureTransaction()
    {
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateTransactionExistsId, KeyValueType.Single,
            config => config.WithErrorName(ApplicationErrors.TransactionsNotFound)
                .WithMessageTemplate("Не удалось найти транзакцию.")
                .WithErrorCode((int)HttpStatusCode.NotFound)
                .WithErrorType(typeof(NotFoundException)));
        
        ConfigureDbValidation.AddConfig(ValidationFunctions.ValidateTransactionExistsId, KeyValueType.MultipleKeys,
            config => config.WithErrorName(ApplicationErrors.TransactionsNotFound)
                .WithMessageTemplate("Не удалось найти транзакцию.")
                .WithErrorCode((int)HttpStatusCode.NotFound)
                .WithErrorType(typeof(NotFoundException)));
    }
}