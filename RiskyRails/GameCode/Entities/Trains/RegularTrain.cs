using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;
using RiskyRails.GameCode.Entities;
using RiskyRails.GameCode.Managers;
namespace RiskyRails.GameCode.Entities.Trains
{
    /// <summary>
    /// поїзд, що дотримується правил
    /// </summary>
    public class RegularTrain : Train
    {
        public Station Destination { get; set; }
        private float _progress; // прогрес руху між сегментами (0-1)
        private float _pathUpdateTimer;
        private const float PathUpdateInterval = 2.0f;
        private readonly RailwayManager _railwayManager;

        public RegularTrain(RailwayManager railwayManager)
        {
            _railwayManager = railwayManager;
            Speed = 0.3f;
        }

        public override void Update(GameTime gameTime)
        {
            if (!CurrentTrack.CanPassThrough(this))
            {
                IsActive = false;
                return;
            }

            //оновлення шляху
            _pathUpdateTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_pathUpdateTimer >= PathUpdateInterval)
            {
                UpdatePath();
                _pathUpdateTimer = 0;
            }

            //обробка відсутності шляху
            if (Path.Count == 0)
            {
                var availableTracks = CurrentTrack.ConnectedSegments
                    .Where(t => t.CanPassThrough(this))
                    .ToList();

                Path = availableTracks.Count > 0
                    ? new Queue<TrackSegment>(new[] { availableTracks[new Random().Next(availableTracks.Count)] })
                    : new Queue<TrackSegment>();
            }

            //
            if (Path.Count > 0)
            {
                var targetTrack = Path.Peek();

                //перевірка коректності з'єднання
                //перевірка стрілки
                if (CurrentTrack is SwitchTrack switchTrack && !switchTrack.ConnectedSegments.Contains(targetTrack))
                {
                    Speed = 0;
                    return;
                }

                //перевірка з'єднання
                if (!CurrentTrack.ConnectedSegments.Contains(targetTrack))
                {
                    Path.Clear();
                    IsActive = false;
                    return;
                }

                //рух
                _progress += Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                GridPosition = Vector2.Lerp(CurrentTrack.GridPosition, targetTrack.GridPosition, _progress);

                if (_progress >= 1.0f)
                {
                    CurrentTrack = Path.Dequeue();
                    _progress = 0;
                    GridPosition = CurrentTrack.GridPosition;
                    if (CurrentTrack == Destination) IsActive = false;
                }
            }
            else
            {
                IsActive = false;
            }
        }

        private void UpdatePath()
        {
            if (CurrentTrack is Station currentStation && Destination != null)
            {
                var newPath = _railwayManager.FindPath(currentStation, Destination);
                if (newPath != null && newPath.Count > 0)
                {
                    Path = newPath;
                }
            }
        }

        public override void HandleSignal(Signal signal)
        {
            if (!signal.IsGreen)
            {
                //зупинка на червоний сигнал
                Speed = 0;
                Path.Clear();
                IsActive = false;
            }
        }
    }
}
