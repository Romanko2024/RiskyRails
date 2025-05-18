using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
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
        public Vector2 GridPosition { get; set; }           // позиція на сітці
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
            return other.GetConnectionPoints().Contains(reverseDir);
        }
        // Методи
        public bool CanPassThrough(Train train)
        {
            // перевірка можливості проїзду
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

        public TrackType PrimaryType => _primaryType;

        public SwitchTrack(TrackType primaryType, TrackType secondaryType)
            : base()
        {
            if (!IsValidTypeCombination(primaryType, secondaryType))
                throw new ArgumentException("Недопустима комбінація типів для стрілки");

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
                TrackType.StraightX => new List<Vector2> { Vector2.UnitX, -Vector2.UnitX },
                TrackType.StraightY => new List<Vector2> { Vector2.UnitY, -Vector2.UnitY },
                TrackType.CurveNE => new List<Vector2> { Vector2.UnitX, -Vector2.UnitY },
                TrackType.CurveSE => new List<Vector2> { Vector2.UnitX, Vector2.UnitY },
                TrackType.CurveSW => new List<Vector2> { -Vector2.UnitX, Vector2.UnitY },
                TrackType.CurveNW => new List<Vector2> { -Vector2.UnitX, -Vector2.UnitY },
                _ => base.GetConnectionPoints()
            };
        }

        private static bool IsValidTypeCombination(TrackType t1, TrackType t2)
        {
            var validPairs = new HashSet<(TrackType, TrackType)>
        {
            (TrackType.StraightX, TrackType.CurveNE),
            (TrackType.StraightX, TrackType.CurveSE),
            (TrackType.StraightY, TrackType.CurveNW),
            (TrackType.StraightY, TrackType.CurveSW)
        };

            return validPairs.Contains((t1, t2)) || validPairs.Contains((t2, t1));
        }
    }
}
