using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace RiskyRails.GameCode.Utilities
{
    public static class IsometricConverter
    {
        //розміри тайлів
        public const int TileWidth = 64;
        public const int TileHeight = 32;

        //перетворення грид-координат в ізометричні
        public static Vector2 GridToIso(Vector2 gridPos)
        {
            return new Vector2(
                (gridPos.X - gridPos.Y) * TileWidth / 2,
                (gridPos.X + gridPos.Y) * TileHeight / 2
            );
        }

        //зворотнє перетворення
        public static Vector2 IsoToGrid(Vector2 isoPos)
        {
            return new Vector2(
                (isoPos.X / (TileWidth / 2) + isoPos.Y / (TileHeight / 2)) / 2,
                (isoPos.Y / (TileHeight / 2) - isoPos.X / (TileWidth / 2)) / 2
            );
        }
        public static float CalculateDepth(Vector2 gridPosition)
        {
            return (gridPosition.X + gridPosition.Y) * 0.001f;
        }
    }
}
