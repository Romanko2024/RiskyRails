using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using RiskyRails.GameCode.Entities;

namespace RiskyRails.GameCode.Managers
{
    public class Level
    {
        public List<TrackSegment> Tracks { get; } = new();
        public List<Station> Stations { get; } = new();
        public List<Signal> Signals { get; } = new();

        private enum Direction { North, East, South, West }

        public void GenerateLevel1()
        {
            //очищаємо попередні дані
            Tracks.Clear();
            Stations.Clear();
            Signals.Clear();

            //станції
            var station1 = new Station { GridPosition = new Vector2(2, 2), Name = "North Station" };
            var station2 = new Station { GridPosition = new Vector2(14, 2), Name = "East Station" };
            var station3 = new Station { GridPosition = new Vector2(14, 14), Name = "South Station" };
            var station4 = new Station { GridPosition = new Vector2(2, 14), Name = "West Station" };

            AddStation(station1);
            AddStation(station2);
            AddStation(station3);
            AddStation(station4);

            //колії з поворотами
            CreateRailLine(station1.GridPosition, new Vector2(13, 2), Direction.East, TrackSegment.TrackType.StraightX);
            AddCurve(station2.GridPosition + new Vector2(0, 1), TrackSegment.TrackType.CurveSE);
            CreateRailLine(new Vector2(14, 3), new Vector2(14, 13), Direction.South, TrackSegment.TrackType.StraightY);
            AddCurve(station3.GridPosition + new Vector2(-1, 0), TrackSegment.TrackType.CurveSW);
            CreateRailLine(new Vector2(13, 14), new Vector2(3, 14), Direction.West, TrackSegment.TrackType.StraightX);
            AddCurve(station4.GridPosition + new Vector2(0, -1), TrackSegment.TrackType.CurveNW);
            CreateRailLine(new Vector2(2, 13), new Vector2(2, 3), Direction.North, TrackSegment.TrackType.StraightY);
            AddCurve(station1.GridPosition + new Vector2(1, 0), TrackSegment.TrackType.CurveNE);

            ConnectAllSegments();
        }

        private void CreateRailLine(Vector2 start, Vector2 end, Direction dir, TrackSegment.TrackType type)
        {
            var step = dir switch
            {
                Direction.East => new Vector2(1, 0),
                Direction.West => new Vector2(-1, 0),
                Direction.North => new Vector2(0, -1),
                Direction.South => new Vector2(0, 1),
                _ => Vector2.Zero
            };

            var current = start;
            while (current != end)
            {
                var segment = new TrackSegment
                {
                    GridPosition = current,
                    Type = type
                };
                AddTrack(segment);
                current += step;
            }
        }

        private void AddCurve(Vector2 position, TrackSegment.TrackType curveType)
        {
            var curve = new TrackSegment
            {
                GridPosition = position,
                Type = curveType
            };
            AddTrack(curve);
        }

        private void ConnectAllSegments()
        {
            foreach (var track in Tracks)
            {
                foreach (var direction in new[]
                {
                    Vector2.UnitX, -Vector2.UnitX,
                    Vector2.UnitY, -Vector2.UnitY
                })
                {
                    var neighborPos = track.GridPosition + direction;
                    var neighbor = Tracks.Find(t => t.GridPosition == neighborPos);
                    if (neighbor != null) track.ConnectTo(neighbor);
                }
            }
        }

        private void AddStation(Station station)
        {
            Stations.Add(station);
            Tracks.Add(station);
        }

        private void AddTrack(TrackSegment track)
        {
            Tracks.Add(track);
        }

        private void ConnectTracks(TrackSegment a, TrackSegment b)
        {
            a.ConnectedSegments.Add(b);
            b.ConnectedSegments.Add(a);
        }
    }
}
