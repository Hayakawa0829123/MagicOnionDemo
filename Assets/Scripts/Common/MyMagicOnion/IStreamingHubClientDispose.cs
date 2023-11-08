using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// MagicOnionManager内でStreamingHubを解放するためのクラス
/// </summary>
public interface IStreamingHubClientDispose
{
    public Task DisposeHub();
}
