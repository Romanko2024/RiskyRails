using Microsoft.Xna.Framework;
using RiskyRails.GameCode.Managers;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private const float DirectionChangeInterval = 2.0f;
        private float _stationTimer;
        private const float StationStayTime = 5.0f;
        private float _spawnTimer = 0f;
        private const float SpawnImmunityTime = 0.5f;

        public DrunkenTrain(RailwayManager railwayManager)
        {
            _railwayManager = railwayManager;
            Speed = 0.5f;

            //початковий напрямок буде встановлений під час першого Update
            _direction = Vector2.Zero;
            _progress = 0f;
            _directionChangeTimer = 0f;
            _stationTimer = 0f;
            IsImmune = true;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (!IsActive) return;

            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            _spawnTimer += elapsed;
            if (_spawnTimer >= SpawnImmunityTime)
            {
                IsImmune = false;
            }

            // Перевірка чи потяг зійшов з колій
            if (CurrentTrack == null && !IsImmune)
            {
                IsActive = false;
                Debug.WriteLine("П'яний потяг зійшов з колій");
                return;
            }

            if (!IsImmune && CurrentTrack is Station)
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

            if (CurrentTrack != null && !CurrentTrack.CanPassThrough(this) && !IsImmune)
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
                if (_random.NextDouble() < 0.05)
                {
                    Vector2 newDirection = new Vector2(
                        (float)(_random.NextDouble() * 2 - 1),
                        (float)(_random.NextDouble() * 2 - 1)
                    );
                    newDirection.Normalize();

                    float angle = MathHelper.ToRadians(45);
                    float dot = Vector2.Dot(_direction, newDirection);
                    if (dot > Math.Cos(angle))
                    {
                        _direction = newDirection;
                    }
                }
            }

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
                //немає цільового сегмента - знаходимо новий
                var possibleDirections = GetPossibleDirections();

                if (possibleDirections.Count > 0)
                {
                    //рандом вибір напрямку з доступних
                    _direction = possibleDirections[_random.Next(possibleDirections.Count)];
                    _direction.Normalize();

                    var nextSegment = _railwayManager.CurrentLevel.Tracks
                        .FirstOrDefault(t => t.GridPosition == CurrentTrack.GridPosition + _direction);

                    if (nextSegment != null && CurrentTrack.ConnectedSegments.Contains(nextSegment))
                    {
                        _targetTrack = nextSegment;
                        _progress = 0f;
                    }
                }
                else
                {
                    _direction = new Vector2(
                        (float)(_random.NextDouble() * 2 - 1),
                        (float)(_random.NextDouble() * 2 - 1)
                    );
                    _direction.Normalize();
                }
            }
        }

        private List<Vector2> GetPossibleDirections()
        {
            var directions = new List<Vector2>();

            if (CurrentTrack != null)
            {
                foreach (var dir in CurrentTrack.GetConnectionPoints())
                {
                    var neighborPos = CurrentTrack.GridPosition + dir;
                    var neighbor = _railwayManager.CurrentLevel.Tracks
                        .FirstOrDefault(t => t.GridPosition == neighborPos);

                    if (neighbor != null && CurrentTrack.ConnectedSegments.Contains(neighbor))
                    {
                        directions.Add(dir);
                    }
                }
            }

            return directions;
        }

        public override void HandleSignal(Signal signal)
        {
            //нічого бо ігноруємо сигнали
        }
    }
}
