using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskyRails.GameCode.Entities
{
    /// <summary>
    /// Базовий клас для всіх типів поїздів
    /// </summary>
    public abstract class Train
    {
        // властивості
        public Vector2 GridPosition { get; set; }  // позиція на грід-сітці
        public TrackSegment CurrentTrack
        {
            get => _currentTrack;
            set
            {
                _currentTrack = value;
                if (value != null)
                {
                    GridPosition = value.GridPosition;
                }
            }
        }
        private TrackSegment _currentTrack;       // поточний сегмент колії
        public float Speed { get; protected set; } = 0.5f;   // швидкість руху (тайлів/секунду)
        public bool IsActive { get; protected set; } = true; // чи активний поїзд
        public Queue<TrackSegment> Path { get; set; } = new Queue<TrackSegment>();     // шлях руху
        public Vector2 TrackGridPosition => new Vector2(
        (int)Math.Floor(GridPosition.X),
        (int)Math.Floor(GridPosition.Y)
    );
        public bool IsStoppedBySignal { get; protected set; }
        public Signal StoppedSignal { get; protected set; }
        public float WaitingTime { get; protected set; }
        // абстрактні методи
        public abstract void Update(GameTime gameTime);      // оновлення стану
        public abstract void HandleSignal(Signal signal);    // реакція на сигнал

        // віртуальні методи
        public virtual void HandleCollision()
        {
            // стандартна реакція на зіткнення
            IsActive = false;
            CurrentTrack?.MarkAsDamaged();
        }

        protected virtual void MoveToNextTrack()
        {
            if (Path.Count > 0)
            {
                CurrentTrack = Path.Dequeue();
                GridPosition = CurrentTrack.GridPosition;
            }
        }
        public virtual void Dispose()
        {
            Path.Clear();
            CurrentTrack = null;
        }
    }
}
