using App.Shared.MessagePack;

namespace App.Shared.Reversi
{
    // クライアント・サーバーの共通定義
    public static class ReversiDefine
    {
        public enum StoneColor : short
        {
            None = 0,
            Black,
            White,
        }
        
        // 相対位置
        private static readonly Point LeftUp = new Point(-1, -1);
        private static readonly Point Up = new Point(0, -1);
        private static readonly Point RightUp = new Point(1, -1);
        
        private static readonly Point Left = new Point(-1, 0);
        private static readonly Point Right = new Point(1, 0);
        
        private static readonly Point LeftDown = new Point(-1, 1);
        private static readonly Point Down = new Point(0, 1);
        private static readonly Point RightDown = new Point(1, 1);

        // 全方向探索用
        public static readonly Point[] AllDirectionPoints = new Point[8]
        {
            LeftUp,
            Up,
            RightUp,
            Left,
            Right,
            LeftDown,
            Down,
            RightDown
        };
        
        public const short Width = 8;
        public const short Height = 8;
    }
}
