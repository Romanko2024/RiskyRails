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
        public List<Level> Levels { get; } = new();
        public Level CurrentLevel { get; private set; }

        public void Initialize()
        {
            //створення рівнів
            var level1 = new Level();
            level1.GenerateTestLevel();
            Levels.Add(level1);
            LoadLevel(0);
        }

        public void LoadLevel(int levelIndex)
        {
            if (levelIndex >= 0 && levelIndex < Levels.Count)
            {
                CurrentLevel = Levels[levelIndex];
            }
        }

        public Queue<TrackSegment> FindPath(Station start, Station end)
        {
            var visited = new Dictionary<TrackSegment, TrackSegment>();
            var queue = new Queue<TrackSegment>();
            queue.Enqueue(start);
            visited[start] = null;

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (current == end) break;

                foreach (var neighbor in current.ConnectedSegments.Where(t => t.CanPassThrough(null)))
                {
                    if (!visited.ContainsKey(neighbor))
                    {
                        visited[neighbor] = current;
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
                node = visited.ContainsKey(node) ? visited[node] : null;
            }

            var result = new Queue<TrackSegment>();
            while (path.Count > 0) result.Enqueue(path.Pop());
            return result;
        }
        //ConnectTracks не потрібний тк логіка з'єднання в класі Level
    }
}
