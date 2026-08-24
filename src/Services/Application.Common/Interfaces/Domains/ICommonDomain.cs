namespace Application.Common.Interfaces.Domains;

public interface ICommonDomain;

public interface ICommonDomainMarker<TDomain>
    where TDomain : ICommonDomain;
