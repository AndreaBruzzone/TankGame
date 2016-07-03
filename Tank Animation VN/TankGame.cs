using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;

namespace TankAnimationVN
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont SFont;

        List<Tank> TankList = new List<Tank>();
        Tank Cmodel, Enemy;

        Camera camera;
        MouseState LastMouseState;
        bool FirstRun = true;

        Vector3 forward = new Vector3(0, 0, 0);

        float canonRot = 0;
        float turretRot = 0;
        float wheelRot = 0;
        float steelRot = 0;
        float BodyRot = 0;

        float enemyRot = 0;

        bool enableEnemyFiring = false;

        Terrain terrain;
        Effect effect;

        Matrix CmodelTransform;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            base.Initialize();
        }


        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            effect = Content.Load<Effect>("terrain");

            this.IsMouseVisible = true;

            SFont = Content.Load<SpriteFont>("SpriteFont");

            terrain = new Terrain(GraphicsDevice, Content.Load<Texture2D>("desert"), Content.Load<Texture2D>("water"), Content.Load<Texture2D>("sand"), Content.Load<Texture2D>("vetta"), 1f, 128, 128, 5f);

            Cmodel = new Tank(Content.Load<Model>("tank"), new Vector3(50, terrain.GetHeight(50, 61), 61),
                                new Quaternion(), new Vector3(0.001f, 0.001f, 0.001f), GraphicsDevice);

            Enemy = new Tank(Content.Load<Model>("enemy"), new Vector3(53, terrain.GetHeight(53, 61), 61),
                                new Quaternion(), new Vector3(0.001f, 0.001f, 0.001f), GraphicsDevice);

            var BulletTranslation = GetTransformPaths(Cmodel.Model.Bones[10]);
            var EnemyBulletTranslation = GetTransformPaths(Enemy.Model.Bones[10]);

            Cmodel.Bullet = new Bullet(Content.Load<Model>("Bullet"), BulletTranslation.Translation,
                                 new Quaternion(), new Vector3(0.001f, 0.001f, 0.001f), GraphicsDevice);
            Enemy.Bullet = new Bullet(Content.Load<Model>("Bullet"), BulletTranslation.Translation,
                                 new Quaternion(), new Vector3(0.001f, 0.001f, 0.001f), GraphicsDevice);

            TankList.Add(Cmodel);
            TankList.Add(Enemy);

            camera = new FreeCamera(new Vector3(11, terrain.GetHeight(11, 61) + 2, 61), -20f, 0f, GraphicsDevice);

            SFXManager.AddEffect("Explosion", Content.Load<SoundEffect>("Explosion1"));
            SFXManager.AddEffect("Jump", Content.Load<SoundEffect>("Jump"));
            SFXManager.AddEffect("PlayerShot", Content.Load<SoundEffect>("Shot1"));
            SFXManager.AddEffect("EnemyShot", Content.Load<SoundEffect>("Shot2"));

            ParticleManager.Initialize(GraphicsDevice, Content.Load<Effect>("Particles"), Content.Load<Texture2D>("Explosion"));

            LastMouseState = Mouse.GetState();
        }

        protected override void UnloadContent() { }
        protected override void Update(GameTime gameTime)
        {
            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (FirstRun)
            {
                LastMouseState = Mouse.GetState();
                FirstRun = false;
            }
            KeyboardState KState = Keyboard.GetState();
            if (KState.IsKeyDown(Keys.Escape))
                Exit();


            if (KState.IsKeyDown(Keys.U))
            {
                canonRot -= 0.05f;
                Cmodel.BoneTransform(10, Matrix.CreateRotationX(canonRot));
            }

            if (KState.IsKeyDown(Keys.J))
            {
                canonRot += 0.05f;
                Cmodel.BoneTransform(10, Matrix.CreateRotationX(canonRot));
            }
            if (KState.IsKeyDown(Keys.L))
            {
                turretRot += 0.05f;
                Cmodel.BoneTransform(9, Matrix.CreateRotationY(turretRot));
            }
            if (KState.IsKeyDown(Keys.R))
            {
                turretRot -= 0.05f;
                Cmodel.BoneTransform(9, Matrix.CreateRotationY(turretRot));
            }
           
                if (KState.IsKeyDown(Keys.Left))
                {
                    steelRot += 0.05f;
                    if (steelRot > 1.5f)
                        steelRot = 1.5f;
                    Cmodel.BoneTransform(3, Matrix.CreateRotationY(steelRot));
                    Cmodel.BoneTransform(7, Matrix.CreateRotationY(steelRot));
                }
                if (KState.IsKeyDown(Keys.Right))
                {
                    steelRot -= 0.05f;
                    if (steelRot < -1.5f)
                        steelRot = -1.5f;
                    Cmodel.BoneTransform(3, Matrix.CreateRotationY(steelRot));
                    Cmodel.BoneTransform(7, Matrix.CreateRotationY(steelRot));
                }

            if (KState.IsKeyDown(Keys.Down))
            {
                BodyRot -= delta * steelRot;
                Cmodel.BoneTransform(0, Matrix.CreateRotationY(BodyRot));
                CmodelTransform = GetTransformPaths(Cmodel.Model.Bones[0]);
                Vector3 scale;
                Quaternion rotation;
                Vector3 translation;
                CmodelTransform.Decompose(out scale, out rotation, out translation);
                Vector3 CmodelForward = Vector3.Transform(Vector3.UnitZ, rotation);

                Vector3 newPos = Cmodel.Position + translation - CmodelForward * 0.03f;

                wheelRot -= 0.05f;
                Cmodel.BoneTransform(2, Matrix.CreateRotationX(wheelRot));
                Cmodel.BoneTransform(4, Matrix.CreateRotationX(wheelRot));
                Cmodel.BoneTransform(6, Matrix.CreateRotationX(wheelRot));
                Cmodel.BoneTransform(8, Matrix.CreateRotationX(wheelRot));
                Cmodel.Position += translation - CmodelForward * 0.03f;
                Cmodel.Position = new Vector3(Cmodel.Position.X, terrain.GetHeight(Cmodel.Position.X, Cmodel.Position.Z), Cmodel.Position.Z);
            }

            if (KState.IsKeyDown(Keys.Up))
            {
                BodyRot += delta * steelRot;
                Cmodel.BoneTransform(0, Matrix.CreateRotationY(BodyRot));
                CmodelTransform = GetTransformPaths(Cmodel.Model.Bones[0]);
                Vector3 scale;
                Quaternion rotation;
                Vector3 translation;
                CmodelTransform.Decompose(out scale, out rotation, out translation);
                Vector3 CmodelForward = Vector3.Transform(Vector3.UnitZ, rotation);

                Vector3 newPos = Cmodel.Position + translation + CmodelForward * 0.03f;

                wheelRot += 0.05f;
                Cmodel.BoneTransform(2, Matrix.CreateRotationX(wheelRot));
                Cmodel.BoneTransform(4, Matrix.CreateRotationX(wheelRot));
                Cmodel.BoneTransform(6, Matrix.CreateRotationX(wheelRot));
                Cmodel.BoneTransform(8, Matrix.CreateRotationX(wheelRot));
                Cmodel.Position += translation + CmodelForward * 0.03f;
                Cmodel.Position = new Vector3(Cmodel.Position.X, terrain.GetHeight(Cmodel.Position.X, Cmodel.Position.Z), Cmodel.Position.Z);
            }
            
            if (KState.IsKeyDown(Keys.F))
            {
                if (Cmodel.Bullet.IsFired == false)
                {
                    Cmodel.Bullet.IsFired = true;
                    BulletFire(Cmodel);
                }
            }

            if (KState.IsKeyDown(Keys.E))
            {
                enableEnemyFiring = true;
            }   

            foreach (Tank tank in TankList)
            {
                UpdateBullet(tank, gameTime);               
            }

            enemyRot += 0.05f;
            Enemy.BoneTransform(9, Matrix.CreateRotationY(enemyRot));

            if (enableEnemyFiring)
            {
                if (Enemy.Bullet.IsFired == false)
                {
                    Enemy.Bullet.IsFired = true;
                    BulletFire(Enemy);
                }
            }

            updateCamera(gameTime);

            base.Update(gameTime);
        }
        private Matrix GetTransformPaths(ModelBone bone)
        {
            Matrix result = Matrix.Identity;
            while (bone != null)
            {
                result = result * bone.Transform;
                bone = bone.Parent;
            }
            return result;
        }
        Vector3 GetForwardVector(Quaternion rot)
        {
            return new Vector3(2 * (rot.X * rot.Z + rot.W * rot.Y),
                                    2 * (rot.Y * rot.Z - rot.W * rot.X),
                                    1 - 2 * (rot.X * rot.X + rot.Y * rot.Y));
        }
        public void updateCamera(GameTime gameTime)
        {
            MouseState mouseState = Mouse.GetState();
            KeyboardState keyState = Keyboard.GetState();



            float deltaX = (float)LastMouseState.X - (float)mouseState.X;
            float deltaY = (float)LastMouseState.Y - (float)mouseState.Y;

            ((FreeCamera)camera).Rotate(deltaX * 0.01f, deltaY * 0.01f);

            Vector3 translation = Vector3.Zero;

            ((FreeCamera)camera).position = Cmodel.Position + new Vector3(-0.4f, 0.35f, 0f);
            ((FreeCamera)camera).Move(translation);


            camera.Update();
            ParticleManager.Update(gameTime);

            LastMouseState = Mouse.GetState();
        }
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
            spriteBatch.DrawString(SFont, "Position :  " +
                  ((FreeCamera)camera).position.X.ToString() + "," +
                  ((FreeCamera)camera).position.Y.ToString() + "," +
                  ((FreeCamera)camera).position.Z.ToString() + "," +
                  "Yaw, Pitch, Roll " +
                  MathHelper.ToDegrees(((FreeCamera)camera).yaw).ToString() + "," +
                  MathHelper.ToDegrees(((FreeCamera)camera).pitch).ToString() + "," +
                  MathHelper.ToDegrees(((FreeCamera)camera).pitch).ToString(),
                  new Vector2(10, 10), Color.Black);
            spriteBatch.End();

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            foreach (Tank tank in TankList)
            {
                tank.Draw(camera.view, camera.projection);
                if (tank.Bullet.IsFired)
                {
                    tank.Bullet.Draw(camera.view, camera.projection);
                }
            }

            terrain.Draw(camera, effect);
            ParticleManager.Draw((FreeCamera)camera);
            base.Draw(gameTime);
        }
        private void MakeExplosion(Bullet Bullet)
        {
            Vector3 impactPoint = new Vector3(
            Bullet.Position.X, 0, Bullet.Position.Z);
            impactPoint.Y = terrain.GetHeight(
            impactPoint.X, impactPoint.Z);
            ParticleManager.MakeExplosion(impactPoint, 200);
        }
        private void BulletFire(Tank Shooter)
        {
            SFXManager.Play("PlayerShot");

            Vector3 scale;
            Quaternion rotation;
            Vector3 translation;
            Matrix CanonRelTransform = new Matrix();
            CanonRelTransform = GetTransformPaths(Shooter.Model.Bones[10]);
            CanonRelTransform.Decompose(out scale, out rotation, out translation);

            Shooter.Bullet.bulletDirection = CalculateBulletDirection(Shooter);

            Shooter.Bullet.Position = Shooter.Position + BulletTranslation(Shooter) * new Vector3(0.001f, 0.001f, 0.001f) + Shooter.Bullet.bulletDirection * 0.04f;
            Shooter.Bullet.Rotation = rotation;
            Shooter.Bullet.BulletTime.Start();
        }
        private void UpdateBullet(Tank Shooter,GameTime gameTime)
        {
            if (Shooter.Bullet.IsFired == true)
            {
                if (Shooter.Bullet.Position.Y > terrain.GetHeight(Shooter.Bullet.Position.X, Shooter.Bullet.Position.Z))
                {
                    Shooter.Bullet.Position += Shooter.Bullet.bulletDirection * 0.2f - 0.0005f * Vector3.UnitY;
                }
                else
                {
                    MakeExplosion(Shooter.Bullet);
                    Shooter.Bullet.Position = Shooter.Position + BulletTranslation(Shooter) * new Vector3(0.01f, 0.01f, 0.01f) + Shooter.Bullet.bulletDirection * 0.04f;
                    SFXManager.Play("Explosion");
                    Shooter.Bullet.IsFired = false;
                }
                if (Shooter.Bullet.BulletTime.IsTimeEspired(gameTime))
                {
                    Shooter.Bullet.Position = Shooter.Position + BulletTranslation(Shooter) * new Vector3(0.01f, 0.01f, 0.01f) + Shooter.Bullet.bulletDirection * 0.04f;
                    Shooter.Bullet.IsFired = false;
                }
            }
        }
        private Vector3 CalculateBulletDirection(Tank Shooter)
        {
            Vector3 scale;
            Quaternion rotation;
            Vector3 translation;
            Matrix CanonRelTransform = new Matrix();
            CanonRelTransform = GetTransformPaths(Shooter.Model.Bones[10]);
            CanonRelTransform.Decompose(out scale, out rotation, out translation);
                      
            return Vector3.Transform(Vector3.UnitZ, rotation);
        }
        private Vector3 BulletTranslation(Tank Shooter)
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

