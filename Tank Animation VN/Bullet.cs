using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TankAnimationVN
{
    public class Bullet : TankAnimationVN.CModel
    {
        public bool IsFired;
        public TimeClass BulletTime;
        public Vector3 bulletDirection;

        public Bullet(Model Model, Vector3 Position, Quaternion Rotation,
                       Vector3 Scale, GraphicsDevice graphicsDevice) : base(Model,Position,Rotation,Scale,graphicsDevice)
        {
            IsFired = false;
            BulletTime = new TimeClass(3000);
            bulletDirection = new Vector3(1, 1, 1);
        }
    }
}