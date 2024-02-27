using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;

namespace BridgeApp;

class GraphClient
{
    private readonly GraphServiceClient graph;
    private readonly string clientId;
    private readonly string tenantId;
    private readonly IEnumerable<string> userScopes;
    private readonly TokenCredential credential;
    
    public GraphClient(IConfiguration config)
    {
        clientId = config.GetValue<string>("graph:clientId") ?? throw new Exception("Missing required config: graph:clientId");
        tenantId = config.GetValue<string>("graph:tenantId") ?? throw new Exception("Missing required config: graph:tenantId");
        userScopes = config.GetSection("graph:userScopes").Get<IEnumerable<string>>() ?? throw new Exception("Missing required config: graph:userScopes");

        credential = new DeviceCodeCredential(new DeviceCodeCredentialOptions
        {
            ClientId = clientId,
            TenantId = tenantId,
            DeviceCodeCallback = DeviceCodePrompt
        });
        
        graph = new GraphServiceClient(credential, config.GetValue<IEnumerable<string>>("graph:userScopes"));
    }

    public async Task Init(CancellationToken cancellationToken)
    {
        var context = new TokenRequestContext(userScopes.ToArray());
        var response = await credential.GetTokenAsync(context, cancellationToken);

        Console.WriteLine(response.Token);
    }

    private async Task DeviceCodePrompt(DeviceCodeInfo info, CancellationToken cancellationToken)
    {
        Console.WriteLine(info.Message);
    }
}