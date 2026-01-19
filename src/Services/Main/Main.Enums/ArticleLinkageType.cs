namespace Main.Enums;

public enum ArticleLinkageType
{
    /// <summary>
    ///     Будет создана связь только Артикула с Кросс-Артикулом и наоборот, их кросс-артикулы не будут затронуты.
    ///     Article --> CrossArticle, CrossArticle --> Article
    /// </summary>
    SingleCross,

    /// <summary>
    ///     Будет создана связь всех КРОССАРТИКУЛОВ Article с КРОССАРТИКУЛОВ ArticleCross.
    ///     1Article --> 1CrossArticle
    ///     1Article --> 2CrossArticle
    ///     etc...
    /// </summary>
    FullCross,

    /// <summary>
    ///     Будет создана связь для всех КРОССАРТИКУЛОВ Article с CrossArticle.
    ///     1Article --> CrossArticle
    ///     2Article --> CrossArticle
    /// </summary>
    FullLeftToRightCross,

    /// <summary>
    ///     Будет создана связь для Article со всеми КРОССАРТИКУЛАМИ CrossArticle.
    ///     Article --> 1CrossArticle
    ///     Article --> 2CrossArticle
    /// </summary>
    FullRightToLeftCross
}