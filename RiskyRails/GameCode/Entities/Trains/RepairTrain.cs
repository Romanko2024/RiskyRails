using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RiskyRails.GameCode.Interfaces;
using RiskyRails.GameCode.Entities;
using System.Diagnostics;
namespace RiskyRails.GameCode.Entities.Trains
{
    /// <summary>
    /// Ремонтний поїзд зі своєю логікою
    /// </summary>
    public class RepairTrain : Train, IRepairable
    {
        public bool IsRepairing { get; private set; }

        public RepairTrain()
        {
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
            if (IsRepairing) return;

            if (IsStoppedBySignal)
            {
                if (StoppedSignal.IsGreen)
                {
                    Speed = 0.4f;
                    IsStoppedBySignal = false;
                }
                return;
            }

            if (Path.Count > 0)
            {
                base.MoveToNextTrack();
            }

            if (CurrentTrack.IsDamaged)
            {
                Repair(CurrentTrack);
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
