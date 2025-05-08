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
            var station2 = new Station { GridPosition = new Vector2(8, 2), Name = "East Station" };
            var station3 = new Station { GridPosition = new Vector2(8, 8), Name = "South Station" };
            var station4 = new Station { GridPosition = new Vector2(2, 8), Name = "West Station" };
            AddStation(station1);
            AddStation(station2);
            AddStation(station3);
            AddStation(station4);

            //основні колії
            CreateStraightTrack(station1, new Vector2(3, 2), station2);
            CreateCurvedTrack(station2, new Vector2(8, 3), station3);
            CreateStraightTrack(station3, new Vector2(7, 8), station4);
            CreateCurvedTrack(station4, new Vector2(2, 7), station1);

            //стрілка
            var switchTrack = new TrackSegment
            {
                GridPosition = new Vector2(5, 5),
                IsSwitch = true
            };
            AddTrack(switchTrack);

            //стрілку до шляху
            ConnectTracks(Tracks[4], switchTrack);
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
