using Microsoft.Xna.Framework;
using RiskyRails.GameCode.Entities.Trains;
using RiskyRails.GameCode.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskyRails.GameCode.Entities
{
    /// <summary>
    /// Базовий клас сегмента залізничної інфраструктури
    /// </summary>
    public class TrackSegment
    {
        // властивості
        private Vector2 _gridPosition;
        public Vector2 GridPosition
        {
            get => _gridPosition;
            set => _gridPosition = new Vector2((int)value.X, (int)value.Y);
        }           // позиція на сітці
        public bool IsDamaged { get; private set; }         // чи пошкоджений сегмент
        public bool IsSwitch { get; set; } = false;         // чи є стрілкою
        public Signal Signal { get; set; }                   // пов'язаний світлофор
        public List<TrackSegment> ConnectedSegments { get; } = new();  // з'єднані сегменти

        public TrackType Type { get; set; } = TrackType.StraightX;
        public enum TrackType
        {
            StraightX,
            StraightY,
            StraightX_Signal,
            StraightY_Signal,
            CurveNE,
            CurveSE,
            CurveSW,
            CurveNW
        }
        public virtual List<Vector2> GetConnectionPoints()
        {
            return Type switch
            {
                TrackType.StraightX_Signal => new List<Vector2> { Vector2.UnitX, -Vector2.UnitX },
                TrackType.StraightY_Signal => new List<Vector2> { Vector2.UnitY, -Vector2.UnitY },
                TrackType.StraightX => new List<Vector2> { Vector2.UnitX, -Vector2.UnitX },
                TrackType.StraightY => new List<Vector2> { Vector2.UnitY, -Vector2.UnitY },
                TrackType.CurveNE => new List<Vector2> { Vector2.UnitX, -Vector2.UnitY },
                TrackType.CurveSE => new List<Vector2> { Vector2.UnitX, Vector2.UnitY },
                TrackType.CurveSW => new List<Vector2> { -Vector2.UnitX, Vector2.UnitY },
                TrackType.CurveNW => new List<Vector2> { -Vector2.UnitX, -Vector2.UnitY },
                _ => new List<Vector2>()
            };
        }
        public bool CanConnectTo(TrackSegment other, Vector2 direction)
        {
            var reverseDir = -direction;
            bool canConnect = other.GetConnectionPoints().Contains(reverseDir);
            if (this is Station)
            {
                // Станція приймає з'єднання з будь-якого напрямку
                return true;
            }

            Debug.WriteLine($"CanConnectTo: {GridPosition} -> {other.GridPosition} | " +
                            $"Direction: {direction} | Reverse: {reverseDir} | Result: {canConnect}");

            return canConnect;
        }
        // Методи
        public bool CanPassThrough(Train train)
        {
            if (train is RepairTrain) return true;

            // Для станцій завжди дозволяємо проїзд
            if (this is Station) return true;

            // Для звичайних колій
            return !IsDamaged && (Signal?.IsGreen ?? true);
        }

        public void MarkAsDamaged()
        {
            if (this is Station) return;
            IsDamaged = true;
            Signal = null; // світлофор знищується при пошкодженні
        }

        public void ConnectTo(TrackSegment segment)
        {
            if (!ConnectedSegments.Contains(segment))
            {
                ConnectedSegments.Add(segment);
                segment.ConnectedSegments.Add(this);
            }
        }
        public void SetDamaged(bool isDamaged)
        {
            IsDamaged = isDamaged;
        }
        protected virtual void OnTypeChanged()
        {
            //
        }

    }
    public class SwitchTrack : TrackSegment
    {
        public TrackType SecondaryType { get; private set; }
        private TrackType _primaryType;

        public SwitchTrack(TrackType primaryType, TrackType secondaryType)
            : base()
        {
            var forbiddenTypes = new[] {
            TrackType.StraightX_Signal,
            TrackType.StraightY_Signal
        };

            if (forbiddenTypes.Contains(primaryType) ||
                forbiddenTypes.Contains(secondaryType))
            {
                throw new ArgumentException("Не можна використовувати сигнали в стрілках");
            }

            _primaryType = primaryType;
            SecondaryType = secondaryType;
            Type = primaryType;
            IsSwitch = true;
        }

        public void Toggle()
        {
            Type = (Type == _primaryType) ? SecondaryType : _primaryType;
            OnTypeChanged();
        }

        protected override void OnTypeChanged()
        {
            base.OnTypeChanged();
            ConnectedSegments.Clear();
        }

        public override List<Vector2> GetConnectionPoints()
        {
            return Type switch
            {
                TrackType.StraightX_Signal => new List<Vector2> { Vector2.UnitX, -Vector2.UnitX },
                TrackType.StraightY_Signal => new List<Vector2> { Vector2.UnitY, -Vector2.UnitY },
                TrackType.StraightX => new List<Vector2> { Vector2.UnitX, -Vector2.UnitX },
                TrackType.StraightY => new List<Vector2> { Vector2.UnitY, -Vector2.UnitY },
                TrackType.CurveNE => new List<Vector2> { Vector2.UnitX, -Vector2.UnitY },
                TrackType.CurveSE => new List<Vector2> { Vector2.UnitX, Vector2.UnitY },
                TrackType.CurveSW => new List<Vector2> { -Vector2.UnitX, Vector2.UnitY },
                TrackType.CurveNW => new List<Vector2> { -Vector2.UnitX, -Vector2.UnitY },
                _ => base.GetConnectionPoints()
            };
        }
    }
}
