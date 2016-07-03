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
            BulletTime = new TimeClass(6000);
            bulletDirection = new Vector3(1, 1, 1);
        }

        public Vector3 CalculateBulletDirection(Tank Shooter)
        {
            Vector3 scale;
            Quaternion rotation;
            Vector3 translation;
            Matrix CanonRelTransform = new Matrix();
            CanonRelTransform = GetTransformPaths(Shooter.Model.Bones[10]);
            CanonRelTransform.Decompose(out scale, out rotation, out translation);

            return Vector3.Transform(Vector3.UnitZ, rotation);
        }
        public Vector3 BulletTranslation(Tank Shooter)
        {
            Vector3 scale;
            Quaternion rotation;
            Vector3 translation;
            Matrix CanonRelTransform = new Matrix();
            CanonRelTransform = GetTransformPaths(Shooter.Model.Bones[10]);
            CanonRelTransform.Decompose(out scale, out rotation, out translation);

            return translation;
        }
    }
}