using Exceptions.Base;

namespace Exceptions.Exceptions.Users;

public class UserAlreadyContainsRoleException(Guid userId, string role) 
    : ConflictException("Данная роль уже есть у пользователя.", new { UserId = userId, Role = role}) { }