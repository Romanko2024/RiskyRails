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
using System.Diagnostics;
using System.Linq;
using static RiskyRails.GameCode.Entities.TrackSegment;

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
        private Texture2D _tileSignalX;
        private Texture2D _tileSignalY;
        private Texture2D _tileRailX;
        private Texture2D _tileRailY;
        private Texture2D _tileCurveNE;
        private Texture2D _tileCurveSE;
        private Texture2D _tileCurveSW;
        private Texture2D _tileCurveNW;
        private Texture2D _tileSwitchPrimary;
        private Texture2D _tileSwitchSecondary;

        private MouseState _previousMouseState;
        private double _lastToggleTime;
        private const double ClickDelayMs = 300;
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            try
            {
                _camera = new IsometricCamera(GraphicsDevice.Viewport);
                _railwayManager = new RailwayManager();
                _railwayManager.Initialize();

                _collisionManager = new CollisionManager();
                if (_railwayManager.CurrentLevel == null)
                    throw new InvalidOperationException("Рівень не завантажено!");
                base.Initialize();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Помилка ініціалізації: {ex}");
                throw;
            }
        }

        protected override void LoadContent()
        {
            //завантаження текстур
            _tileRailY = Content.Load<Texture2D>("rail_straight_x");
            _tileRailX = Content.Load<Texture2D>("rail_straight_y");
            _tileCurveSE = Content.Load<Texture2D>("rail_curve_se");
            _tileCurveNE = Content.Load<Texture2D>("rail_curve_ne");
            _tileCurveNW = Content.Load<Texture2D>("rail_curve_nw");
            _tileCurveSW = Content.Load<Texture2D>("rail_curve_sw");
            _tileSignal = Content.Load<Texture2D>("tile_signal");
            _tileSwitchPrimary = Content.Load<Texture2D>("switch_primary");
            _tileSwitchSecondary = Content.Load<Texture2D>("switch_secondary");
            _tileTexture = Content.Load<Texture2D>("Tile");
            _tileSignalX = Content.Load<Texture2D>("StraightX_Signal");
            _tileSignalY = Content.Load<Texture2D>("StraightY_Signal");

            _spriteBatch = new SpriteBatch(GraphicsDevice);
        }
        private Dictionary<Station, double> _lastSpawnTimes = new();
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
            double currentTime = gameTime.TotalGameTime.TotalSeconds;
            foreach (var station in _railwayManager.CurrentLevel.Stations)
            {
                if (!_lastSpawnTimes.TryGetValue(station, out var lastTime))
                {
                    _lastSpawnTimes[station] = currentTime;
                    continue;
                }

                if (currentTime - lastTime >= 12)
                {
                    SpawnTrainFromStation(station);
                    _lastSpawnTimes[station] = currentTime;
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
            //if (new Random().Next(100) < 5)
            //{
            //    foreach (var signal in _railwayManager.CurrentLevel.Signals)
            //    {
            //        signal.IsGreen = !signal.IsGreen;
            //    }
            //}

            //
            var mouseState = Mouse.GetState();
            var worldPos = _camera.ScreenToWorld(new Vector2(mouseState.X, mouseState.Y));
            var gridPos = IsometricConverter.IsoToGrid(worldPos);
            gridPos = new Vector2(
                (int)MathF.Round(gridPos.X),
                (int)MathF.Round(gridPos.Y)
            );
            if (mouseState.LeftButton == ButtonState.Pressed &&
        _previousMouseState.LeftButton == ButtonState.Released)
            {
                var track = _railwayManager.CurrentLevel.Tracks
                    .FirstOrDefault(t => t.GridPosition == gridPos);

                if (track is Station station)
                {
                    _activeTrains.Add(new RepairTrain { CurrentTrack = station });
                }
            }
            if (Keyboard.GetState().IsKeyDown(Keys.P))
            {
                var stationB = _railwayManager.CurrentLevel.Stations.First(s => s.Name == "B");
                var stationC = _railwayManager.CurrentLevel.Stations.First(s => s.Name == "C");

                var path = _railwayManager.FindPath(stationB, stationC);
                Debug.WriteLine(path != null
                    ? $"Шлях B->C знайдено! Сегментів: {path.Count}"
                    : "Шлях B->C не знайдено!");
            }
            // правий клік - перемикання стрілок/сигналів
            if (mouseState.RightButton == ButtonState.Pressed &&
                _previousMouseState.RightButton == ButtonState.Released)
            {
                if (gameTime.TotalGameTime.TotalMilliseconds - _lastToggleTime > ClickDelayMs)
                {
                    var switchTrack = _railwayManager.CurrentLevel.Tracks
                        .OfType<SwitchTrack>()
                        .FirstOrDefault(t => t.GridPosition == gridPos);

                    if (switchTrack != null)
                    {
                        switchTrack.Toggle();
                        _railwayManager.CurrentLevel.ConnectSwitchNeighbors(switchTrack);

                        Debug.WriteLine($"Переключено стрілку на {gridPos}");
                    }
                    else
                    {
                        var signalTrack = _railwayManager.CurrentLevel.Tracks
                            .FirstOrDefault(t => t.GridPosition == gridPos && t.Signal != null);

                        if (signalTrack != null)
                        {
                            signalTrack.Signal.IsGreen = !signalTrack.Signal.IsGreen;
                            Debug.WriteLine($"Переключено сигнал на {gridPos}");
                        }
                    }

                    _lastToggleTime = gameTime.TotalGameTime.TotalMilliseconds;
                }
            }
            _previousMouseState = mouseState;

            base.Update(gameTime);
        }
        private void SpawnTrainFromStation(Station station)
        {
            var otherStations = _railwayManager.CurrentLevel.Stations
                .Where(s => s != station && s != null)
                .ToList();

            if (otherStations.Count == 0)
            {
                Debug.WriteLine($"Немає інших станцій для спавну з {station.Name}");
                return;
            }

            var destination = otherStations[new Random().Next(otherStations.Count)];
            Debug.WriteLine($"Спавн потяга з {station.Name} до {destination.Name}");

            var path = _railwayManager.FindPath(station, destination);

            if (path == null || path.Count == 0)
            {
                Debug.WriteLine($"Шлях не знайдено! Потяг не створений.");
                return;
            }

            var train = new RegularTrain(_railwayManager)
            {
                CurrentTrack = station,
                Destination = destination,
                Path = path
            };

            _activeTrains.Add(train);
            Debug.WriteLine($"Потяг створений і доданий до активних. Шлях має {path.Count} сегментів.");
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
                Vector2 origin = Vector2.Zero;
                Color tint = Color.White;

                if (track is SwitchTrack switchTrack)
                {
                    //текстури колій + сірий відтінок
                    texture = GetTextureForTrackType(switchTrack.Type);
                    tint = Color.LightGray;
                    origin = new Vector2(texture.Width / 2, texture.Height / 2);
                }
                else
                {
                    //колії та сигнали
                    texture = GetTextureForTrackType(track.Type);
                    origin = new Vector2(texture.Width / 2, texture.Height / 2);

                    //обробка сигналів
                    switch (track.Type)
                    {
                        case TrackType.StraightX_Signal:
                        case TrackType.StraightY_Signal:
                            tint = track.Signal?.IsGreen == true ? Color.Green : Color.Red;
                            break;
                    }
                }

                var isoPos = IsometricConverter.GridToIso(track.GridPosition);
                float depth = IsometricConverter.CalculateDepth(track.GridPosition);

                _spriteBatch.Draw(
                    texture,
                    isoPos,
                    null,
                    tint,
                    0f,
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

                float trainDepth = IsometricConverter.CalculateDepth(
                    train.TrackGridPosition,
                    offset: 0.01f
                );

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
                    0.7f,
                    SpriteEffects.None,
                    trainDepth
                );
            }
            _spriteBatch.End();

            base.Draw(gameTime);
        }

        //метод для отримання текстури за типом колії
        private Texture2D GetTextureForTrackType(TrackType type)
        {
            switch (type)
            {
                case TrackType.StraightX: return _tileRailX;
                case TrackType.StraightY: return _tileRailY;
                case TrackType.CurveNE: return _tileCurveNE;
                case TrackType.CurveSE: return _tileCurveSE;
                case TrackType.CurveSW: return _tileCurveSW;
                case TrackType.CurveNW: return _tileCurveNW;
                case TrackType.StraightX_Signal: return _tileSignalX;
                case TrackType.StraightY_Signal: return _tileSignalY;
                default: return _tileRailX;
            }
        }
    }
}