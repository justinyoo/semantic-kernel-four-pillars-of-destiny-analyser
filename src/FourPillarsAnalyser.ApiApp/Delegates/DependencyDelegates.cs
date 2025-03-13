using Azure.Core;
using Azure.Identity;

using Microsoft.SemanticKernel.Plugins.Core.CodeInterpreter;

namespace FourPillarsAnalyser.ApiApp.Delegates;

public static class DependencyDelegates
{
    public static SessionsPythonPlugin GetSessionsPythonPluginAsync(IServiceProvider sp, IConfiguration config)
    {
        var settings = new SessionsPythonSettings(
            sessionId: Guid.NewGuid().ToString(),
            endpoint: new Uri(config["Azure:ContainerApps:PoolManagement:Endpoint"]!));
        
        var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
        var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

        var plugin = new SessionsPythonPlugin(
            settings: settings,
            httpClientFactory: httpClientFactory,
            authTokenProvider: GetAuthTokenAsync,
            loggerFactory: loggerFactory);

        return plugin;
    }

    private static async Task<string> GetAuthTokenAsync()
    {
        var resource = "https://acasessions.io/.default";
#if DEBUG
        var credential = new AzureCliCredential();
#else
        var credential = new DefaultAzureCredential();
#endif
        var context = new TokenRequestContext(scopes: [ resource ]);
        var accessToken = await credential.GetTokenAsync(context).ConfigureAwait(false);

        return accessToken.Token;
    }
}
