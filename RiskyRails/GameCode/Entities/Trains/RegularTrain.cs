using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;
using RiskyRails.GameCode.Entities;
namespace RiskyRails.GameCode.Entities.Trains
{
    /// <summary>
    /// поїзд, що дотримується правил
    /// </summary>
    public class RegularTrain : Train
    {
        public Station Destination { get; set; }
        private float _progress; // прогрес руху між сегментами (0-1)

        public RegularTrain()
        {
            Speed = 0.3f;
        }

        public override void Update(GameTime gameTime)
        {
            if (!CurrentTrack.CanPassThrough(this)) return;

            _progress += Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_progress >= 1.0f)
            {
                MoveToNextTrack();
                _progress = 0;

                if (CurrentTrack == Destination)
                {
                    IsActive = false;
                    return;
                }
            }

            //перевірка наявності елементів у черзі
            if (Path.Count > 0)
            {
                // інтерполяція позиції
                GridPosition = Vector2.Lerp(
                    CurrentTrack.GridPosition,
                    Path.Peek().GridPosition,
                    _progress
                );
            }
            else
            {
                //якщо шлях порожній - зупинити потяг
                IsActive = false;
            }
        }

        public override void HandleSignal(Signal signal)
        {
            if (signal.IsGreen) return;

            //зупинка на червоний сигнал
            Speed = 0;
            Path.Clear();
            IsActive = false;
        }
    }
}
