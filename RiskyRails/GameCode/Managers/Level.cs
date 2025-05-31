using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public void GenerateLevel(int levelIndex)
        {
            Tracks.Clear();
            Stations.Clear();
            Signals.Clear();

            switch (levelIndex)
            {
                case 0:
                    GenerateTestLevel();
                    break;
                case 1:
                    GenerateSecondLevel();
                    break;
            }
        }
        public void GenerateTestLevel()
        {
            Tracks.Clear();
            Stations.Clear();
            Signals.Clear();

            // Станції з вирівняними координатами
            var stationA = new Station { GridPosition = new Vector2(5, 5), Name = "A" };
            var stationB = new Station { GridPosition = new Vector2(18, 5), Name = "B" };
            var stationC = new Station { GridPosition = new Vector2(12, 15), Name = "C" };
            var stationD = new Station { GridPosition = new Vector2(4, 11), Name = "D" };
            AddStation(stationA);
            AddStation(stationB);
            AddStation(stationC);
            AddStation(stationD);

            //від A до B
            for (int x = 6; x < 10; x++)
            {
                AddTrack(new TrackSegment
                {
                    GridPosition = new Vector2(x, 5),
                    Type = TrackType.StraightX
                });
            }
            AddSwitch(new Vector2(10, 5),
                TrackType.CurveSW,
                TrackType.StraightX);
            for (int x = 11; x < 14; x++)
            {
                AddTrack(new TrackSegment
                {
                    GridPosition = new Vector2(x, 5),
                    Type = TrackType.StraightX
                });
            }
            AddSwitch(new Vector2(14, 5),
                TrackType.StraightX,
                TrackType.CurveSE);
            for (int x = 15; x < 18; x++)
            {
                AddTrack(new TrackSegment
                {
                    GridPosition = new Vector2(x, 5),
                    Type = TrackType.StraightX
                });
            }
            //кишеня від A до B
            AddCurve(new Vector2(10, 6),
                TrackType.CurveNE);
            AddSignalSegment(new Vector2(11, 6), TrackType.StraightX_Signal);
            AddSignalSegment(new Vector2(13, 6), TrackType.StraightX_Signal);
            AddCurve(new Vector2(14, 6),
                TrackType.CurveNW);
            AddSwitch(new Vector2(12, 6),
                TrackType.CurveSE,
                TrackType.CurveSW);
            //від кишеня до C
            for (int y = 7; y < 10; y++)
            {
                AddTrack(new TrackSegment
                {
                    GridPosition = new Vector2(12, y),
                    Type = TrackType.StraightY
                });
            }
            AddSignalSegment(new Vector2(12, 10), TrackType.StraightY_Signal); 
            //
            AddSwitch(new Vector2(12, 11),
                TrackType.CurveSW,
                TrackType.StraightY);
            //
            for (int y = 12; y < 15; y++)
            {
                AddTrack(new TrackSegment
                {
                    GridPosition = new Vector2(12, y),
                    Type = TrackType.StraightY
                });
            }
            //до д
            for (int x = 5; x < 12; x++)
            {
                AddTrack(new TrackSegment
                {
                    GridPosition = new Vector2(x, 11),
                    Type = TrackType.StraightX
                });
            }
            //від д до першого
            for (int y = 10; y < 11; y++)
            {
                AddTrack(new TrackSegment
                {
                    GridPosition = new Vector2(4, y),
                    Type = TrackType.StraightY
                });
            }
            AddCurve(new Vector2(4, 9),
                TrackType.CurveSE);
            AddCurve(new Vector2(5, 9),
               TrackType.CurveNW);
            for (int y = 6; y < 9; y++)
            {
                AddTrack(new TrackSegment
                {
                    GridPosition = new Vector2(5, y),
                    Type = TrackType.StraightY
                });
            }


            ConnectAllSegments();

            // Фінальна перевірка коректності
            ValidateLevel();
        }

        private void GenerateSecondLevel()
        {
            //
            var stationA = new Station { GridPosition = new Vector2(3, 3), Name = "Alpha" };
            var stationB = new Station { GridPosition = new Vector2(15, 3), Name = "Beta" };
            var stationC = new Station { GridPosition = new Vector2(8, 12), Name = "Gamma" };
            AddStation(stationA);
            AddStation(stationB);
            AddStation(stationC);

            // ВИПРАВИТИ. просто натикано
            for (int x = 4; x < 15; x++)
            {
                if (x == 8 || x == 9) continue;
                AddTrack(new TrackSegment
                {
                    GridPosition = new Vector2(x, 3),
                    Type = TrackType.StraightX
                });
            }

            //
            for (int y = 4; y < 12; y++)
            {
                AddTrack(new TrackSegment
                {
                    GridPosition = new Vector2(8, y),
                    Type = TrackType.StraightY
                });
            }

            //
            AddSwitch(new Vector2(8, 3),
                TrackType.CurveSE,
                TrackType.StraightX);
            AddSwitch(new Vector2(9, 3),
                TrackType.CurveSW,
                TrackType.StraightX);
            AddCurve(new Vector2(10, 11),
                TrackType.CurveSW);

            for (int y = 4; y < 11; y++)
            {
                AddTrack(new TrackSegment
                {
                    GridPosition = new Vector2(9, y),
                    Type = TrackType.StraightY
                });
            }
            AddCurve(new Vector2(9, 11),
                TrackType.CurveNE);
            //
            for (int x = 4; x < 15; x++)
            {
                if (x == 8 || x == 9 || x == 10) continue;
                AddTrack(new TrackSegment
                {
                    GridPosition = new Vector2(x, 12),
                    Type = TrackType.StraightX
                });
            }
            AddCurve(new Vector2(10, 12),
                TrackType.CurveNE);
            AddSignalSegment(new Vector2(15, 4), TrackType.StraightY_Signal);
            for (int y = 5; y < 12; y++)
            {
                AddTrack(new TrackSegment
                {
                    GridPosition = new Vector2(15, y),
                    Type = TrackType.StraightY
                });
            }
            AddCurve(new Vector2(15, 12),
                TrackType.CurveNW);

            for (int y = 4; y < 12; y++)
            {
                AddTrack(new TrackSegment
                {
                    GridPosition = new Vector2(3, y),
                    Type = TrackType.StraightY
                });
            }
            AddCurve(new Vector2(3, 12),
                TrackType.CurveNE);

            ConnectAllSegments();
            ValidateLevel();
        }

        private void ValidateLevel()
        {
            foreach (var track in Tracks)
            {
                if (track.GridPosition.X < 0 || track.GridPosition.Y < 0)
                    throw new Exception($"Некоректні координати у сегмента {track.GridPosition}");

                foreach (var connected in track.ConnectedSegments)
                {
                    if (!connected.ConnectedSegments.Contains(track))
                        throw new Exception($"Некоректне з'єднання {track.GridPosition} -> {connected.GridPosition}");
                }
            }
        }
        private void AddSignalSegment(Vector2 position, TrackType type)
        {
            var segment = new TrackSegment
            {
                GridPosition = position,
                Type = type,
                Signal = new Signal()
            };
            AddTrack(segment);
            Signals.Add(segment.Signal);
        }

        public void AddSwitch(Vector2 position, TrackType primaryType, TrackType secondaryType)
        {
            var switchTrack = new SwitchTrack(primaryType, secondaryType)
            {
                GridPosition = position
            };
            AddTrack(switchTrack);
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

        public void ConnectAllSegments()
        {
            Debug.WriteLine("=== Початок з'єднання сегментів ===");
            foreach (var track in Tracks)
            {
                foreach (var direction in track.GetConnectionPoints())
                {
                    var neighborPos = track.GridPosition + direction;
                    var neighbor = Tracks.FirstOrDefault(t =>
                        (int)t.GridPosition.X == (int)neighborPos.X &&
                        (int)t.GridPosition.Y == (int)neighborPos.Y
                    );

                    // Додано перевірку зворотного з'єднання
                    if (neighbor != null
                        && track.CanConnectTo(neighbor, direction)
                        && neighbor.CanConnectTo(track, -direction)) // Нова перевірка
                    {
                        Debug.WriteLine($"Спроба з'єднати {track.GridPosition} з {neighbor.GridPosition}...");
                        if (!track.ConnectedSegments.Contains(neighbor))
                        {
                            track.ConnectedSegments.Add(neighbor);
                            Debug.WriteLine($"Додано з'єднання: {track.GridPosition} -> {neighbor.GridPosition}");
                        }

                        if (!neighbor.ConnectedSegments.Contains(track))
                        {
                            neighbor.ConnectedSegments.Add(track);
                            Debug.WriteLine($"Додано зворотнє з'єднання: {neighbor.GridPosition} -> {track.GridPosition}");
                        }
                        else
                        {
                            Debug.WriteLine($"Не вдалося з'єднати {track.GridPosition} з {neighborPos}: сусід {neighbor?.GridPosition} не знайдений або несумісний");
                        }
                    }
                }
            }
            Debug.WriteLine("=== Завершення з'єднання сегментів ===");
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
        public void ConnectSwitchNeighbors(SwitchTrack switchTrack)
        {
            switchTrack.ConnectedSegments.Clear();

            foreach (var direction in switchTrack.GetConnectionPoints())
            {
                Vector2 neighborPos = switchTrack.GridPosition + direction;
                TrackSegment neighbor = Tracks.FirstOrDefault(t =>
                    t.GridPosition == neighborPos);

                if (neighbor != null &&
                    switchTrack.CanConnectTo(neighbor, direction) &&
                    neighbor.CanConnectTo(switchTrack, -direction))
                {
                    switchTrack.ConnectTo(neighbor);
                }
            }

            foreach (var neighbor in Tracks)
            {
                Vector2 direction = neighbor.GridPosition - switchTrack.GridPosition;
                if (direction.Length() > 1.5f) continue; // Тільки сусідні сегменти

                if (neighbor.ConnectedSegments.Contains(switchTrack))
                {
                    if (!switchTrack.GetConnectionPoints().Contains(direction))
                    {
                        neighbor.ConnectedSegments.Remove(switchTrack);
                    }
                }
                else
                {
                    if (switchTrack.GetConnectionPoints().Contains(direction) &&
                        switchTrack.CanConnectTo(neighbor, direction) &&
                        neighbor.CanConnectTo(switchTrack, -direction))
                    {
                        neighbor.ConnectedSegments.Add(switchTrack);
                    }
                }
            }
        }
    }
}
