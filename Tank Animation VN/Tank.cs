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
        public float canonRot = 0;
        public float turretRot = 0;
        public float wheelRot = 0;
        public float steelRot = 0;
        public float BodyRot = 0;
        public float enemyRot = 0;
        public float yinclination = 0;
        public float zinclination = 0;

        public Vector3 precEnemyFiringDirection = Vector3.Zero;
        public Vector3 EnemyFiringDirection = Vector3.Zero;

        public bool playerFiring = false;
        public bool enableForward = false;
        public bool enableBackward = false;

        public int HitsCounter = 0;


        public Tank(Model Model, Vector3 Position, Quaternion Rotation,
                       Vector3 Scale, GraphicsDevice graphicsDevice) : base(Model, Position, Rotation, Scale, graphicsDevice)
        {        }

        public void RotateWheels(float rotation)
        {
            this.BoneTransform(2, Matrix.CreateRotationX(rotation));
            this.BoneTransform(4, Matrix.CreateRotationX(rotation));
            this.BoneTransform(6, Matrix.CreateRotationX(rotation));
            this.BoneTransform(8, Matrix.CreateRotationX(rotation));
        }

        public Vector3 GetTankDirection()
        {
            Vector3 scale, translation;
            Quaternion rotation;
            Matrix CmodelTransform = new Matrix();
            CmodelTransform = GetTransformPaths(this.Model.Bones[0]);
            CmodelTransform.Decompose(out scale, out rotation, out translation);
            return Vector3.Transform(Vector3.UnitZ, rotation);
        }

        public Vector3 GetTankTranslation()
        {
            Vector3 scale, translation;
            Quaternion rotation;
            Matrix CmodelTransform = new Matrix();
            CmodelTransform = GetTransformPaths(this.Model.Bones[0]);
            CmodelTransform.Decompose(out scale, out rotation, out translation);
            return translation;
        }
        public void BulletFire()
        {
            if (this.Bullet.IsFired == false)
            {
                this.Bullet.IsFired = true;
                Vector3 scale;
                Quaternion rotation;
                Vector3 translation;
                Matrix CanonRelTransform = new Matrix();
                CanonRelTransform = GetTransformPaths(this.Model.Bones[10]);
                CanonRelTransform.Decompose(out scale, out rotation, out translation);

                this.Bullet.bulletDirection = this.Bullet.CalculateBulletDirection(this);

                this.Bullet.Position = this.Position + this.Bullet.BulletTranslation(this) * new Vector3(0.001f, 0.001f, 0.001f) + this.Bullet.bulletDirection * 0.04f;
                this.Bullet.Rotation = rotation;
                this.Bullet.BulletTime.Start();
            }
        }
    }
}