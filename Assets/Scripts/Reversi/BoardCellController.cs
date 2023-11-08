using System;
using App.Shared.MessagePack;
using App.Shared.Reversi;
using UnityEngine;

public class BoardCellController : MonoBehaviour, IClickObject
{
    [SerializeField] private Renderer cellRenderer = null;
    [SerializeField] private StoneController stoneController = null;

    private Material cellMaterial = null;
    private Color defaultCellColor;
    private Point cellPoint;

    private Action<Point> onTapCellAction = null;

    public Point CellPoint => cellPoint;
    private bool isPlaceable = false;

    private bool isClickEnable = false;

    public void Initialize(Point point, Action<Point> onTapCellAction)
    {
        cellMaterial = cellRenderer.material;
        defaultCellColor = cellMaterial.color;
        
        cellPoint = point;
        this.onTapCellAction = onTapCellAction;
        SetColor(ReversiDefine.StoneColor.None);
    }

    /// <summary>
    /// 石を配置できるか設定
    /// </summary>
    public void SetStonePlaceableCell(bool isPlaceable)
    {
        cellMaterial.color = isPlaceable ? Color.green : defaultCellColor;
        this.isPlaceable = isPlaceable;
    }

    /// <summary>
    /// クリックできるか設定
    /// </summary>
    public void SetClickEnable(bool isEnable)
    {
        isClickEnable = isEnable;
    }

    /// <summary>
    /// 石の色をセット(置くだけでアニメーションなし)
    /// </summary>
    /// <param name="color"></param>
    public void SetColor(ReversiDefine.StoneColor color)
    {
        stoneController.SetInitColor(color);
    }

    /// <summary>
    /// 石をひっくり返す
    /// </summary>
    public void OnReverse(ReversiDefine.StoneColor reverseColor, Action onReverseCompleted)
    {
        switch (reverseColor)
        {
            case ReversiDefine.StoneColor.Black:
                stoneController.OnReverseBlack(onReverseCompleted);
                break;
            case ReversiDefine.StoneColor.White:
                stoneController.OnReverseWhite(onReverseCompleted);
                break;
        }
    }

    /// <summary>
    /// マスがタップされた
    /// </summary>
    public void OnClickObject()
    {
        if (!isClickEnable)
            return;
        if (!isPlaceable)
            return;
        onTapCellAction?.Invoke(CellPoint);
    }
}
