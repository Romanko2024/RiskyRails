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
            CurveNE,
            CurveSE,
            CurveSW,
            CurveNW
        }
        public List<Vector2> GetConnectionPoints()
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
    }
}
