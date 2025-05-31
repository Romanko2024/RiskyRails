using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public bool IsImmune { get; set; } = true;
        private float _immuneTimer;
        private const float ImmuneDuration = 0.5f;

        public Vector2 LastPosition { get; set; }
        public Vector2 DirectionVector { get; protected set; } = Vector2.UnitX;
        public int AnimationFrame { get; protected set; }
        private float _animationTimer;
        private const float AnimationSpeed = 0.2f;

        // абстрактні методи
        public virtual void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (IsImmune)
            {
                _immuneTimer += elapsed;
                if (_immuneTimer >= ImmuneDuration)
                {
                    IsImmune = false;
                    Debug.WriteLine($"Імунітет потяга вимкнено: {GetType().Name}");
                }
            }
            _animationTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_animationTimer >= AnimationSpeed)
            {
                AnimationFrame = (AnimationFrame + 1) % 2; //чергуєсо 0 і 1
                _animationTimer = 0;
            }

            // напрямок руху
            if (GridPosition != LastPosition)
            {
                DirectionVector = Vector2.Normalize(GridPosition - LastPosition);
            }
            LastPosition = GridPosition;
        }

        public string DirectionName
        {
            get
            {
                if (Math.Abs(DirectionVector.X) > Math.Abs(DirectionVector.Y))
                {
                    return DirectionVector.X > 0 ? "E" : "W";
                }
                return DirectionVector.Y > 0 ? "S" : "N";
            }
        }

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
