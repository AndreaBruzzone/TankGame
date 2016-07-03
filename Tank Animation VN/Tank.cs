using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TankAnimationVN
{
    public class Tank : TankAnimationVN.CModel
    {
        public Bullet Bullet;
        public Tank(Model Model, Vector3 Position, Quaternion Rotation,
                       Vector3 Scale, GraphicsDevice graphicsDevice) : base(Model, Position, Rotation, Scale, graphicsDevice)
        {

        }
    }
}