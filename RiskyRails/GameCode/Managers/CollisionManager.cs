using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RiskyRails.GameCode.Entities;
using RiskyRails.GameCode.Entities.Trains;
using Microsoft.Xna.Framework;

namespace RiskyRails.GameCode.Managers
{
    public class CollisionManager
    {
        public void CheckCollisions(List<Train> trains)
        {
            foreach (var train1 in trains)
            {
                foreach (var train2 in trains)
                {
                    if (train1 == train2 || train1 is RepairTrain || train2 is RepairTrain)
                        continue;

                    var distance = Vector2.Distance(train1.GridPosition, train2.GridPosition);
                    if (distance < 0.3f) //якщо поїзди дуже близько
                    {
                        train1.HandleCollision();
                        train2.HandleCollision();
                        train1.CurrentTrack?.MarkAsDamaged();
                    }
                }
            }
        }
    }
}
