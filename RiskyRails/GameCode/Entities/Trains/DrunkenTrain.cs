using Microsoft.Xna.Framework;
using RiskyRails.GameCode.Managers;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RiskyRails.GameCode.Entities;

namespace RiskyRails.GameCode.Entities.Trains
{
    /// <summary>
    /// П'яний поїзд -- ігнорує правила
    /// </summary>
    public class DrunkenTrain : Train
    {
        private Vector2 _direction;
        private static readonly Random _random = new Random();
        private TrackSegment _targetTrack;
        private float _progress;
        private readonly RailwayManager _railwayManager;
        private float _directionChangeTimer;
        private const float DirectionChangeInterval = 1.0f;
        private float _stationTimer;
        private const float StationStayTime = 2.0f;

        public DrunkenTrain(RailwayManager railwayManager)
        {
            _railwayManager = railwayManager;
            Speed = 0.5f;
            Vector2[] possibleDirections = {
                new Vector2(1, 0),
                new Vector2(-1, 0),
                new Vector2(0, 1),
                new Vector2(0, -1)
            };
            _direction = possibleDirections[_random.Next(possibleDirections.Length)];
            _progress = 0f;
            _directionChangeTimer = 0f;
            _stationTimer = 0f;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (!IsActive) return;

            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            //чи прибули на станцію
            if (CurrentTrack is Station)
            {
                _stationTimer += elapsed;

                if (_stationTimer >= StationStayTime)
                {
                    IsActive = false;
                    Debug.WriteLine("П'яний потяг прибув на станцію і зник");
                    return;
                }
            }
            else
            {
                _stationTimer = 0f;
            }

            //чи може проїхати поточний сегмент
            if (CurrentTrack != null && !CurrentTrack.CanPassThrough(this))
            {
                CurrentTrack.MarkAsDamaged();
                IsActive = false;
                Debug.WriteLine("П'яний потяг зіткнувся та був знищений");
                return;
            }

            //випадкова зміна напрямку
            _directionChangeTimer += elapsed;
            if (_directionChangeTimer > DirectionChangeInterval)
            {
                _directionChangeTimer = 0;
                if (_random.NextDouble() < 0.1)
                {
                    _direction = new Vector2(
                        (float)(_random.NextDouble() * 2 - 1),
                        (float)(_random.NextDouble() * 2 - 1)
                    );
                    _direction.Normalize();
                }
            }

            // Рух
            if (_targetTrack != null)
            {
                _progress += Speed * elapsed;
                GridPosition = Vector2.Lerp(CurrentTrack.GridPosition, _targetTrack.GridPosition, _progress);

                if (_progress >= 1.0f)
                {
                    CurrentTrack = _targetTrack;
                    GridPosition = CurrentTrack.GridPosition;
                    _targetTrack = null;
                    _progress = 0f;
                }
            }
            else
            {
                GridPosition += _direction * Speed * elapsed;

                if (CurrentTrack != null)
                {
                    Vector2 direction = _direction;
                    direction.Normalize();

                    var nextSegment = _railwayManager.CurrentLevel.Tracks
                        .FirstOrDefault(t => t.GridPosition == CurrentTrack.GridPosition + direction);

                    if (nextSegment != null && CurrentTrack.ConnectedSegments.Contains(nextSegment))
                    {
                        _targetTrack = nextSegment;
                        _progress = 0f;
                    }
                }
            }
        }

        private void UpdateCurrentTrack()
        {
            if (_railwayManager?.CurrentLevel?.Tracks == null)
                return;

            var nearestTrack = _railwayManager.CurrentLevel.Tracks
                .OrderBy(t => Vector2.Distance(GridPosition, t.GridPosition))
                .FirstOrDefault();

            if (nearestTrack != null && Vector2.Distance(GridPosition, nearestTrack.GridPosition) < 0.5f)
            {
                CurrentTrack = nearestTrack;
            }
        }

        public override void HandleSignal(Signal signal)
        {
            //нічого бо ігноруємо сигнали
        }
    }
}
