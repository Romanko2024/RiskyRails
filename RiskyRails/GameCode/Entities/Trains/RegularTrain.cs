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
            if (!CurrentTrack.CanPassThrough(this) || Path.Count == 0)
            {
                IsActive = false;
                return;
            }

            var targetTrack = Path.Peek();

            //перевірка коректності з'єднання
            if (!CurrentTrack.ConnectedSegments.Contains(targetTrack))
            {
                Path.Clear();
                IsActive = false;
                return;
            }

            _progress += Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            GridPosition = Vector2.Lerp(CurrentTrack.GridPosition, targetTrack.GridPosition, _progress);

            if (_progress >= 1.0f)
            {
                CurrentTrack = Path.Dequeue();
                _progress = 0;
                if (CurrentTrack == Destination) IsActive = false;
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
