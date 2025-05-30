using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using RiskyRails.GameCode.Entities;
using RiskyRails.GameCode.Entities.Trains;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;

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

        public Queue<TrackSegment> FindPath(TrackSegment start, TrackSegment end, Train train = null)
        {
            Debug.WriteLine($"=== ПОЧАТОК ПОШУКУ ШЛЯХУ ===");
            Debug.WriteLine($"Від: {start?.GridPosition} До: {end?.GridPosition} Для: {train?.GetType().Name}");

            var visited = new Dictionary<TrackSegment, TrackSegment>();
            var queue = new Queue<TrackSegment>();
            queue.Enqueue(start);
            visited[start] = null;

            bool pathFound = false;

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                Debug.WriteLine($"Поточний сегмент: {current.GridPosition}");

                if (current == end)
                {
                    Debug.WriteLine($"Досягнуто ціль: {end.GridPosition}");
                    pathFound = true;
                    break;
                }

                foreach (var neighbor in current.ConnectedSegments)
                {
                    Debug.WriteLine($" Перевірка сусіда: {neighbor.GridPosition}");

                    //перевірка можливості проходу
                    if (!neighbor.CanPassThrough(train))
                    {
                        Debug.WriteLine($"  - Не можна пройти через {neighbor.GridPosition}");
                        continue;
                    }

                    //перевірка стрілок
                    if (current is SwitchTrack switchTrack)
                    {
                        Vector2 dirToNeighbor = neighbor.GridPosition - current.GridPosition;
                        if (!switchTrack.GetConnectionPoints().Contains(dirToNeighbor))
                        {
                            Debug.WriteLine($"  - Пропущено через напрямок стрілки");
                            continue;
                        }
                    }

                    if (!visited.ContainsKey(neighbor))
                    {
                        visited[neighbor] = current;
                        queue.Enqueue(neighbor);
                        Debug.WriteLine($"  - Додано до черги: {neighbor.GridPosition}");
                    }
                }
            }

            if (!pathFound)
            {
                Debug.WriteLine($"ШЛЯХ НЕ ЗНАЙДЕНО! Кінцева точка: {end.GridPosition}");
                Debug.WriteLine($"Відвідані сегменти: {string.Join(", ", visited.Keys.Select(v => v.GridPosition))}");
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
            Debug.WriteLine($"Знайдено шлях: {string.Join(" -> ", result.Select(r => r.GridPosition))}");
            Debug.WriteLine($"=== КІНЕЦЬ ПОШУКУ ШЛЯХУ ===");

            return result;
        }
        //ConnectTracks не потрібний тк логіка з'єднання в класі Level
    }
}
