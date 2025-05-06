using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RiskyRails.GameCode.Entities;

namespace RiskyRails.GameCode.Managers
{
    public class CollisionManager
    {
        public void CheckCollisions(List<Train> trains)
        {
            var collisions = trains
                .GroupBy(t => t.CurrentTrack)
                .Where(g => g.Count() > 1);

            foreach (var group in collisions)
            {
                foreach (var train in group)
                {
                    train.HandleCollision();
                }
                group.Key.SetDamaged(true);
            }
        }
    }
}
