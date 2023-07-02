using FandomApi;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class FandomPostParameters
{
    public string Title { get; set; }
    public string Text { get; set; }
}

public class FandomPostResponse
{
    public bool Success { get; set; }
}

public class FandomPostService : IFandomService<FandomPostParameters, FandomPostResponse>
{
    public FandomPostResponse Response { get; set; }
    public FandomPostParameters Parameters { get; set; }

    public async Task Execute(FandomApiClient client)
    {
        var token = await client.GetCSRFToken();
        var result = await client.EditAsync(new FandomApi.Model.EditDto(Parameters.Title, Parameters.Text, token));
        Response = new FandomPostResponse() { Success = result };
    }
}
