using FandomApi;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <typeparam name="T">Paramters model</typeparam>
/// <typeparam name="U">Reponse model</typeparam>
public interface IFandomService<T, U> : IFandomService<T>
{
    U Response { get; set; }
}

/// <typeparam name="T">Paramters model</typeparam>
public interface IFandomService<T> : IFandomService
{
    T Parameters { get; set; }
}

public interface IFandomService
{
    Task Execute(FandomApiClient client);
}
