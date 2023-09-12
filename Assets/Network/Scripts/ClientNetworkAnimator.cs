using System.Collections;
using System.Collections.Generic;
using Unity.Netcode.Components;
using Unity.Netcode;
using UnityEngine;

public class ClientNetworkAnimator : NetworkAnimator
{

    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }

}
