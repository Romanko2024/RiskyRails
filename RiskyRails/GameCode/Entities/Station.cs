using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskyRails.GameCode.Entities
{
    public class Station : TrackSegment
    {
        public string Name { get; set; } = "Unnamed Station";
        public List<Train> SpawnedTrains { get; } = new();
    }
}
