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
        public Vector2 GridPosition { get; protected set; }  // позиція на грід-сітці
        public TrackSegment CurrentTrack { get; set; }       // поточний сегмент колії
        public float Speed { get; protected set; } = 0.5f;   // швидкість руху (тайлів/секунду)
        public bool IsActive { get; protected set; } = true; // чи активний поїзд
        public Queue<TrackSegment> Path { get; } = new();    // шлях руху

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
    }
}
