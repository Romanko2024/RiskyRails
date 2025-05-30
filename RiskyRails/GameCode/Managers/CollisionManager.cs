using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RiskyRails.GameCode.Entities;
using RiskyRails.GameCode.Entities.Trains;
using Microsoft.Xna.Framework;
using RiskyRails.GameCode.Effects;
using Microsoft.Xna.Framework.Graphics;

namespace RiskyRails.GameCode.Managers
{
    public class CollisionManager
    {
        private readonly List<ExplosionEffect> _explosions = new();
        private readonly Texture2D _explosionTexture;

        public CollisionManager(Texture2D explosionTexture)
        {
            _explosionTexture = explosionTexture;
        }
        public void CheckCollisions(List<Train> trains)
        {
            for (int i = 0; i < trains.Count; i++)
            {
                for (int j = i + 1; j < trains.Count; j++)
                {
                    var train1 = trains[i];
                    var train2 = trains[j];

                    if (train1.IsImmune || train2.IsImmune)
                        continue;

                    var distance = Vector2.Distance(train1.GridPosition, train2.GridPosition);
                    if (distance < 0.3f)
                    {
                        if (train1.IsActive && train2.IsActive)
                        {
                            Vector2 explosionPos = (train1.GridPosition + train2.GridPosition) / 2f;
                            _explosions.Add(new ExplosionEffect(_explosionTexture, explosionPos));

                            train1.HandleCollision();
                            train2.HandleCollision();
                        }
                    }
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            foreach (var explosion in _explosions.ToList())
            {
                explosion.Update(gameTime);
                if (!explosion.IsActive)
                {
                    _explosions.Remove(explosion);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 cameraPosition)
        {
            foreach (var explosion in _explosions)
            {
                explosion.Draw(spriteBatch, cameraPosition);
            }
        }
    }
}
