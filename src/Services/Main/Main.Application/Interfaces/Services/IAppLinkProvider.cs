using Main.Application.Interfaces.Services.Document;

namespace Main.Application.Interfaces.Services;

public interface IAppLinkProvider : IAuthLinkProvider, IDocumentUrlProvider;
