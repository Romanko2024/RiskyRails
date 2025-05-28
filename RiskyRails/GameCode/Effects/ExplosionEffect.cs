using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiskyRails.GameCode.Utilities;

namespace RiskyRails.GameCode.Effects
{
    public class ExplosionEffect
    {
        private readonly Texture2D _texture;
        private readonly Vector2 _position;
        private readonly float _duration;
        private float _elapsedTime;
        private bool _isActive = true;

        public bool IsActive => _isActive;

        public ExplosionEffect(Texture2D texture, Vector2 position, float duration = 0.8f)
        {
            _texture = texture;
            _position = position;
            _duration = duration;
        }

        public void Update(GameTime gameTime)
        {
            if (!_isActive) return;

            _elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_elapsedTime >= _duration)
            {
                _isActive = false;
            }
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 cameraPosition)
        {
            if (!_isActive) return;

            //прозорість і пульсація
            float progress = _elapsedTime / _duration;
            float alpha = 1f - progress;
            float pulse = (float)Math.Sin(progress * MathHelper.Pi * 8) * 0.5f + 1f;
            Vector2 isoPos = IsometricConverter.GridToIso(_position);
            spriteBatch.Draw(
                _texture,
                isoPos,
                null,
                Color.White * alpha,
                0f,
                new Vector2(_texture.Width / 2, _texture.Height / 2),
                pulse,
                SpriteEffects.None,
                0.95f
            );
        }
    }
}
