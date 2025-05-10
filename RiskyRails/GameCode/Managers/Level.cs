using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using RiskyRails.GameCode.Entities;
using static RiskyRails.GameCode.Entities.TrackSegment;

namespace RiskyRails.GameCode.Managers
{
    public class Level
    {
        public List<TrackSegment> Tracks { get; } = new();
        public List<Station> Stations { get; } = new();
        public List<Signal> Signals { get; } = new();

        private enum Direction { North, East, South, West }

        public void GenerateTestLevel()
        {
            Tracks.Clear();
            Stations.Clear();
            Signals.Clear();

            //станції
            var station1 = new Station { GridPosition = new Vector2(5, 5), Name = "Станція А" };
            var station2 = new Station { GridPosition = new Vector2(15, 5), Name = "Станція Б" };
            AddStation(station1);
            AddStation(station2);

            //пряма лінія
            CreateRailLine(station1.GridPosition, station2.GridPosition, Direction.East, TrackSegment.TrackType.StraightX);

            //шлях через повороти
            CreateRailLine(new Vector2(5, 6), new Vector2(5, 11), Direction.South, TrackSegment.TrackType.StraightY);
            AddCurve(new Vector2(5, 11), TrackSegment.TrackType.CurveSE);
            CreateRailLine(new Vector2(6, 11), new Vector2(15, 11), Direction.East, TrackSegment.TrackType.StraightX);
            AddCurve(new Vector2(15, 11), TrackSegment.TrackType.CurveSW);
            CreateRailLine(new Vector2(15, 10), station2.GridPosition + new Vector2(0, -1), Direction.North, TrackSegment.TrackType.StraightY);

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

        private void AddCurve(Vector2 position, TrackSegment.TrackType type)
        {
            var curve = new TrackSegment
            {
                GridPosition = position,
                Type = type
            };
            AddTrack(curve);
        }

        private void ConnectAllSegments()
        {
            foreach (var track in Tracks)
            {
                foreach (var direction in track.GetConnectionPoints())
                {
                    var neighborPos = track.GridPosition + direction;
                    var neighbor = Tracks.FirstOrDefault(t => t.GridPosition == neighborPos);

                    if (neighbor != null && track.CanConnectTo(neighbor, direction))
                    {
                        track.ConnectTo(neighbor);
                    }
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
