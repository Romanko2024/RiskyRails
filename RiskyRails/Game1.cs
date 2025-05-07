using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RiskyRails.GameCode.Entities;
using RiskyRails.GameCode.Entities.Trains;
using RiskyRails.GameCode.Managers;
using RiskyRails.GameCode.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RiskyRails
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private RailwayManager _railwayManager;
        private CollisionManager _collisionManager;
        private List<Train> _activeTrains = new();
        private IsometricCamera _camera;
        private Texture2D _tileTexture;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            _camera = new IsometricCamera(GraphicsDevice.Viewport);
            _railwayManager = new RailwayManager();
            _railwayManager.GenerateTestMap();

            _collisionManager = new CollisionManager();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _tileTexture = Content.Load<Texture2D>("tile");
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            //оновлення потягів
            foreach (var train in _activeTrains.ToList())
            {
                train.Update(gameTime);
                if (!train.IsActive) _activeTrains.Remove(train);
            }

            //перевірка зіткнень
            _collisionManager.CheckCollisions(_activeTrains);

            //спавн нових потягів
            if (new Random().Next(100) < 5)
            {
                var station = _railwayManager.Stations[0];
                var destination = _railwayManager.Stations[1];

                //створення шляху між станціями
                var path = new Queue<TrackSegment>();
                path.Enqueue(station.ConnectedSegments.First());

                _activeTrains.Add(new RegularTrain
                {
                    CurrentTrack = station,
                    Destination = destination,
                    Path = path //+ініціалізація шляху
                });
            }

            //рух камери стрілками
            var keyboardState = Keyboard.GetState();
            var moveSpeed = 5.0f;
            var newPosition = _camera.Position;

            if (keyboardState.IsKeyDown(Keys.Left))
                newPosition.X += moveSpeed;
            if (keyboardState.IsKeyDown(Keys.Right))
                newPosition.X -= moveSpeed;
            if (keyboardState.IsKeyDown(Keys.Up))
                newPosition.Y += moveSpeed;
            if (keyboardState.IsKeyDown(Keys.Down))
                newPosition.Y -= moveSpeed;

            _camera.Position = newPosition;
            _camera.Update();

            var mouseState = Mouse.GetState();
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                var worldPos = _camera.ScreenToWorld(new Vector2(mouseState.X, mouseState.Y));
                var gridPos = IsometricConverter.IsoToGrid(worldPos);
                gridPos = new Vector2((int)gridPos.X, (int)gridPos.Y);

                // Логіка спавну ремонтного поїзда
                var track = _railwayManager.Tracks.FirstOrDefault(t => t.GridPosition == gridPos);
                if (track is Station station)
                {
                    _activeTrains.Add(new RepairTrain { CurrentTrack = station });
                }
            }

            base.Update(gameTime);

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin(
                transformMatrix: _camera.TransformMatrix,
                sortMode: SpriteSortMode.Texture,
                blendState: BlendState.AlphaBlend,
                samplerState: SamplerState.PointClamp
                );

            // малювання колій
            foreach (var track in _railwayManager.Tracks)
            {
                var isoPos = IsometricConverter.GridToIso(track.GridPosition);
                var origin = new Vector2(_tileTexture.Width / 2, _tileTexture.Height / 2);

                _spriteBatch.Draw(
                    _tileTexture,
                    isoPos,
                    null,
                    track.IsDamaged ? Color.Red : Color.White,
                    0f,
                    origin,
                    1f,
                    SpriteEffects.None,
                    isoPos.Y / 1000f
                );
            }

            //малювання поїздів
            foreach (var train in _activeTrains)
            {
                var isoPos = IsometricConverter.GridToIso(train.GridPosition);
                var trainColor = train switch
                {
                    RegularTrain => Color.Blue,
                    DrunkenTrain => Color.Red,
                    RepairTrain => Color.Green,
                    _ => Color.White
                };

                _spriteBatch.Draw(
                    _tileTexture,
                    isoPos,
                    null,
                    trainColor,
                    0f,
                    new Vector2(_tileTexture.Width / 2, _tileTexture.Height / 2),
                    0.7f, //масштаб поїзда
                    SpriteEffects.None,
                    isoPos.Y / 1000f + 0.1f //поїзди поверх колій
                );
            }
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}