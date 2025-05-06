using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RiskyRails.GameCode.Utilities
{
    public class IsometricCamera
    {
        private readonly Viewport _viewport;
        private Vector2 _position;
        public Vector2 Position
        {
            get => _position;
            set => _position = value;
        }
        public float Zoom { get; set; } = 1.0f;
        public Matrix TransformMatrix { get; private set; }

        public IsometricCamera(Viewport viewport)
        {
            _viewport = viewport;
            Position = new Vector2(viewport.Width / 2, viewport.Height / 2);
        }

        public void Update()
        {
            TransformMatrix = Matrix.CreateTranslation(-Position.X, -Position.Y, 0) *
                              Matrix.CreateScale(Zoom) *
                              Matrix.CreateTranslation(_viewport.Width / 2, _viewport.Height / 2, 0);
        }

        public Vector2 ScreenToWorld(Vector2 screenPos)
        {
            return Vector2.Transform(screenPos, Matrix.Invert(TransformMatrix));
        }
    }
}
