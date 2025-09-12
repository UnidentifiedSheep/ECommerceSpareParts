using Exceptions.Base;
using Exceptions.Exceptions;

namespace Core.Exceptions.Sales;

public class SaleContentPriceOrCountException() : BadRequestException(@"Цена или количество не может быть отрицательным или нулевым. Так же цена со скидкой не может быть больше чем цена без скидки")
{
    
}