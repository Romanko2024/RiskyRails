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
