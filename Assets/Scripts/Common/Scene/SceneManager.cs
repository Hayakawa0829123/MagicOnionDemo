using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class SceneManager : SingletonMonoBehaviour<SceneManager>
{
    public enum SceneState
    {
        Lobby = 0,
        Game,
    }

    private SceneState currentState = SceneState.Lobby;

    protected override void doAwake()
    {
        
    }

    public async UniTask ChangeScene(SceneState state)
    {
        if (currentState == state)
            return;
        await UnityEngine.SceneManagement.SceneManager.LoadSceneAsync((int)state);
        currentState = state;
    }
}
