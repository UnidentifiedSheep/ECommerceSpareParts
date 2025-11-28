using Exceptions.Exceptions.Articles;
using Exceptions.Exceptions.Balances;
using Exceptions.Exceptions.Currencies;
using Exceptions.Exceptions.Permissions;
using Exceptions.Exceptions.Producers;
using Exceptions.Exceptions.Roles;
using Exceptions.Exceptions.Storages;
using Exceptions.Exceptions.Users;
using Main.Core.Entities;
using Main.Core.Interfaces.Validation;

namespace Main.Application.Extensions;

public static class ValidationPlanExtensions
{
    //USER
    public static IValidationPlan EnsureUserExists(this IValidationPlan plan, Guid userId)
        => plan.EnsureExists<User, Guid>(x => x.Id, userId, typeof(UserNotFoundException));
    
    public static IValidationPlan EnsureUserExists(this IValidationPlan plan, IEnumerable<Guid> userIds)
        => plan.EnsureExists<User, Guid>(x => x.Id, userIds, typeof(UserNotFoundException));
    
    //CURRENCY
    public static IValidationPlan EnsureCurrencyExists(this IValidationPlan plan, int currencyId)
        => plan.EnsureExists<Currency, int>(x => x.Id, currencyId, typeof(CurrencyNotFoundException));
    
    public static IValidationPlan EnsureCurrencyExists(this IValidationPlan plan, IEnumerable<int> currencyIds)
        => plan.EnsureExists<Currency, int>(x => x.Id, currencyIds, typeof(CurrencyNotFoundException));
    
    //ARTICLE
    public static IValidationPlan EnsureArticleExists(this IValidationPlan plan, int articleId)
        => plan.EnsureExists<Article, int>(x => x.Id, articleId, typeof(ArticleNotFoundException));
    
    public static IValidationPlan EnsureArticleExists(this IValidationPlan plan, IEnumerable<int> articleIds)
        => plan.EnsureExists<Article, int>(x => x.Id, articleIds, typeof(ArticleNotFoundException));
    
    //STORAGE
    public static IValidationPlan EnsureStorageExists(this IValidationPlan plan, string storageName)
        => plan.EnsureExists<Storage, string>(x => x.Name, storageName, typeof(StorageNotFoundException));
    
    public static IValidationPlan EnsureStorageExists(this IValidationPlan plan, IEnumerable<string> storageNames)
        => plan.EnsureExists<Storage, string>(x => x.Name, storageNames, typeof(StorageNotFoundException));
    
    //STORAGE CONTENT
    
    
    //PRODUCER
    public static IValidationPlan EnsureProducerExists(this IValidationPlan plan, int producerId)
        => plan.EnsureExists<Producer, int>(x => x.Id, producerId, typeof(ProducerNotFoundException));
    
    public static IValidationPlan EnsureProducerExists(this IValidationPlan plan, IEnumerable<int> producerIds)
        => plan.EnsureExists<Producer, int>(x => x.Id, producerIds, typeof(ProducerNotFoundException));
    
    //TRANSACTION
    public static IValidationPlan EnsureTransactionExists(this IValidationPlan plan, string transactionId)
        => plan.EnsureExists<Transaction, string>(x => x.Id, transactionId, typeof(TransactionNotFound));
    
    public static IValidationPlan EnsureTransactionExists(this IValidationPlan plan, IEnumerable<string> transactionIds)
        => plan.EnsureExists<Transaction, string>(x => x.Id, transactionIds, typeof(TransactionNotFound));
    
    //PERMISSION
    
    public static IValidationPlan EnsurePermissionExists(this IValidationPlan plan, string name)
        => plan.EnsureExists<Permission, string>(x => x.Name, name, typeof(PermissionNotFoundException));
    
    //ROLE
    
    public static IValidationPlan EnsureRoleExists(this IValidationPlan plan, Guid roleId)
        => plan.EnsureExists<Role, Guid>(x => x.Id, roleId, typeof(RoleNotFoundException));
}
