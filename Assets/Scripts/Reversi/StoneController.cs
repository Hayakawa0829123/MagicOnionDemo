using System;
using App.Shared.Reversi;
using DG.Tweening;
using UnityEngine;

public class StoneController : MonoBehaviour
{
    [SerializeField] private float duration = 0.5f;
    
    public void SetInitColor(ReversiDefine.StoneColor color)
    {
        switch (color)
        {
            case ReversiDefine.StoneColor.Black:
                transform.rotation = Quaternion.Euler(0, 0, 0);
                break;
            case ReversiDefine.StoneColor.White:
                transform.rotation = Quaternion.Euler(180, 0, 0);
                break;
            case ReversiDefine.StoneColor.None:
                gameObject.SetActive(false);
                return;
        }
        gameObject.SetActive(true);
    }

    public void OnReverseBlack(Action onReverseCompleted)
    {
        var reverseBlackSequence = DOTween.Sequence()
            .Append(transform.DOJump(transform.position, // 移動終了地点
                2f, // ジャンプの高さ
                1, // ジャンプの総数
                duration // 演出時間
            ))
            .Join(transform.DORotate(
                new Vector3(0, 0, 0), // 終了時のRotation
                duration // 演出時間
            ));
        
        reverseBlackSequence.Play().OnComplete(() =>
        {
            onReverseCompleted?.Invoke();
        });
    }

    public void OnReverseWhite(Action onReverseCompleted)
    {
        var reverseWhiteSequence = DOTween.Sequence()
            .Append(transform.DOJump(transform.position, // 移動終了地点
                2f, // ジャンプの高さ
                1, // ジャンプの総数
                duration // 演出時間
            ))
            .Join(transform.DORotate(
                new Vector3(180, 0, 0), // 終了時のRotation
                duration // 演出時間
            ));
        
        reverseWhiteSequence.Play().OnComplete(() =>
        {
            onReverseCompleted?.Invoke();
        });
    }
}
