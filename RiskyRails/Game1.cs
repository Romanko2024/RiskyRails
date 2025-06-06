﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RiskyRails.GameCode.Effects;
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
        private Texture2D _stationTexture;
        private Texture2D _damagedTexture;
        private Texture2D _explosionTexture;
        private List<ExplosionEffect> _explosions = new();
        private MouseState _previousMouseState;
        private double _lastToggleTime;
        private const double ClickDelayMs = 300;

        private int _currentLevelIndex = 0;

        private Dictionary<string, Texture2D> _trainTextures = new();
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }
        private double _lastDrunkenSpawnTime;
        private const double DrunkenSpawnInterval = 15.0;
        private const float DrunkenSpawnChance = 1f;
        protected override void Initialize()
        {
            try
            {
                _camera = new IsometricCamera(GraphicsDevice.Viewport);
                _railwayManager = new RailwayManager();
                _railwayManager.Initialize();
                _lastDrunkenSpawnTime = 0;
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
            _tileTexture = Content.Load<Texture2D>("Tile");
            _tileSignalX = Content.Load<Texture2D>("StraightX_Signal");
            _tileSignalY = Content.Load<Texture2D>("StraightY_Signal");
            _stationTexture = Content.Load<Texture2D>("station");
            _damagedTexture = Content.Load<Texture2D>("damaged_track");
            _explosionTexture = Content.Load<Texture2D>("explosion");
            _collisionManager = new CollisionManager(_explosionTexture);
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            string[] directions = { "N", "S", "E", "W" };
            foreach (var dir in directions)
            {
                _trainTextures.Add($"train_{dir}1", Content.Load<Texture2D>($"train_{dir}"));
                _trainTextures.Add($"train_{dir}2", Content.Load<Texture2D>($"train_{dir}2"));
            }
        }
        private Dictionary<Station, double> _lastSpawnTimes = new();
        protected override void Update(GameTime gameTime)
        {
            var keyboardState = Keyboard.GetState();

            //перемикання рівнів
            if (keyboardState.IsKeyDown(Keys.D1) && _currentLevelIndex != 0)
            {
                _railwayManager.LoadLevel(0);
                _currentLevelIndex = 0;
                ResetGameState();
            }
            if (keyboardState.IsKeyDown(Keys.D2) && _currentLevelIndex != 1)
            {
                _railwayManager.LoadLevel(1);
                _currentLevelIndex = 1;
                ResetGameState();
            }

            //оновлення потягів
            foreach (var train in _activeTrains.ToList())
            {
                train.Update(gameTime);
                if (!train.IsActive) _activeTrains.Remove(train);
            }

            //перевірка зіткнень
            _collisionManager.CheckCollisions(_activeTrains);
            _collisionManager.Update(gameTime);
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
            //спавн п'яного потягу
            if (currentTime - _lastDrunkenSpawnTime > DrunkenSpawnInterval)
            {
                _lastDrunkenSpawnTime = currentTime;

                if (new Random().NextDouble() < DrunkenSpawnChance)
                {
                    SpawnDrunkenTrain();
                }
            }

            //рух камери стрілками
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

            //
            var mouseState = Mouse.GetState();
            var worldPos = _camera.ScreenToWorld(new Vector2(mouseState.X, mouseState.Y));
            var gridPos = IsometricConverter.IsoToGrid(worldPos);
            gridPos = new Vector2(
                (int)MathF.Round(gridPos.X),
                (int)MathF.Round(gridPos.Y)
            );
            //виклик рем потягу
            if (mouseState.LeftButton == ButtonState.Pressed &&
        _previousMouseState.LeftButton == ButtonState.Released)
            {
                var track = _railwayManager.CurrentLevel.Tracks
                    .FirstOrDefault(t => t.GridPosition == gridPos);

                if (track is Station station)
                {
                    var repairTrain = new RepairTrain(_railwayManager, station)
                    {
                        CurrentTrack = station,
                        GridPosition = station.GridPosition,
                        IsImmune = true
                    };
                    _activeTrains.Add(repairTrain);
                }
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
                .Where(s => s != station)
                .ToList();

            if (otherStations.Count == 0) return;

            var destination = otherStations[new Random().Next(otherStations.Count)];

            // Перевірка наявності активних потягів на маршруті
            bool routeOccupied = _activeTrains.OfType<RegularTrain>().Any(t =>
                (t.DepartureStation == station && t.Destination == destination) ||
                (t.DepartureStation == destination && t.Destination == station)
            );

            if (routeOccupied)
            {
                Debug.WriteLine($"Маршрут {station.Name}-{destination.Name} зайнятий. Спавн скасовано.");
                return;
            }

            var path = _railwayManager.FindPath(station, destination);
            if (path == null || path.Count == 0) return;

            var train = new RegularTrain(_railwayManager)
            {
                CurrentTrack = station,
                DepartureStation = station, // Встановлюємо станцію відправлення
                Destination = destination,
                Path = path,
                IsImmune = true
            };
            if (path == null || path.Count == 0)
            {
                train.TryFindNewPath();
            }
            _activeTrains.Add(train);
            Debug.WriteLine($"Створено потяг {station.Name} -> {destination.Name}");
        }
        private void SpawnDrunkenTrain()
        {
            if (_railwayManager.CurrentLevel.Stations.Count == 0)
                return;

            int index = new Random().Next(_railwayManager.CurrentLevel.Stations.Count);
            Station station = _railwayManager.CurrentLevel.Stations[index];

            var drunkenTrain = new DrunkenTrain(_railwayManager)
            {
                CurrentTrack = station,
                GridPosition = station.GridPosition,
                IsImmune = true
            };
            _activeTrains.Add(drunkenTrain);
            Debug.WriteLine($"Створено п'яний потяг на станції {station.Name}");
        }
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(108, 156, 82));

            _spriteBatch.Begin(
                transformMatrix: _camera.TransformMatrix,
                sortMode: SpriteSortMode.FrontToBack,
                blendState: BlendState.AlphaBlend,
                samplerState: SamplerState.PointClamp
            );
            _collisionManager.Draw(_spriteBatch, _camera.Position);
            // малювання колій
            foreach (var track in _railwayManager.CurrentLevel.Tracks)
            {
                Texture2D texture;
                Vector2 origin = Vector2.Zero;
                Color tint = Color.White;
                if (track.IsDamaged)
                {
                    texture = _damagedTexture;
                    origin = new Vector2(texture.Width / 2, texture.Height / 2);
                    tint = Color.White;
                }
                else if (track is Station)
                {
                    texture = _stationTexture;
                    origin = new Vector2(texture.Width / 2, texture.Height / 2);
                    tint = Color.White;
                }
                else if(track is SwitchTrack switchTrack)
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
            foreach (var train in _activeTrains.ToList())
            {
                train.Update(gameTime);
                string direction = train.DirectionName;
                int frame = train.AnimationFrame + 1;
                string textureKey = $"train_{direction}{frame}";
                if (!train.IsActive)
                {
                    train.Dispose(); // Вивільнення ресурсів
                    _activeTrains.Remove(train);
                }
                if (_trainTextures.TryGetValue(textureKey, out Texture2D texture))
                {
                    Vector2 isoPos = IsometricConverter.GridToIso(train.GridPosition);
                    Vector2 origin = new Vector2(texture.Width / 2, texture.Height / 2);
                    float depth = IsometricConverter.CalculateDepth(train.TrackGridPosition, 0.01f);

                    //ОФСЕТ
                    Vector2 offset = Vector2.Zero;
                    switch (direction)
                    {
                        case "N": offset = new Vector2(-3, -10); break;
                        case "S": offset = new Vector2(-3, -10); break;
                        case "E": offset = new Vector2(0, -10); break;
                        case "W": offset = new Vector2(0, -10); break;
                    }

                    _spriteBatch.Draw(
                        texture,
                        isoPos + offset,
                        null,
                        GetTrainColor(train),
                        0f,
                        origin,
                        0.7f,
                        SpriteEffects.None,
                        depth
                    );
                }
            }
            _spriteBatch.End();

            base.Draw(gameTime);
        }
        private Color GetTrainColor(Train train)
        {
            return train switch
            {
                RegularTrain => new Color(66, 135, 245),
                DrunkenTrain => new Color(217, 79, 61),
                RepairTrain => new Color(85, 194, 89),
                _ => Color.White
            };
        }

        private void ResetGameState()
        {
            _activeTrains.Clear();
            _lastSpawnTimes.Clear();
            _lastDrunkenSpawnTime = 0;
            Debug.WriteLine("Стан гри скинуто для нового рівня");
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