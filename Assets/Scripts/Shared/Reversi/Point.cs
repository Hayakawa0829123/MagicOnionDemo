using System.Collections;
using System.Collections.Generic;
using MessagePack;
using UnityEngine;

namespace App.Shared.MessagePack
{
    // リバーシのマス計算用
    [MessagePackObject()]
    public class Point
    {
        [Key(0)] public short x;
        [Key(1)] public short y;

        public Point(short x, short y)
        {
            this.x = x;
            this.y = y;
        }

        public Point Add(Point add)
        {
            return new Point((short)(x + add.x), (short)(y + add.y));
        }

        public bool Match(Point point)
        {
            return point.x == x && point.y == y;
        }
    }
}

