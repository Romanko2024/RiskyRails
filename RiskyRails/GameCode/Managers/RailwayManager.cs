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
            Tracks.Add(station1); //додаємо станції до Tracks
            Tracks.Add(station2);
            Stations.Add(station1); //і до окремої колкц станцій
            Stations.Add(station2);
            //з'єднання станцій
            ConnectTracks(station1, station2);
        }

        private void ConnectTracks(TrackSegment a, TrackSegment b)
        {
            a.ConnectedSegments.Add(b);
            b.ConnectedSegments.Add(a);
        }
        public Queue<TrackSegment> FindPath(Station start, Station end)
        {
            var queue = new Queue<TrackSegment>();
            var visited = new HashSet<TrackSegment>();
            var parent = new Dictionary<TrackSegment, TrackSegment>();

            queue.Enqueue(start);
            visited.Add(start);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (current == end) break;

                foreach (var neighbor in current.ConnectedSegments)
                {
                    if (!visited.Contains(neighbor) && !neighbor.IsDamaged)
                    {
                        visited.Add(neighbor);
                        parent[neighbor] = current;
                        queue.Enqueue(neighbor);
                    }
                }
            }

            //відновлення шляху
            var path = new Stack<TrackSegment>();
            TrackSegment node = end; //змінено з 'var' на явний тип
            while (node != null && node != start)
            {
                path.Push(node);
                node = parent.ContainsKey(node) ? parent[node] : null;
            }

            var result = new Queue<TrackSegment>();
            while (path.Count > 0) result.Enqueue(path.Pop());
            return result;
        }
    }
}
