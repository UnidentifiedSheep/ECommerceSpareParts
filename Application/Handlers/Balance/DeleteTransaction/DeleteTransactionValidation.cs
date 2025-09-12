using FluentValidation;

namespace Application.Handlers.Balance.DeleteTransaction;

public class DeleteTransactionValidation : AbstractValidator<DeleteTransactionCommand>
{
    public DeleteTransactionValidation()
    {
        RuleFor(x => x.TransactionId).NotEmpty()
            .WithMessage("Id Транзакции не может быть пуст.");
        RuleFor(x => x.WhoDeleteUserId).NotEmpty()
            .WithMessage("Id пользователя который удаляет транзакцию не может быть пуст.");
    }
}