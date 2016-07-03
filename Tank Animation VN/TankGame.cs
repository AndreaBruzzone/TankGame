using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;

namespace TankAnimationVN
{
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont SFont;

        List<Tank> TankList = new List<Tank>();
        Tank PlayerTank, EnemyTank;

        Camera camera;
        MouseState LastMouseState;
        bool FirstRun = true;

        float canonRot = 0;
        float turretRot = 0;
        float wheelRot = 0;
        float steelRot = 0;
        float BodyRot = 0;
        float enemyRot = 0;

        bool enableEnemyFiring = false;

        Terrain Terrain;
        Effect Effect;

        Matrix PlayerTankTransform;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            this.IsMouseVisible = false;
            spriteBatch = new SpriteBatch(GraphicsDevice);
            LastMouseState = Mouse.GetState();
            base.Initialize();
        }


        protected override void LoadContent()
        {

            Effect = Content.Load<Effect>("terrain");

            SFont = Content.Load<SpriteFont>("SpriteFont");

            Terrain = new Terrain(GraphicsDevice, Content.Load<Texture2D>("desert"), Content.Load<Texture2D>("water"), Content.Load<Texture2D>("sand"), Content.Load<Texture2D>("vetta"), 1f, 128, 128, 5f);

            PlayerTank = new Tank(Content.Load<Model>("tank"), new Vector3(50, Terrain.GetHeight(50, 61), 61),
                                new Quaternion(), new Vector3(0.001f, 0.001f, 0.001f), GraphicsDevice);

            EnemyTank = new Tank(Content.Load<Model>("enemy"), new Vector3(53, Terrain.GetHeight(53, 61), 61),
                                new Quaternion(), new Vector3(0.001f, 0.001f, 0.001f), GraphicsDevice);

            PlayerTank.Bullet = new Bullet(Content.Load<Model>("Bullet"), PlayerTank.GetTransformPaths(PlayerTank.Model.Bones[10]).Translation,
                                 new Quaternion(), new Vector3(0.001f, 0.001f, 0.001f), GraphicsDevice);

            EnemyTank.Bullet = new Bullet(Content.Load<Model>("Bullet"), EnemyTank.GetTransformPaths(EnemyTank.Model.Bones[10]).Translation,
                                 new Quaternion(), new Vector3(0.001f, 0.001f, 0.001f), GraphicsDevice);

            TankList.Add(PlayerTank);
            TankList.Add(EnemyTank);

            camera = new FreeCamera(new Vector3(11, Terrain.GetHeight(11, 61) + 2, 61), -20f, 0f, GraphicsDevice);

            SFXManager.AddEffect("Explosion", Content.Load<SoundEffect>("Explosion1"));
            SFXManager.AddEffect("Jump", Content.Load<SoundEffect>("Jump"));
            SFXManager.AddEffect("PlayerShot", Content.Load<SoundEffect>("Shot1"));
            SFXManager.AddEffect("EnemyShot", Content.Load<SoundEffect>("Shot2"));

            ParticleManager.Initialize(GraphicsDevice, Content.Load<Effect>("Particles"), Content.Load<Texture2D>("Explosion"));

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
                PlayerTank.BoneTransform(10, Matrix.CreateRotationX(canonRot));
            }

            if (KState.IsKeyDown(Keys.J))
            {
                canonRot += 0.05f;
                PlayerTank.BoneTransform(10, Matrix.CreateRotationX(canonRot));
            }
            if (KState.IsKeyDown(Keys.L))
            {
                turretRot += 0.05f;
                PlayerTank.BoneTransform(9, Matrix.CreateRotationY(turretRot));
            }
            if (KState.IsKeyDown(Keys.R))
            {
                turretRot -= 0.05f;
                PlayerTank.BoneTransform(9, Matrix.CreateRotationY(turretRot));
            }

            if (KState.IsKeyDown(Keys.Left))
            {
                steelRot += 0.05f;
                if (steelRot > 1.5f)
                    steelRot = 1.5f;
                PlayerTank.BoneTransform(3, Matrix.CreateRotationY(steelRot));
                PlayerTank.BoneTransform(7, Matrix.CreateRotationY(steelRot));
            }
            if (KState.IsKeyDown(Keys.Right))
            {
                steelRot -= 0.05f;
                if (steelRot < -1.5f)
                    steelRot = -1.5f;
                PlayerTank.BoneTransform(3, Matrix.CreateRotationY(steelRot));
                PlayerTank.BoneTransform(7, Matrix.CreateRotationY(steelRot));
            }

            if (KState.IsKeyDown(Keys.Down))
            {
                BodyRot -= delta * steelRot;
                PlayerTank.BoneTransform(0, Matrix.CreateRotationY(BodyRot));

                wheelRot -= 0.05f;
                PlayerTank.RotateWheels(wheelRot);

                PlayerTank.Position += PlayerTank.GetTankTranslation() - PlayerTank.GetTankDirection() * 0.03f;
                PlayerTank.Position = new Vector3(PlayerTank.Position.X, Terrain.GetHeight(PlayerTank.Position.X, PlayerTank.Position.Z), PlayerTank.Position.Z);
            }

            if (KState.IsKeyDown(Keys.Up))
            {
                BodyRot += delta * steelRot;
                PlayerTank.BoneTransform(0, Matrix.CreateRotationY(BodyRot));

                wheelRot += 0.05f;
                PlayerTank.RotateWheels(wheelRot);

                PlayerTank.Position += PlayerTank.GetTankTranslation() + PlayerTank.GetTankDirection() * 0.03f;
                PlayerTank.Position = new Vector3(PlayerTank.Position.X, Terrain.GetHeight(PlayerTank.Position.X, PlayerTank.Position.Z),PlayerTank.Position.Z);
            }

            if (KState.IsKeyDown(Keys.F))
            {
                if (PlayerTank.Bullet.IsFired == false)               
                    SFXManager.Play("PlayerShot");                 
             
                PlayerTank.BulletFire();
            }

            if (KState.IsKeyDown(Keys.E))
            {
                if (enableEnemyFiring == false)
                {
                    enableEnemyFiring = true;
                    SFXManager.Play("PlayerShot");
                }
            }

            foreach (Tank tank in TankList)
            {
                UpdateBullet(tank, gameTime);
            }

            enemyRot += 0.05f;
            EnemyTank.BoneTransform(9, Matrix.CreateRotationY(enemyRot));

            if (enableEnemyFiring)
            {
                EnemyTank.BulletFire();
            }

            CheckBounds(PlayerTank);

            UpdateCamera(gameTime);

            base.Update(gameTime);
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

            Terrain.Draw(camera, Effect);
            ParticleManager.Draw((FreeCamera)camera);
            base.Draw(gameTime);
        }
        private void MakeExplosion(Bullet Bullet)
        {
            Vector3 impactPoint = new Vector3(
            Bullet.Position.X, 0, Bullet.Position.Z);
            impactPoint.Y = Terrain.GetHeight(
            impactPoint.X, impactPoint.Z);
            ParticleManager.MakeExplosion(impactPoint, 200);
        }           
        private void UpdateBullet(Tank Shooter, GameTime gameTime)
        {
            if (Shooter.Bullet.IsFired == true)
            {
                if (Shooter.Bullet.Position.Y > Terrain.GetHeight(Shooter.Bullet.Position.X, Shooter.Bullet.Position.Z))
                {
                    Shooter.Bullet.Position += Shooter.Bullet.bulletDirection * 0.1f - 0.0005f * Vector3.UnitY;
                }
                else
                {
                    MakeExplosion(Shooter.Bullet);
                    Shooter.Bullet.Position = Shooter.Position + Shooter.Bullet.BulletTranslation(Shooter) * new Vector3(0.01f, 0.01f, 0.01f) + Shooter.Bullet.bulletDirection * 0.04f;
                    SFXManager.Play("Explosion");
                    Shooter.Bullet.IsFired = false;
                }
                if (Shooter.Bullet.BulletTime.IsTimeEspired(gameTime))
                {
                    Shooter.Bullet.Position = Shooter.Position + Shooter.Bullet.BulletTranslation(Shooter) * new Vector3(0.01f, 0.01f, 0.01f) + Shooter.Bullet.bulletDirection * 0.04f;
                    Shooter.Bullet.IsFired = false;
                }
            }
        }
        public void UpdateCamera(GameTime gameTime)
        {
            MouseState mouseState = Mouse.GetState();
            KeyboardState keyState = Keyboard.GetState();



            float deltaX = (float)LastMouseState.X - (float)mouseState.X;
            float deltaY = (float)LastMouseState.Y - (float)mouseState.Y;

            ((FreeCamera)camera).Rotate(deltaX * 0.01f, deltaY * 0.01f);

            Vector3 translation = Vector3.Zero;

            ((FreeCamera)camera).position = PlayerTank.Position + new Vector3(-0.4f, 0.35f, 0f);
            ((FreeCamera)camera).Move(translation);


            camera.Update();
            ParticleManager.Update(gameTime);

            LastMouseState = Mouse.GetState();
        }

        public void CheckBounds(Tank tank)
        {
            if (tank.Position.X > 125 || tank.Position.Z > 125 || tank.Position.X < 2 || tank.Position.Z < 2)
                tank.Position = new Vector3(50, Terrain.GetHeight(50, 61), 61);
        }
    }
}

