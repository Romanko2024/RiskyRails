using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RiskyRails.GameCode.Entities;
using RiskyRails.GameCode.Entities.Trains;
using RiskyRails.GameCode.Managers;
using RiskyRails.GameCode.Utilities;
using SharpDX.Direct2D1.Effects;
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

        //текстури
        private Texture2D _tileTexture;
        private Texture2D _tileSwitch;
        private Texture2D _tileSignal;
        private Texture2D _tileRailX;
        private Texture2D _tileRailY;
        private Texture2D _tileCurveNE;
        private Texture2D _tileCurveSE;
        private Texture2D _tileCurveSW;
        private Texture2D _tileCurveNW;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _camera = new IsometricCamera(GraphicsDevice.Viewport);
            _railwayManager = new RailwayManager();
            _railwayManager.Initialize();

            _collisionManager = new CollisionManager();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            //завантаження текстур
            _tileRailX = Content.Load<Texture2D>("rail_straight_x");
            _tileRailY = Content.Load<Texture2D>("rail_straight_y");
            _tileCurveNE = Content.Load<Texture2D>("rail_curve_ne");
            _tileCurveSE = Content.Load<Texture2D>("rail_curve_se");
            _tileCurveSW = Content.Load<Texture2D>("rail_curve_sw");
            _tileCurveNW = Content.Load<Texture2D>("rail_curve_nw");
            _tileSignal = Content.Load<Texture2D>("tile_signal");
            _tileSwitch = Content.Load<Texture2D>("tile_switch");
            _tileSignal = Content.Load<Texture2D>("tile_signal");
            _tileTexture = Content.Load<Texture2D>("Tile");

            _spriteBatch = new SpriteBatch(GraphicsDevice);
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
            if (new Random().Next(10000) < 100 && _railwayManager.CurrentLevel.Stations.Count >= 2)
            {
                var stations = _railwayManager.CurrentLevel.Stations;
                var startStation = stations[new Random().Next(stations.Count)];
                var endStation = stations.FirstOrDefault(s => s != startStation);

                //створення шляху між станціями
                var path = _railwayManager.FindPath(startStation, endStation);
                if (path != null && path.Count > 0)
                {
                    _activeTrains.Add(new RegularTrain
                    {
                        CurrentTrack = startStation,
                        Destination = endStation,
                        Path = new Queue<TrackSegment>(path)
                    });
                }
            }

            //рух камери стрілками
            var keyboardState = Keyboard.GetState();
            var moveSpeed = 5.0f;
            var newPosition = _camera.Position;

            if (keyboardState.IsKeyDown(Keys.Left)) newPosition.X -= moveSpeed;
            if (keyboardState.IsKeyDown(Keys.Right)) newPosition.X += moveSpeed;
            if (keyboardState.IsKeyDown(Keys.Up)) newPosition.Y -= moveSpeed;
            if (keyboardState.IsKeyDown(Keys.Down)) newPosition.Y += moveSpeed;

            //перемикання рівнів
            if (keyboardState.IsKeyDown(Keys.D1)) _railwayManager.LoadLevel(0);
            if (keyboardState.IsKeyDown(Keys.D2)) _railwayManager.LoadLevel(1);

            _camera.Position = newPosition;
            _camera.Update();

            //періодична зміна сигналів
            if (new Random().Next(100) < 5)
            {
                foreach (var signal in _railwayManager.CurrentLevel.Signals)
                {
                    signal.IsGreen = !signal.IsGreen;
                }
            }

            // спавн ремонтних потягів
            var mouseState = Mouse.GetState();
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                var worldPos = _camera.ScreenToWorld(new Vector2(mouseState.X, mouseState.Y));
                var gridPos = IsometricConverter.IsoToGrid(worldPos);
                gridPos = new Vector2((int)gridPos.X, (int)gridPos.Y);

                // Логіка спавну ремонтного поїзда
                var track = _railwayManager.CurrentLevel.Tracks.FirstOrDefault(t => t.GridPosition == gridPos);
                //явна перевірка типу через is
                if (track is Station station)
                {
                    _activeTrains.Add(new RepairTrain { CurrentTrack = station });
                }
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin(
                transformMatrix: _camera.TransformMatrix,
                sortMode: SpriteSortMode.FrontToBack,
                blendState: BlendState.AlphaBlend,
                samplerState: SamplerState.PointClamp
            );

            // малювання колій
            foreach (var track in _railwayManager.CurrentLevel.Tracks)
            {
                Texture2D texture;
                float rotation = 0f;
                Vector2 origin = Vector2.Zero;

                switch (track.Type)
                {
                    case TrackSegment.TrackType.StraightX:
                        texture = _tileRailX;
                        origin = new Vector2(_tileRailX.Width / 2, _tileRailY.Height / 2);
                        break;
                    case TrackSegment.TrackType.StraightY:
                        texture = _tileRailY;
                        rotation = MathHelper.PiOver2;
                        origin = new Vector2(_tileRailY.Width / 2, _tileRailY.Height / 2);
                        break;
                    case TrackSegment.TrackType.CurveNE:
                        texture = _tileCurveNE;
                        origin = new Vector2(_tileCurveNE.Width / 2, _tileCurveNE.Height / 2);
                        break;
                    case TrackSegment.TrackType.CurveSE:
                        texture = _tileCurveSE;
                        rotation = MathHelper.PiOver2;
                        origin = new Vector2(_tileCurveSE.Width / 2, _tileCurveSE.Height / 2);
                        break;
                    case TrackSegment.TrackType.CurveSW:
                        texture = _tileCurveSW;
                        rotation = MathHelper.Pi;
                        origin = new Vector2(_tileCurveSW.Width / 2, _tileCurveSW.Height / 2);
                        break;
                    case TrackSegment.TrackType.CurveNW:
                        texture = _tileCurveNW;
                        rotation = MathHelper.PiOver2 * 3;
                        origin = new Vector2(_tileCurveNW.Width / 2, _tileCurveNW.Height / 2);
                        break;
                    default:
                        texture = _tileRailX;
                        break;
                }

                var isoPos = IsometricConverter.GridToIso(track.GridPosition);
                float depth = IsometricConverter.CalculateDepth(track.GridPosition);

                _spriteBatch.Draw(
                    texture,
                    isoPos,
                    null,
                    Color.White,
                    rotation,
                    origin,
                    1f,
                    SpriteEffects.None,
                    depth
                );
            }

            //малювання поїздів
            foreach (var train in _activeTrains)
            {
                var isoPos = IsometricConverter.GridToIso(train.GridPosition);
                var origin = new Vector2(_tileTexture.Width / 2, _tileTexture.Height / 2);
                float trainDepth = IsometricConverter.CalculateDepth(train.GridPosition) + 0.0001f;

                Color trainColor = train switch
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
                    origin,
                    0.7f, //масштаб поїзда
                    SpriteEffects.None,
                    trainDepth
                );
            }
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}