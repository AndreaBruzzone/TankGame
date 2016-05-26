using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TankAnimationVN
{
    public class TimeClass
    {
        float TimePassed = 0;
        float Interval;
        public TimeClass(float Interval)
        {
            this.Interval = Interval;
        }
        public void Start()
        {
            TimePassed = 0;
        }
        public bool IsTimeEspired(GameTime gameTime)
        {
            TimePassed += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (TimePassed > Interval)
            {
                TimePassed = 0;
                return true;
            }
            return false;
        }
    }
}
