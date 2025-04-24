using RiskyRails.GameCode.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskyRails.GameCode.Interfaces
{
    /// <summary>
    /// Інтерфейс для об'єктів, що ремонтуюють колії
    /// </summary>
    public interface IRepairable
    {
        void Repair(TrackSegment track);
        bool IsRepairing { get; }
    }
}
