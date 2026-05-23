using Exceptions.Base.Localized;

namespace Main.Entities.Exceptions.Sales;

public class SaleContentNotFoundException(int id)
    : LocalizedBadRequestException("sale.content.not.found", new { Id = id });

public class SaleNotFoundException(Guid id)
    : LocalizedNotFoundException("sale.not.found", new { Id = id });

public class SaleSoftConfirmationNeededException(string confirmationCode, Dictionary<string, int> reserved)
    : LocalizedPreconditionRequiredException(
        "soft.confirmation.needed.for.sale.reservation.reason",
        new { ConfirmationCode = confirmationCode, Reserved = reserved },
        [confirmationCode]);
