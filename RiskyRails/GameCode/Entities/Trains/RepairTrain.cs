using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RiskyRails.GameCode.Interfaces;
using RiskyRails.GameCode.Entities;
using System.Diagnostics;
using RiskyRails.GameCode.Managers;
namespace RiskyRails.GameCode.Entities.Trains
{
    /// <summary>
    /// Ремонтний поїзд зі своєю логікою
    /// </summary>
    public class RepairTrain : Train, IRepairable
    {
        private float _progress;
        private TrackSegment _targetTrack;
        private readonly RailwayManager _railwayManager;
        private TrackSegment _currentTarget;
        private bool _isGoingToStation;
        private float _pathUpdateTimer;
        private const float PathUpdateInterval = 1.0f;
        public bool IsRepairing { get; private set; }
        private readonly Station _homeStation;
        private float _idleTime;
        public RepairTrain(RailwayManager railwayManager, Station homeStation)
        {
            _railwayManager = railwayManager;
            Speed = 0.4f;
            _progress = 0f;
            _homeStation = homeStation;
            _idleTime = 0;
        }

        public void Repair(TrackSegment track)
        {
            if (!track.IsDamaged) return;

            IsRepairing = true;
            track.SetDamaged(false);

            //ремонт триває 1000мс
            new System.Threading.Timer(_ => IsRepairing = false,
                null, 1000, System.Threading.Timeout.Infinite);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (IsImmune || IsRepairing)
                return;
            _pathUpdateTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            bool shouldFindPath = _pathUpdateTimer >= PathUpdateInterval;
            if (shouldFindPath)
            {
                _pathUpdateTimer = 0;
            }

            if (CurrentTrack == null) return;
            if (IsRepairing) return;

            // Обробка зупинки через сигнал
            if (IsStoppedBySignal)
            {
                if (StoppedSignal?.IsGreen == true)
                {
                    Speed = 0.4f;
                    IsStoppedBySignal = false;
                }
                return;
            }

            // Ремонт поточного сегмента
            if (CurrentTrack.IsDamaged)
            {
                Repair(CurrentTrack);
                Path?.Clear();
                return;
            }

            // Якщо немає цільового сегмента, шукаємо нову ціль
            if (_targetTrack == null && (Path == null || Path.Count == 0))
            {
                if (shouldFindPath)
                {
                    FindNextTarget();
                }
            }

            // Якщо є шлях, рухаємося по ньому
            if (Path != null && Path.Count > 0 && _targetTrack == null)
            {
                _targetTrack = Path.Dequeue();
                _progress = 0f;
            }

            // Плавний рух до цільового сегмента
            if (_targetTrack != null)
            {
                _progress += Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (CurrentTrack != null && _targetTrack != null)
                {
                    GridPosition = Vector2.Lerp(CurrentTrack.GridPosition, _targetTrack.GridPosition, _progress);
                }

                if (_progress >= 1.0f)
                {
                    CurrentTrack = _targetTrack;
                    GridPosition = CurrentTrack.GridPosition;
                    _targetTrack = null;
                    _progress = 0f;

                    if (CurrentTrack.IsDamaged)
                    {
                        Repair(CurrentTrack);
                        Path?.Clear();
                    }
                    else if (CurrentTrack == _homeStation)
                    {
                        Path?.Clear();
                        _currentTarget = null;
                        Debug.WriteLine("Ремонтний потяг повернувся на базу");
                    }
                    else
                    {
                        FindNextTarget();
                    }
                }
            }
            else if (CurrentTrack == _homeStation && _currentTarget == null)
            {
                _idleTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_idleTime > 1.0f)
                {
                    IsActive = false;
                    Debug.WriteLine("Ремонтний потяг деактивовано після очікування");
                }
            }
        }

        private void FindNextTarget()
        {
            // Пошук пошкоджених сегментів
            var damagedTracks = _railwayManager.CurrentLevel.Tracks
                .Where(t => t != null && t.IsDamaged && t != CurrentTrack)
                .OrderBy(t => Vector2.Distance(CurrentTrack.GridPosition, t.GridPosition))
                .ToList();

            if (damagedTracks.Count > 0)
            {
                _currentTarget = damagedTracks[0];
                _isGoingToStation = false;
                Path = _railwayManager.FindPath(CurrentTrack, _currentTarget, this);
            }
            else if (_homeStation != null && _homeStation != CurrentTrack)
            {
                // Повертаємось на базову станцію
                _currentTarget = _homeStation;
                _isGoingToStation = true;
                Path = _railwayManager.FindPath(CurrentTrack, _homeStation, this);
            }
            else
            {
                _currentTarget = null;
                Path?.Clear();
            }
        }

        public override void HandleSignal(Signal signal)
        {
            if (!signal.IsGreen)
            {
                Speed = 0; //зупинка на червоний
                IsStoppedBySignal = true;
                StoppedSignal = signal;
            }
            else if (IsStoppedBySignal && signal == StoppedSignal)
            {
                IsStoppedBySignal = false;
            }
        }
    }
}
