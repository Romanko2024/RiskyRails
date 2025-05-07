using Microsoft.Xna.Framework;
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
        public DrunkenTrain()
        {
            Speed = 0.7f;
            _direction = new Vector2(1, 0); //початковий напрямок
        }

        public override void Update(GameTime gameTime)
        {
            if (!CurrentTrack.CanPassThrough(this))
            {
                CurrentTrack.MarkAsDamaged();
                IsActive = false;
                return;
            }

            //випадковий вибір напрямку
            if (new Random().NextDouble() < 0.1)
            {
                _direction = new Vector2(
                    (float)(_random.NextDouble() - 0.5),
                    (float)(_random.NextDouble() - 0.5)
                );
                _direction.Normalize();
            }

            GridPosition += _direction * Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public override void HandleSignal(Signal signal)
        {
            //нічого бо ігноруємо сигнали
        }
    }
}
