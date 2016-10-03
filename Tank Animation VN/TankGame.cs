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
        TimeClass bulletTimer;
        List<Tank> TankList = new List<Tank>();
        Tank PlayerTank, EnemyTank;
        Camera camera;
        MouseState LastMouseState;
        List<Terrain> TerrainList = new List<Terrain>();
        Effect Effect;
        Random random;
        CModel Arrow;

        int bulletTimerCounter = 3000;
        int enemySpeedUpCnt = 0;

        bool FirstRun = true;
        bool playerIsOnFire = false;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            this.IsMouseVisible = false;
            random = new Random();
            spriteBatch = new SpriteBatch(GraphicsDevice);
            LastMouseState = Mouse.GetState();
            bulletTimer = new TimeClass(1500);
            bulletTimer.Start();
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

            //Arrow = new CModel(Content.Load<Model>("bull"), PlayerTank.Position + new Vector3(0,0.3f,0),
            //                    new Quaternion(), new Vector3(0.005f, 0.005f, 0.005f), GraphicsDevice);

            TankList.Add(PlayerTank);
            TankList.Add(EnemyTank);


            camera = new FreeCamera(new Vector3(11, TerrainList[4].GetHeight(11, 61) + 2, 61), -20f, 0f, GraphicsDevice);

            Arrow = new CModel(Content.Load<Model>("bull"), PlayerTank.Position,
                                new Quaternion(), new Vector3(0.005f, 0.005f, 0.005f), GraphicsDevice);

            SFXManager.AddEffect("Explosion", Content.Load<SoundEffect>("Explosion1"));
            SFXManager.AddEffect("Jump", Content.Load<SoundEffect>("Jump"));
            SFXManager.AddEffect("PlayerShot", Content.Load<SoundEffect>("Shot1"));
            SFXManager.AddEffect("EnemyShot", Content.Load<SoundEffect>("Shot2"));

            ParticleManager.Initialize(GraphicsDevice, Content.Load<Effect>("Particles"), Content.Load<Texture2D>("Explosion"));
        }

        protected override void Update(GameTime gameTime)
        {            
            foreach (Tank tank in TankList)
            {
                UpdateBullet(tank, gameTime);

                if (tank == PlayerTank)
                {
                    PlayerTankControls(gameTime);
                    CheckBounds(tank);
                    continue;
                }

                EnemyAutoFiring(gameTime, tank);

                if (!playerIsOnFire)
                {
                    tank.turretRot += 0.03f;
                    tank.BoneTransform(9, Matrix.CreateRotationY(tank.turretRot));
                    tank.precEnemyFiringDirection = Vector3.Zero;
                }
            }

            Vector3 distance = PlayerTank.Position - EnemyTank.Position;
            Vector3 oldEnemyPosition = EnemyTank.Position;
            if (distance.Length() > 6f)
            {

                EnemyTank.Position = PlayerTank.Position + new Vector3(random.Next(-5, 5), 0, random.Next(-5, 5));
                
                //if (TerrainList[4].GetHeight(EnemyTank.Position.X, EnemyTank.Position.Z) < 0.1f)
                    EnemyTank.Position = new Vector3(EnemyTank.Position.X, TerrainList[4].GetHeight(EnemyTank.Position.X, EnemyTank.Position.Z), EnemyTank.Position.Z);
                //else
                    //EnemyTank.Position = oldEnemyPosition;
            }

            //updateArrow();    Utilizzata come debug per vedere il puntamento di vettori calcolati

            UpdateCamera(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkGray);
            KeyboardState KState = Keyboard.GetState();

            //spriteBatch.Begin();
            //spriteBatch.DrawString(SFont, "Position :  " +
            //      ((FreeCamera)camera).position.X.ToString() + "," +
            //      ((FreeCamera)camera).position.Y.ToString() + "," +
            //      ((FreeCamera)camera).position.Z.ToString() + ",",
            //        new Vector2(10, 10), Color.Black);
            //spriteBatch.End();

            spriteBatch.Begin();
            spriteBatch.DrawString(SFont, "Player Hits :  " +
                  PlayerTank.HitsCounter + "       " +
                  "Enemy Hits :  " +
                  EnemyTank.HitsCounter,
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

            TerrainDraw();

            //Arrow.Draw(camera.view, camera.projection); Utilizzata come debug per vedere il puntamento di vettori calcolati

            ParticleManager.Draw((FreeCamera)camera);
            base.Draw(gameTime);
        }

        public void TerrainDraw()                               //Generale griglia 3x3 dei terreni, il [4] è il terreno effettivo di gioco
        {
            TerrainList[0].Draw(camera, Effect, -127, 127);
            TerrainList[1].Draw(camera, Effect, 0, 127);
            TerrainList[2].Draw(camera, Effect, 127, 127);
            TerrainList[3].Draw(camera, Effect, -127, 0);
            TerrainList[4].Draw(camera, Effect);
            TerrainList[5].Draw(camera, Effect, 127, 0);
            TerrainList[6].Draw(camera, Effect, -127, -127);
            TerrainList[7].Draw(camera, Effect, 0, -127);
            TerrainList[8].Draw(camera, Effect, 127, -127);
        }

        public void PlayerTankControls(GameTime gameTime)
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


            if (KState.IsKeyDown(Keys.W))
            {
                PlayerTank.canonRot -= 0.01f;
                PlayerTank.BoneTransform(10, Matrix.CreateRotationX(PlayerTank.canonRot));
            }

            if (KState.IsKeyDown(Keys.S))
            {
                PlayerTank.canonRot += 0.01f;
                PlayerTank.BoneTransform(10, Matrix.CreateRotationX(PlayerTank.canonRot));
            }
            if (KState.IsKeyDown(Keys.A))
            {
                PlayerTank.turretRot += 0.01f;
                PlayerTank.BoneTransform(9, Matrix.CreateRotationY(PlayerTank.turretRot));
            }
            if (KState.IsKeyDown(Keys.D))
            {
                PlayerTank.turretRot -= 0.01f;
                PlayerTank.BoneTransform(9, Matrix.CreateRotationY(PlayerTank.turretRot));
            }

            if (KState.IsKeyDown(Keys.Left))
            {
                PlayerTank.steelRot += 0.015f;
                if (PlayerTank.steelRot > 0.5f)
                    PlayerTank.steelRot = 0.5f;

                PlayerTank.BoneTransform(3, Matrix.CreateRotationY(PlayerTank.steelRot));
                PlayerTank.BoneTransform(7, Matrix.CreateRotationY(PlayerTank.steelRot));
            }
            if (KState.IsKeyDown(Keys.Right))
            {
                PlayerTank.steelRot -= 0.015f;
                if (PlayerTank.steelRot < -0.5f)
                    PlayerTank.steelRot = -0.5f;
                PlayerTank.BoneTransform(3, Matrix.CreateRotationY(PlayerTank.steelRot));
                PlayerTank.BoneTransform(7, Matrix.CreateRotationY(PlayerTank.steelRot));

            }

            if (KState.IsKeyDown(Keys.Down))
            {
                if (CheckInclinationForMove(PlayerTank) || PlayerTank.enableBackward == true)
                {
                    if (CheckInclinationForMove(PlayerTank))
                        PlayerTank.enableBackward = false;
                    PlayerTank.BodyRot -= delta * PlayerTank.steelRot;

                    DoTankTransform(PlayerTank);

                    PlayerTank.wheelRot -= 0.05f;
                    PlayerTank.RotateWheels(PlayerTank.wheelRot);

                    PlayerTank.Position += PlayerTank.GetTankTranslation() - PlayerTank.GetTankDirection() * 0.01f;
                    PlayerTank.Position = new Vector3(PlayerTank.Position.X, TerrainList[4].GetHeight(PlayerTank.Position.X, PlayerTank.Position.Z), PlayerTank.Position.Z);
                }
                else
                    PlayerTank.enableForward = true;
            }

            if (KState.IsKeyDown(Keys.Up))
            {
                if (CheckInclinationForMove(PlayerTank) || PlayerTank.enableForward == true)
                {
                    if (CheckInclinationForMove(PlayerTank))
                        PlayerTank.enableForward = false;

                    PlayerTank.BodyRot += delta * PlayerTank.steelRot;

                    DoTankTransform(PlayerTank);

                    PlayerTank.wheelRot += 0.05f;
                    PlayerTank.RotateWheels(PlayerTank.wheelRot);

                    PlayerTank.Position += PlayerTank.GetTankTranslation() + PlayerTank.GetTankDirection() * 0.01f;
                    PlayerTank.Position = new Vector3(PlayerTank.Position.X, TerrainList[4].GetHeight(PlayerTank.Position.X, PlayerTank.Position.Z), PlayerTank.Position.Z);
                }
                else
                    PlayerTank.enableBackward = true;
            }

            if (KState.IsKeyDown(Keys.F))
            {
                if (PlayerTank.Bullet.IsFired == false)
                    SFXManager.Play("PlayerShot");

                PlayerTank.BulletFire();
            }
        }

        private void MakeExplosion(Bullet Bullet)  //Genera animazione 3D per esplosione proiettile
        {
            Vector3 impactPoint = new Vector3(Bullet.Position.X, TerrainList[4].GetHeight(Bullet.Position.X, Bullet.Position.Z), Bullet.Position.Z);
            ParticleManager.MakeExplosion(impactPoint, 50);
        }

        public void DoTankTransform(Tank tank)  //Calcola e compie le inclinazioni con il terreno
        {
            Vector3 tankAxis = CalculateTankDirection(tank); //Direzione del carro
            Vector3 tankZAxis = CalculateTankPerpDir(tank);  //Direzione laterale rispetto alla direzione del carro
            tankAxis.Normalize();
            tankZAxis.Normalize();

            tank.yinclination += NextInclination(tankAxis);  //Calcola l'angolo di inclinazione su Y
            tank.zinclination += ZInclination(tankZAxis);    //Calcola l'angolo di inclinazione su Z

            PlayerTank.BoneTransform(0, 
                Matrix.CreateRotationY(tank.BodyRot) *
                Matrix.CreateFromAxisAngle(tankAxis, tank.yinclination) *   //Applico rotazione e inclinazione)
                Matrix.CreateFromAxisAngle(tankZAxis, tank.zinclination));
        }

        private void UpdateBullet(Tank Shooter, GameTime gameTime)  //Gestisce la dinamica della collisione dei proiettili
        {
            if (Shooter.Bullet.IsFired == true)
            {
                if (Shooter.Bullet.Position.Y > TerrainList[4].GetHeight(Shooter.Bullet.Position.X, Shooter.Bullet.Position.Z)) //Controllo se il proiettile è in volo
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
                        if (Shooter != tank)    //Proiettile del nemico
                        {
                            MakeExplosion(Shooter.Bullet);
                            Shooter.Bullet.Position = Shooter.Position + Shooter.Bullet.BulletTranslation(Shooter) * new Vector3(0.01f, 0.01f, 0.01f) + Shooter.Bullet.bulletDirection * 0.04f;
                            SFXManager.Play("Explosion");
                            Shooter.HitsCounter++;
                            Shooter.Bullet.IsFired = false;
                            if (Shooter == PlayerTank)      //Gestione comparsa del carro nemico
                            {
                                int xCoord = random.Next(-5, 5);
                                int zCoord = random.Next(-5, 5);
                                if (xCoord < 2 && xCoord > -2)
                                    if (xCoord < 0)
                                        xCoord -= 2;
                                    else
                                        xCoord += 2;
                                if (zCoord < 2 && zCoord > -2)
                                    if (zCoord < 0)
                                        zCoord -= 2;
                                    else
                                        zCoord += 2;
                                tank.Position = PlayerTank.Position + new Vector3(xCoord, 0, zCoord);
                                bulletTimerCounter = 11000;
                                enemySpeedUpCnt = 0;
                            }
                            if(Shooter == EnemyTank)
                            {
                                bulletTimerCounter = 11000;
                                enemySpeedUpCnt = 0;
                            }
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
            ((FreeCamera)camera).RotateAuto(PlayerTank.BodyRot);

            Vector3 translation = PlayerTank.GetTankTranslation();

            ((FreeCamera)camera).position = CameraPosition(PlayerTank.GetTankDirection()) + PlayerTank.GetTankTranslation() + new Vector3(0f, 0.35f, 0f);
            ((FreeCamera)camera).Move(translation);
            ((FreeCamera)camera).Update();
            ParticleManager.Update(gameTime);

            LastMouseState = Mouse.GetState();
        }

        public void CheckBounds(Tank tank) //Verifico posizione del carro nei confini della mappa
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

        public bool CollisionCheck(Tank tank, Bullet bullet) //Gestisce collisione Proiettile-Carro
        {
            BoundingSphere tanksphere = tank.BoundingSphere;
            tanksphere = tanksphere.Transform(tank.baseworld);
            tanksphere.Center = tank.Position;
            tanksphere.Radius = 0.155f;

            BoundingSphere bulletsphere = bullet.BoundingSphere;
            bulletsphere = bulletsphere.Transform(tank.baseworld);
            bulletsphere.Center = bullet.Position;

            if (tanksphere.Intersects(bulletsphere))
                return true;
            else
                return false;
        }

        public void EnemyAutoFiring(GameTime gameTime, Tank Enemy) //Gestione AI carro nemico
        {
            Vector3 directionOfEnemyTurret = Enemy.Bullet.CalculateBulletDirection(Enemy);
            Vector3 directionOfFiring = (PlayerTank.Position - Enemy.Position);
            float distance = directionOfFiring.Length();

            MakeEnemyCanonRotation(Enemy, directionOfFiring, directionOfEnemyTurret, distance);

            if (AllowFire(directionOfEnemyTurret, directionOfFiring, distance, playerIsOnFire))
            {
                playerIsOnFire = true;

                Enemy.EnemyFiringDirection = PlayerTank.Position - Enemy.Position;
                if (Enemy.precEnemyFiringDirection == Vector3.Zero)
                    Enemy.precEnemyFiringDirection = Enemy.EnemyFiringDirection;

                MakeEnemyTurretRotation(Enemy, Enemy.EnemyFiringDirection, Enemy.precEnemyFiringDirection);

                if (!Enemy.Bullet.IsFired && AllowFire(directionOfEnemyTurret, directionOfFiring, distance, playerIsOnFire) && bulletTimer.IsTimeEspired(gameTime))
                {
                    Enemy.BulletFire();
                    enemySpeedUpCnt++;
                    if (enemySpeedUpCnt != (bulletTimerCounter/1000))
                        bulletTimerCounter = bulletTimerCounter - 1000;
                    else
                    {
                        bulletTimerCounter = 1000;
                        enemySpeedUpCnt = (bulletTimerCounter / 1000)-2;
                    }
                    bulletTimer = new TimeClass(bulletTimerCounter);
                    bulletTimer.Start();
                    SFXManager.Play("PlayerShot");
                }
                if (distance > 6f)
                {
                    playerIsOnFire = false;
                    Enemy.precEnemyFiringDirection = Vector3.Zero;
                }

                Enemy.precEnemyFiringDirection = Enemy.EnemyFiringDirection;
                }
        }

        public void MakeEnemyCanonRotation(Tank Enemy, Vector3 directionOfFiring, Vector3 directionOfEnemyTurret, float distance)
        {
            directionOfEnemyTurret.Normalize();
            directionOfFiring.Normalize();

            float heightDiff = PlayerTank.Position.Y - Enemy.Position.Y;
            float canonRotationAngle = (float)Math.Asin(heightDiff / distance);
            Enemy.BoneTransform(10, Matrix.CreateRotationX(-canonRotationAngle));
        }

        public void MakeEnemyTurretRotation(Tank Enemy, Vector3 directionOfFiring, Vector3 precDirectionOfFiring)
        {
            Vector2 EnemyFiringDirection2D = new Vector2(directionOfFiring.X, directionOfFiring.Z);
            Vector2 precEnemyFiringDirection2D = new Vector2(precDirectionOfFiring.X, precDirectionOfFiring.Z);

            float TurretRotationAngle = FindAngleBetweenTwoVectors(EnemyFiringDirection2D, precEnemyFiringDirection2D);
            Enemy.turretRot -= TurretRotationAngle;

            Enemy.BoneTransform(9, Matrix.CreateRotationY(Enemy.turretRot));
        }

        public bool AllowFire(Vector3 directionOfEnemyTurret, Vector3 directionOfFiring, float distance, bool playerIsOnFire)
        {
            directionOfEnemyTurret.Normalize();
            directionOfFiring.Normalize();

            if (directionOfEnemyTurret.X > directionOfFiring.X - 0.02f && directionOfEnemyTurret.X < directionOfFiring.X + 0.02f & directionOfEnemyTurret.Z > directionOfFiring.Z - 0.02f && directionOfEnemyTurret.Z < directionOfFiring.Z + 0.02f & distance < 6f || playerIsOnFire == true)
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

        public bool CheckInclinationForMove(Tank tank)
        {
            Vector3 tankDirection2 = CalculateTankDirection(tank);
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
            Vector3 tankDirectionNormalized = tankDirection * 0.14f;

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

        public float ZInclination(Vector3 tankDirection)
        {
            Vector3 tankDirectionNormalized = tankDirection * 0.025f;

            float tankRightHeight = TerrainList[4].GetHeight(PlayerTank.Position.X + tankDirectionNormalized.X, PlayerTank.Position.Z + tankDirectionNormalized.Z);
            Vector3 tankRightPos = new Vector3(PlayerTank.Position.X + tankDirectionNormalized.X, tankRightHeight, PlayerTank.Position.Z + tankDirectionNormalized.Z);
            Vector3 Rightheigtdir = (tankRightPos - PlayerTank.Position);
            Rightheigtdir.Normalize();

            float inclination = (float)Math.Acos(Vector3.Dot(Rightheigtdir, tankDirection));

            if (Rightheigtdir.Y > tankDirection.Y)
                inclination = inclination * -1;

            if (Double.IsNaN(inclination))
                inclination = 0f;

            return inclination;
        }

        public Vector3 CameraPosition(Vector3 tankDirection)
        {
            Vector3 tankDirectionNormalized = tankDirection * 0.5f;

            float tankBack = TerrainList[4].GetHeight(PlayerTank.Position.X - tankDirectionNormalized.X, PlayerTank.Position.Z - tankDirectionNormalized.Z);
            Vector3 tankBackPos = new Vector3(PlayerTank.Position.X - tankDirectionNormalized.X, tankBack, PlayerTank.Position.Z - tankDirectionNormalized.Z);

            return tankBackPos;
        }

        public Vector3 CalculateTankDirection(Tank tank)
        {
            Vector3 scale;
            Quaternion rotation;
            Vector3 translation;
            Matrix CanonRelTransform = new Matrix();
            CanonRelTransform = GetTransformPaths(tank.Model.Bones[0]);
            CanonRelTransform.Decompose(out scale, out rotation, out translation);
            return Vector3.Transform(Vector3.UnitZ, rotation);

        }

        public Vector3 CalculateTankPerpDir(Tank tank)
        {
            Vector3 scale;
            Quaternion rotation;
            Vector3 translation;
            Matrix CanonRelTransform = new Matrix();
            CanonRelTransform = GetTransformPaths(tank.Model.Bones[0]);
            CanonRelTransform.Decompose(out scale, out rotation, out translation);
            return Vector3.Transform(Vector3.UnitZ, rotation * Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathHelper.ToRadians(-90)));
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

        public void updateArrow()  //Funzione per debug direzione vettore
        {
            Vector3 scale;
            Quaternion rotation;
            Vector3 translation;
            Matrix ArrowTransform = new Matrix();
            ArrowTransform = GetTransformPaths(Arrow.Model.Bones[0]);
            ArrowTransform.Decompose(out scale, out rotation, out translation);
            Vector3 Arrow3dDir = Vector3.Transform(Vector3.UnitZ, rotation);
            Vector3 directionToPoint3d = Arrow.Position - EnemyTank.Position;

            Vector2 Arrow2dDir = new Vector2(Arrow3dDir.X, Arrow3dDir.Z);
            Vector2 directionToPoint2d = new Vector2(directionToPoint3d.X, directionToPoint3d.Z);

            float angle = FindAngleBetweenTwoVectors(Arrow2dDir, directionToPoint2d);
            Arrow.rotation -= angle;
            //Arrow.BoneTransform(0, Matrix.CreateRotationY(Arrow.rotation));
            Arrow.BoneTransform(0, Matrix.CreateFromAxisAngle(Vector3.UnitY, Arrow.rotation));

            Arrow.Position = PlayerTank.Position + new Vector3(0.05f, 0.19f, -0.055f);


        }
    }
}

