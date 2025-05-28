using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            Debug.WriteLine($"Шукаємо шлях від {start.Name} до {end.Name}...");

            var visited = new Dictionary<TrackSegment, TrackSegment>();
            var queue = new Queue<TrackSegment>();
            queue.Enqueue(start);
            visited[start] = null;

            bool pathFound = false;

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                if (current == end)
                {
                    pathFound = true;
                    break;
                }

                foreach (var neighbor in current.ConnectedSegments.Where(t => t.CanPassThrough(null)))
                {
                    if (current is SwitchTrack switchTrack)
                    {
                        Vector2 dirToNeighbor = neighbor.GridPosition - current.GridPosition;
                        if (!switchTrack.GetConnectionPoints().Contains(dirToNeighbor))
                        {
                            Debug.WriteLine($"Пропускаємо сусіда {neighbor.GridPosition} через напрямок стрілки");
                            continue;
                        }
                    }

                    if (!visited.ContainsKey(neighbor))
                    {
                        visited[neighbor] = current;
                        queue.Enqueue(neighbor);
                        Debug.WriteLine($"Додано в шлях: {neighbor.GridPosition}");
                    }
                }
            }

            if (!pathFound)
            {
                Debug.WriteLine($"Шлях від {start.Name} до {end.Name} не знайдено!");
                return null;
            }

            // Відновлення шляху
            var path = new Stack<TrackSegment>();
            TrackSegment node = end;
            while (node != null && node != start)
            {
                path.Push(node);
                node = visited[node];
            }

            var result = new Queue<TrackSegment>(path);

            Debug.WriteLine($"Знайдено шлях від {start.Name} до {end.Name}: {result.Count} сегментів");
            return result;
        }
        //ConnectTracks не потрібний тк логіка з'єднання в класі Level
    }
}
