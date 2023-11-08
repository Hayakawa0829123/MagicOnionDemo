using System.Collections;
using System.Collections.Generic;
using MessagePack.Resolvers;
using UnityEngine;

public class RuntimeInitializeOnLoad : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Initialize()
    {
        var managers = (GameObject)Resources.Load("Managers");
        Instantiate(managers);
        // var managerObj = new GameObject("Managers");
        // managerObj.AddComponent<DontDestroyOnLoadObject>();
        // var magicOnion = new GameObject("MagicOnionManager");
        // magicOnion.transform.parent = managerObj.transform;
        // magicOnion.AddComponent<MagicOnionManager>();
    }
}
