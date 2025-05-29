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
        private readonly RailwayManager _railwayManager;
        private TrackSegment _currentTarget;
        private bool _isGoingToStation;
        public bool IsRepairing { get; private set; }

        public RepairTrain(RailwayManager railwayManager)
        {
            _railwayManager = railwayManager;
            Speed = 0.4f;
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
            if (CurrentTrack == null)
                return;

            if (IsRepairing)
                return;

            if (IsStoppedBySignal)
            {
                if (StoppedSignal?.IsGreen == true)
                {
                    Speed = 0.4f;
                    IsStoppedBySignal = false;
                }
                return;
            }

            //ремонт поточного сегмента
            if (CurrentTrack.IsDamaged)
            {
                Repair(CurrentTrack);
                Path?.Clear();
                return;
            }

            if (Path == null)
            {
                Path = new Queue<TrackSegment>();
            }

            if (Path.Count == 0 || (_currentTarget != null && !_currentTarget.IsDamaged && _currentTarget != CurrentTrack))
            {
                FindNextTarget();
            }

            if (Path != null && Path.Count > 0)
            {
                base.MoveToNextTrack();
            }
            else
            {
                FindNextTarget();
            }
        }

        private void FindNextTarget()
        {
            if (_railwayManager?.CurrentLevel?.Tracks == null)
                return;

            var damagedTracks = _railwayManager.CurrentLevel.Tracks
                .Where(t => t != null && t.IsDamaged && t != CurrentTrack)
                .OrderBy(t => Vector2.Distance(CurrentTrack.GridPosition, t.GridPosition))
                .ToList();

            if (damagedTracks.Count > 0)
            {
                _currentTarget = damagedTracks[0];
                _isGoingToStation = false;

                if (Path == null) Path = new Queue<TrackSegment>();
                Path = _railwayManager.FindPath(CurrentTrack, _currentTarget) ?? new Queue<TrackSegment>();
            }
            else
            {
                var stations = _railwayManager.CurrentLevel.Stations?
                    .Where(s => s != null && s != CurrentTrack)
                    .OrderBy(s => Vector2.Distance(CurrentTrack.GridPosition, s.GridPosition))
                    .ToList();

                if (stations?.Count > 0)
                {
                    _currentTarget = stations[0];
                    _isGoingToStation = true;
                    if (Path == null) Path = new Queue<TrackSegment>();
                    Path = _railwayManager.FindPath(CurrentTrack, _currentTarget) ?? new Queue<TrackSegment>();
                }
                else
                {
                    _currentTarget = null;
                    Path?.Clear();
                }
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
