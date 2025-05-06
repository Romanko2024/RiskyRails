using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RiskyRails.GameCode.Entities;
using RiskyRails.GameCode.Entities.Trains;
using RiskyRails.GameCode.Managers;
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

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            _railwayManager = new RailwayManager();
            _railwayManager.GenerateTestMap();

            _collisionManager = new CollisionManager();

            base.Initialize();
        }

        protected override void LoadContent()
        {
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
                _activeTrains.Add(new RegularTrain
                {
                    CurrentTrack = station,
                    Destination = _railwayManager.Stations[1]
                });
            }

            base.Update(gameTime);

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            //малювання колій
            _spriteBatch.Begin();
            foreach (var track in _railwayManager.Tracks)
            {
                //ТИМЧАСОВИЙ ПРЯМОКУТНИК
                var rect = new Rectangle(
                    (int)track.GridPosition.X * 50,
                    (int)track.GridPosition.Y * 50,
                    50, 50);

                _spriteBatch.Draw(
                    Texture2D.FromFile(GraphicsDevice, "whitePixel.png"),
                    rect,
                    track.IsDamaged ? Color.Red : Color.Gray);
            }
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}