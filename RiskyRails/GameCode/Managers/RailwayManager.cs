using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using RiskyRails.GameCode.Entities;

namespace RiskyRails.GameCode.Managers
{
    public class RailwayManager
    {
        public List<TrackSegment> Tracks { get; } = new();
        public List<Station> Stations { get; } = new();

        public void GenerateTestMap()
        {
            //створення станцій
            var station1 = new Station { GridPosition = new Vector2(2, 2), Name = "Central Station" };
            var station2 = new Station { GridPosition = new Vector2(8, 8), Name = "East Station" };

            //з'єднання станцій
            ConnectTracks(station1, station2);

            Tracks.AddRange(new[] { station1, station2 });
            Stations.AddRange(new[] { station1, station2 });
        }

        private void ConnectTracks(TrackSegment a, TrackSegment b)
        {
            a.ConnectedSegments.Add(b);
            b.ConnectedSegments.Add(a);
        }
    }
}
