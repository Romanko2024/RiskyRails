using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskyRails.GameCode.Entities
{
    public abstract class Train
    {
        public Vector2 Position { get; protected set; }
        public TrackSegment CurrentTrack { get; set; } //!!
        public bool IsActive { get; protected set; } = true;

        public abstract void Update(GameTime gameTime);

        public virtual void HandleSignal(Signal signal) //!!
        {
            //базова логіка реакції на сигнал
        }

        public virtual void HandleCollision()
        {
            IsActive = false;
        }
    }
}
