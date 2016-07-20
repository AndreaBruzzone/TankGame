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
        float inclination = 0;

        bool playerIsOnFire = false;

        Vector3 precEnemyFiringDirection = Vector3.Zero;
        Vector3 EnemyFiringDirection = Vector3.Zero;

        bool enableForward = false, enableBackward = false;

        List<Terrain> TerrainList = new List<Terrain>();
        Effect Effect;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            //graphics.PreferredBackBufferWidth = 800;
            //graphics.PreferredBackBufferHeight = 400;
            //graphics.SynchronizeWithVerticalRetrace = false;
            //graphics.PreferMultiSampling = true;
            Content.RootDirectory = "Content";
            //IsFixedTimeStep = true;
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

            for (int i = 0; i < 9; i++)
                if(i==4)
                    TerrainList.Add(new Terrain(GraphicsDevice, Content.Load<Texture2D>("heightmap4"), Content.Load<Texture2D>("sand"), Content.Load<Texture2D>("sand"), Content.Load<Texture2D>("vetta"), 1f, 128, 128, 5f));
                 else
                    TerrainList.Add(new Terrain(GraphicsDevice, Content.Load<Texture2D>("heightmapext"), Content.Load<Texture2D>("sand"), Content.Load<Texture2D>("sand"), Content.Load<Texture2D>("vetta"), 1f, 128, 128, 5f));


            PlayerTank = new Tank(Content.Load<Model>("tank"), new Vector3(10, TerrainList[4].GetHeight(10, 118), 118),
                                new Quaternion(), new Vector3(0.001f, 0.001f, 0.001f), GraphicsDevice);

            EnemyTank = new Tank(Content.Load<Model>("enemy"), new Vector3(15, TerrainList[4].GetHeight(15, 118), 118),
                                new Quaternion(), new Vector3(0.001f, 0.001f, 0.001f), GraphicsDevice);

            PlayerTank.Bullet = new Bullet(Content.Load<Model>("Bullet"), PlayerTank.GetTransformPaths(PlayerTank.Model.Bones[10]).Translation,
                                 new Quaternion(), new Vector3(0.001f, 0.001f, 0.001f), GraphicsDevice);

            EnemyTank.Bullet = new Bullet(Content.Load<Model>("Bullet"), EnemyTank.GetTransformPaths(EnemyTank.Model.Bones[10]).Translation,
                                 new Quaternion(), new Vector3(0.001f, 0.001f, 0.001f), GraphicsDevice);

            TankList.Add(PlayerTank);
            TankList.Add(EnemyTank);

            camera = new FreeCamera(new Vector3(11, TerrainList[4].GetHeight(11, 61) + 2, 61), -20f, 0f, GraphicsDevice);

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
                steelRot += 0.03f;
                if (steelRot > 0.75f)
                    steelRot = 0.75f;

                PlayerTank.BoneTransform(3, Matrix.CreateRotationY(steelRot));
                PlayerTank.BoneTransform(7, Matrix.CreateRotationY(steelRot));
            }
            if (KState.IsKeyDown(Keys.Right))
            {
                steelRot -= 0.03f;
                if (steelRot < -0.75f)
                    steelRot = -0.75f;
                PlayerTank.BoneTransform(3, Matrix.CreateRotationY(steelRot));
                PlayerTank.BoneTransform(7, Matrix.CreateRotationY(steelRot));

            }

            if (KState.IsKeyDown(Keys.Down))
            {
                if (CheckInclinationForMove() || enableBackward == true)
                {
                    if (CheckInclinationForMove())
                        enableBackward = false;
                    BodyRot -= delta * steelRot;

                    DoTankTransform();

                    wheelRot -= 0.05f;
                    PlayerTank.RotateWheels(wheelRot);

                    PlayerTank.Position += PlayerTank.GetTankTranslation() - PlayerTank.GetTankDirection() * 0.01f;
                    PlayerTank.Position = new Vector3(PlayerTank.Position.X, TerrainList[4].GetHeight(PlayerTank.Position.X, PlayerTank.Position.Z), PlayerTank.Position.Z);
                }
                else
                    enableForward = true;
            }

            if (KState.IsKeyDown(Keys.Up))
            {
                if (CheckInclinationForMove() || enableForward == true)
                {
                    if(CheckInclinationForMove())
                        enableForward = false;
                    
                    BodyRot += delta * steelRot;

                    DoTankTransform();

                    wheelRot += 0.05f;
                    PlayerTank.RotateWheels(wheelRot);

                    PlayerTank.Position += PlayerTank.GetTankTranslation() + PlayerTank.GetTankDirection() * 0.01f;
                    PlayerTank.Position = new Vector3(PlayerTank.Position.X, TerrainList[4].GetHeight(PlayerTank.Position.X, PlayerTank.Position.Z), PlayerTank.Position.Z);
                }
                else
                    enableBackward = true;
            }

            if (KState.IsKeyDown(Keys.F))
            {
                if (PlayerTank.Bullet.IsFired == false)
                    SFXManager.Play("PlayerShot");

                PlayerTank.BulletFire();
            }

            foreach (Tank tank in TankList)
            {
                UpdateBullet(tank, gameTime);
            }

            if (!playerIsOnFire)
            {
                enemyRot += 0.03f;
                EnemyTank.BoneTransform(9, Matrix.CreateRotationY(enemyRot));
            }

            EnemyAutoFiring();
        
            CheckBounds(PlayerTank);

            UpdateCamera(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkGray);
            KeyboardState KState = Keyboard.GetState();

            spriteBatch.Begin();
            spriteBatch.DrawString(SFont, "Position :  " +
                  ((FreeCamera)camera).position.X.ToString() + "," +
                  ((FreeCamera)camera).position.Y.ToString() + "," +
                  ((FreeCamera)camera).position.Z.ToString() + ",",
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

            TerrainList[0].Draw(camera, Effect, -127 , 127);
            TerrainList[1].Draw(camera, Effect, 0, 127);
            TerrainList[2].Draw(camera, Effect, 127, 127);
            TerrainList[3].Draw(camera, Effect, -127, 0);
            TerrainList[4].Draw(camera, Effect);
            TerrainList[5].Draw(camera, Effect, 127, 0);
            TerrainList[6].Draw(camera, Effect, -127, -127);
            TerrainList[7].Draw(camera, Effect, 0,-127);
            TerrainList[8].Draw(camera, Effect, 127, -127);


            ParticleManager.Draw((FreeCamera)camera);
            base.Draw(gameTime);
        }
        private void MakeExplosion(Bullet Bullet)
        {
            Vector3 impactPoint = new Vector3(Bullet.Position.X, Bullet.Position.Y, Bullet.Position.Z);
            ParticleManager.MakeExplosion(impactPoint, 10000);
        }

        public void DoTankTransform()
        {
            //Preparo inclinazione
            Vector3 tankAxis = CalculateTankDirection();
            tankAxis.Normalize();
            inclination += NextInclination(tankAxis);
            PlayerTank.BoneTransform(0, Matrix.CreateRotationY(BodyRot) * Matrix.CreateFromAxisAngle(tankAxis, inclination));
        }
        private void UpdateBullet(Tank Shooter, GameTime gameTime)
        {
            if (Shooter.Bullet.IsFired == true)
            {
                if (Shooter.Bullet.Position.Y > TerrainList[4].GetHeight(Shooter.Bullet.Position.X, Shooter.Bullet.Position.Z)) //Controllo la posizione del proiettile
                {
                    Shooter.Bullet.Position += Shooter.Bullet.bulletDirection * 0.05f - 0.001f * Vector3.UnitY;
                }
                else //Proiettile tocca terra
                {
                    MakeExplosion(Shooter.Bullet);
                    Shooter.Bullet.Position = Shooter.Position + Shooter.Bullet.BulletTranslation(Shooter) * new Vector3(0.01f, 0.01f, 0.01f) + Shooter.Bullet.bulletDirection * 0.04f;
                    SFXManager.Play("Explosion");
                    Shooter.Bullet.IsFired = false;
                }
                if (Shooter.Bullet.BulletTime.IsTimeEspired(gameTime)) //Proiettile ha esaurito il tempo (volerebbe indefinitivamente, quindi lo ignoro)
                {
                    MakeExplosion(Shooter.Bullet);
                    Shooter.Bullet.Position = Shooter.Position + Shooter.Bullet.BulletTranslation(Shooter) * new Vector3(0.01f, 0.01f, 0.01f) + Shooter.Bullet.bulletDirection * 0.04f;
                    Shooter.Bullet.IsFired = false;
                }
                foreach (Tank tank in TankList)
                {
                    if (CollisionCheck(tank, Shooter.Bullet)) //Proiettile colpisce un altro carro
                    {
                        if (Shooter != tank)
                        {
                            MakeExplosion(Shooter.Bullet);
                            Shooter.Bullet.Position = Shooter.Position + Shooter.Bullet.BulletTranslation(Shooter) * new Vector3(0.01f, 0.01f, 0.01f) + Shooter.Bullet.bulletDirection * 0.04f;
                            SFXManager.Play("Explosion");
                            Shooter.Bullet.IsFired = false;
                        }
                    }
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

            Vector3 translation = PlayerTank.GetTankTranslation();

            ((FreeCamera)camera).position = PlayerTank.Position + PlayerTank.GetTankTranslation() + new Vector3(-0.4f, 0.35f, 0f);
            ((FreeCamera)camera).Move(translation);


            camera.Update();
            ParticleManager.Update(gameTime);

            LastMouseState = Mouse.GetState();
        }

        public void CheckBounds(Tank tank)
        {
            if (tank.Position.X > 126 )
                tank.Position = new Vector3(2, TerrainList[4].GetHeight(2, tank.Position.Z), tank.Position.Z);
            if (tank.Position.X < 2)
                tank.Position = new Vector3(126, TerrainList[4].GetHeight(126, tank.Position.Z), tank.Position.Z);           
            if (tank.Position.Z > 126)
                tank.Position = new Vector3(tank.Position.X, TerrainList[4].GetHeight(tank.Position.X, 2), 2);     
            if (tank.Position.Z < 2)
                tank.Position = new Vector3(tank.Position.X, TerrainList[4].GetHeight(tank.Position.X, 126), 126);
        }

        public bool CollisionCheck(Tank tank, Bullet bullet)
        {
            BoundingSphere sphere1 = tank.Model.Meshes[2].BoundingSphere;
            sphere1 = sphere1.Transform(tank.baseworld);

            if (sphere1.Intersects(bullet.BoundingSphere))
                return true;
            else
                return false;
        }
        public void EnemyAutoFiring()
        {
            Vector3 directionOfFiring = (PlayerTank.Position - EnemyTank.Position);

            float heightDiff = PlayerTank.Position.Y - EnemyTank.Position.Y;
            float angle = (float)Math.Asin(heightDiff / directionOfFiring.Length());
            EnemyTank.BoneTransform(10, Matrix.CreateRotationX(-angle));

            float distance = directionOfFiring.Length();
            directionOfFiring.Normalize();

            Vector3 directionOfEnemyTurret = EnemyTank.Bullet.CalculateBulletDirection(EnemyTank);
            directionOfEnemyTurret.Normalize();
            if (AllowFire(directionOfEnemyTurret, directionOfFiring, distance, playerIsOnFire))
            {
                playerIsOnFire = true;
                EnemyFiringDirection = PlayerTank.Position - EnemyTank.Position;
                if (precEnemyFiringDirection == Vector3.Zero)
                    precEnemyFiringDirection = EnemyFiringDirection;

                Vector2 EnemyFiringDirection2D = new Vector2(EnemyFiringDirection.X, EnemyFiringDirection.Z);
                Vector2 precEnemyFiringDirection2D = new Vector2(precEnemyFiringDirection.X, precEnemyFiringDirection.Z);

                float rotation = FindAngleBetweenTwoVectors(EnemyFiringDirection2D, precEnemyFiringDirection2D);
                    
                enemyRot -= rotation;
                EnemyTank.BoneTransform(9, Matrix.CreateRotationY(enemyRot));

                if (!EnemyTank.Bullet.IsFired && AllowFire(directionOfEnemyTurret, directionOfFiring, distance, playerIsOnFire))
                {
                    EnemyTank.BulletFire();
                    SFXManager.Play("PlayerShot");
                }
                if (distance > 4f)
                {
                    playerIsOnFire = false;
                    precEnemyFiringDirection = Vector3.Zero;
                }

                precEnemyFiringDirection = EnemyFiringDirection;
            }
        }

        public bool AllowFire(Vector3 directionOfEnemyTurret, Vector3 directionOfFiring, float distance, bool playerIsOnFire)
        {
            if (directionOfEnemyTurret.X > directionOfFiring.X - 0.02f && directionOfEnemyTurret.X < directionOfFiring.X + 0.02f & directionOfEnemyTurret.Z > directionOfFiring.Z - 0.02f && directionOfEnemyTurret.Z < directionOfFiring.Z + 0.02f & distance < 4f || playerIsOnFire == true)
                return true;
            else
                return false;
        }
        public float FindAngleBetweenTwoVectors(Vector2 v1, Vector2 v2)
        {
            float angle;  
            v1.Normalize();
            v2.Normalize();

            angle = (float)Math.Acos(Vector2.Dot(v1, v2)); 

            if (Math.Abs(angle) < 0.001 || double.IsNaN(angle))
                return 0;

            int sign = (v1.Y * v2.X - v2.Y * v1.X) > 0 ? 1 : -1;

            angle *= sign;

            return angle;
        }

        public bool CheckInclinationForMove()
        {
            Vector3 tankDirection2 = CalculateTankDirection();
            tankDirection2.Normalize();
            Vector3 tankDirection = tankDirection2 * 0.15f;

            float tankForwardHeight = TerrainList[4].GetHeight(PlayerTank.Position.X + tankDirection.X, PlayerTank.Position.Z + tankDirection.Z);
            float tankBackwardHeight = TerrainList[4].GetHeight(PlayerTank.Position.X - tankDirection.X, PlayerTank.Position.Z - tankDirection.Z);
            float tankHeight = TerrainList[4].GetHeight(PlayerTank.Position.X, PlayerTank.Position.Z);

            //if (Math.Abs(NextInclination(tankDirection2)) > 0.5f)                   //-------> Togli commento per bloccare scalata troppo ripida
            if (false)                                                                //-------> Il carro scala qualsiasi montagna
                return false;
            else
                return true;
        }

        public float NextInclination(Vector3 tankDirection)
        {
            Vector3 tankDirectionNormalized = tankDirection * 0.15f;

            float tankForwardHeight = TerrainList[4].GetHeight(PlayerTank.Position.X + tankDirectionNormalized.X, PlayerTank.Position.Z + tankDirectionNormalized.Z);
            Vector3 tankForwardPos = new Vector3(PlayerTank.Position.X + tankDirectionNormalized.X, tankForwardHeight, PlayerTank.Position.Z + tankDirectionNormalized.Z);
            Vector3 heigtdir = (tankForwardPos - PlayerTank.Position);
            heigtdir.Normalize();

            float inclination = (float)Math.Acos(Vector3.Dot(heigtdir, tankDirection));

            if (heigtdir.Y > tankDirection.Y)
                inclination = inclination * -1;

            if (Double.IsNaN(inclination))
                inclination = 0f;

            return inclination;
        }

        public Vector3 CalculateTankDirection()
        {
            Vector3 scale;
            Quaternion rotation;
            Vector3 translation;
            Matrix CanonRelTransform = new Matrix();
            CanonRelTransform = GetTransformPaths(PlayerTank.Model.Bones[0]);
            CanonRelTransform.Decompose(out scale, out rotation, out translation);
            return Vector3.Transform(Vector3.UnitZ, rotation);
        }
        public Matrix GetTransformPaths(ModelBone bone)
        {
            Matrix result = Matrix.Identity;
            while (bone != null)
            {
                result = result * bone.Transform;
                bone = bone.Parent;
            }
            return result;
        }
    }
}

