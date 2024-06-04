using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

[DisallowMultipleComponent]
public class ClientNetworkTransform : NetworkTransform
{
    public AuthorityMode authorityMode = AuthorityMode.Client;

    //protected override bool OnIsServerAuthoritative() => authorityMode == AuthorityMode.Server;
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}

public enum AuthorityMode
{
    Server, Client
}