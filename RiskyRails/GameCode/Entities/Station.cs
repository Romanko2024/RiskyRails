using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using System.Diagnostics;


namespace RiskyRails.GameCode.Entities
{
    public class Station : TrackSegment
    {
        public string Name { get; set; } = "Unnamed Station";
        public List<Train> SpawnedTrains { get; } = new();

        public override List<Vector2> GetConnectionPoints()
        {
            return new List<Vector2>
        {
            Vector2.UnitX,
            -Vector2.UnitX,
            Vector2.UnitY,
            -Vector2.UnitY
        };
        }
    }
}
