using FandomApi;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor.PackageManager;
using UnityEngine;

public class FandomLoginParameters
{
    public string Username { get; set; }
    public string Password { get; set; }
}

public class FandomLoginResponse
{
    public bool Success { get; set; }
}

public class FandomLoginService : IFandomService<FandomLoginParameters, FandomLoginResponse>
{
    public FandomLoginResponse Response { get; set; }
    public FandomLoginParameters Parameters { get; set; }

    public async Task Execute(FandomApiClient client)
    {
        var result = await client.LoginAsync(Parameters.Username, Parameters.Password);
        Response = new FandomLoginResponse() { Success = result };
    }
}
