using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;
using RiskyRails.GameCode.Entities;
using RiskyRails.GameCode.Managers;
using System.Diagnostics;
using static RiskyRails.GameCode.Entities.TrackSegment;
namespace RiskyRails.GameCode.Entities.Trains
{
    /// <summary>
    /// поїзд, що дотримується правил
    /// </summary>
    public class RegularTrain : Train
    {
        public Station Destination { get; set; }
        private float _progress; // прогрес руху між сегментами (0-1)
        private float _pathUpdateTimer;
        private const float PathUpdateInterval = 2.0f;
        private readonly RailwayManager _railwayManager;
        private int _rerouteAttempts = 0;
        private const int MaxRerouteAttempts = 3;
        private SwitchTrack _currentSwitch = null;
        private TrackType _lastSwitchState;
        public Station DepartureStation { get; set; }

        public RegularTrain(RailwayManager railwayManager)
        {
            _railwayManager = railwayManager;
            Speed = 0.3f;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (IsStoppedBySignal)
            {
                WaitingTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

                //чекати 3 секунди перед переплануванням маршруту
                if (WaitingTime > 3.0f)
                {
                    TryFindNewPath();
                    WaitingTime = 0;
                    IsStoppedBySignal = false;
                    Speed = 0.3f;
                }
                return;
            }
            if (CurrentTrack.Signal != null && !CurrentTrack.Signal.IsGreen)
            {
                HandleSignal(CurrentTrack.Signal);
                return;
            }
            if (!CurrentTrack.CanPassThrough(this))
            {
                IsActive = false;
                return;
            }

            //оновлення шляху
            _pathUpdateTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_pathUpdateTimer >= PathUpdateInterval)
            {
                TryFindNewPath();
                _pathUpdateTimer = 0;
            }


            //обробка відсутності шляху
            if (Path.Count == 0)
            {
                if (!IsStoppedBySignal)
                {
                    TryFindNewPath();
                }

                if (Path.Count == 0)
                {
                    Speed = 0;
                    return;
                }
            }

            //
            if (Path.Count > 0 && !IsStoppedBySignal)
            {
                var targetTrack = Path.Peek();
                //перевірка зміни стану стрілки
                if (CurrentTrack is SwitchTrack currentSwitch)
                {
                    //якщо стрілка змінила стан
                    if (_currentSwitch == null || _currentSwitch != currentSwitch)
                    {
                        _currentSwitch = currentSwitch;
                        _lastSwitchState = currentSwitch.Type;
                    }
                    else if (_currentSwitch.Type != _lastSwitchState)
                    {
                        Debug.WriteLine($"Стрілка змінила положення! Шукаємо новий шлях...");
                        TryFindNewPath();
                        _lastSwitchState = _currentSwitch.Type;
                        _pathUpdateTimer = 0;
                        return;
                    }
                }
                else
                {
                    _currentSwitch = null;
                }
                //перевірка коректності з'єднання
                //без цього поїзд не зупиняється на стрілці а зникає...
                if (CurrentTrack is SwitchTrack switchTrack)
                {
                    Vector2 direction = targetTrack.GridPosition - switchTrack.GridPosition;

                    if (!switchTrack.GetConnectionPoints().Contains(direction))
                    {
                        UpdatePath();
                        _pathUpdateTimer = 0;
                        return;
                    }
                }

                //перевірка з'єднання
                if (!CurrentTrack.ConnectedSegments.Contains(targetTrack))
                {
                    Path.Clear();
                    IsActive = false;
                    return;
                }

                //рух
                _progress += Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                GridPosition = Vector2.Lerp(CurrentTrack.GridPosition, targetTrack.GridPosition, _progress);

                if (_progress >= 1.0f)
                {
                    CurrentTrack = Path.Dequeue();
                    _progress = 0;
                    GridPosition = CurrentTrack.GridPosition;
                    if (CurrentTrack == Destination) IsActive = false;
                }
            }
            else
            {
                IsActive = false;
            }
        }
        internal void TryFindNewPath(bool forceReroute = false)
        {
            if (forceReroute)
            {
                _rerouteAttempts = 0;
            }

            UpdatePath();
            if (Path.Count > 0) return;

            var availableStations = _railwayManager.CurrentLevel.Stations
                .Where(s => s != DepartureStation)
                .OrderBy(s => Vector2.Distance(CurrentTrack.GridPosition, s.GridPosition))
                .ToList();

            foreach (var station in availableStations)
            {
                Destination = station;
                UpdatePath();
                if (Path.Count > 0)
                {
                    Debug.WriteLine($"Потяг перенаправлено до {station.Name}");
                    return;
                }
            }

            if (Speed == 0 && CurrentTrack is SwitchTrack switchTrack)
            {
                if (_currentSwitch == null)
                {
                    _currentSwitch = switchTrack;
                    _lastSwitchState = switchTrack.Type;
                }
                else if (_currentSwitch.Type != _lastSwitchState)
                {
                    Debug.WriteLine($"Потяг стоїть на зміненій стрілці! Шукаємо новий шлях...");
                    TryFindNewPath(true);
                    _lastSwitchState = _currentSwitch.Type;
                }
            }
            //якщо новий маршрут не знайдено
            if (_rerouteAttempts < MaxRerouteAttempts)
            {
                _rerouteAttempts++;
                Debug.WriteLine($"Спроба {_rerouteAttempts}/{MaxRerouteAttempts} знайти маршрут не вдалася");
            }
            else
            {
                Debug.WriteLine($"Маршрут не знайдено. Потяг зупинено.");
                IsActive = false;
            }
        }
        private void UpdatePath()
        {
            if (CurrentTrack != null && Destination != null)
            {
                var newPath = _railwayManager.FindPath(CurrentTrack, Destination);
                if (newPath != null && newPath.Count > 0)
                {
                    Path = newPath;
                }
            }
        }

        public override void HandleSignal(Signal signal)
        {
            if (!signal.IsGreen)
            {
                _currentSwitch = null;
                //зупинка на червоний сигнал
                Speed = 0;
                Path.Clear();
                IsStoppedBySignal = true;
                StoppedSignal = signal;
            }
        }
    }
}
