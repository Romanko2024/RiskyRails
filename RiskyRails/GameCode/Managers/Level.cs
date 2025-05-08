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

            //має (напевно) прокладати повноцінні шляхи
            CreateRailLine(station1, station2, Direction.East);
            CreateRailLine(station2, station3, Direction.South);
            CreateRailLine(station3, station4, Direction.West);
            CreateRailLine(station4, station1, Direction.North);
        }

        private enum Direction { North, East, South, West }

        private void CreateRailLine(Station start, Station end, Direction dir)
        {
            var currentPos = start.GridPosition;
            var step = Vector2.Zero;

            //крок руху
            switch (dir)
            {
                case Direction.East: step = new Vector2(1, 0); break;
                case Direction.West: step = new Vector2(-1, 0); break;
                case Direction.North: step = new Vector2(0, -1); break;
                case Direction.South: step = new Vector2(0, 1); break;
            }

            //генерація проміжних сегментів
            while (currentPos != end.GridPosition)
            {
                currentPos += step;

                //пропуск старт станції
                if (currentPos == start.GridPosition) continue;

                var segment = new TrackSegment { GridPosition = currentPos };

                //додаємо світлофор кожні 3 тайли
                if ((currentPos.X + currentPos.Y) % 3 == 0)
                {
                    segment.Signal = new Signal();
                    Signals.Add(segment.Signal);
                }

                AddTrack(segment);
                ConnectToPrevious(segment);
            }

            //з'єднання кінцевої станції
            ConnectTracks(Tracks.Last(), end);
        }

        private void ConnectToPrevious(TrackSegment segment)
        {
            var prev = Tracks.LastOrDefault();
            if (prev != null) ConnectTracks(prev, segment);
        }

        private void CreateStraightTrack(Station start, Vector2 middlePos, Station end)
        {
            var middleTrack = new TrackSegment { GridPosition = middlePos };
            AddTrack(middleTrack);

            ConnectTracks(start, middleTrack);
            ConnectTracks(middleTrack, end);
        }

        private void CreateCurvedTrack(Station start, Vector2 curvePos, Station end)
        {
            var curveTrack = new TrackSegment { GridPosition = curvePos };
            var signal = new Signal();

            curveTrack.Signal = signal;
            AddTrack(curveTrack);
            Signals.Add(signal);

            ConnectTracks(start, curveTrack);
            ConnectTracks(curveTrack, end);
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
