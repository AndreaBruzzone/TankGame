using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

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

        Texture2D[] skyboxTextures;
        Model skyboxModel;

        CModel Cmodel, BulletModel, ShTarget1, ShTarget2, ShTarget3, CPlane;
        Camera camera;
        MouseState LastMouseState;
        bool FirstRun = true;
        TimeClass BulletTime;
        BasicEffect beffect;

        bool BulletFired = false;
        Vector3 forward = new Vector3(0, 0, 0);

        float canonRot = 0;
        float turretRot = 0;
        float wheelRot = 0;
        float steelRot = 0;
        float BodyRot = 0;

        bool obstacleForward = false;
        bool obstacleBackward = false;

        Terrain terrain;
        Effect effect;

        Matrix CanonRelTransform, CmodelTransform;
        Vector3 bulletForward;

        //forze
        Vector3 vel = new Vector3(0, 0, 0);

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            //graphics.PreparingDeviceSettings += new EventHandler<PreparingDeviceSettingsEventArgs>(graphics_PreparingDeviceSettings);
            Content.RootDirectory = "Content";
        }
        //void graphics_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        //{
        //    e.GraphicsDeviceInformation.GraphicsProfile = GraphicsProfile.HiDef;
        //}

        protected override void Initialize()
        {
            BulletTime = new TimeClass(1);

            base.Initialize();
        }


        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            effect = Content.Load<Effect>("terrain");   

            this.IsMouseVisible = true;

            SFont = Content.Load<SpriteFont>("SpriteFont");
               
            terrain = new Terrain(GraphicsDevice, Content.Load<Texture2D>("heightmap"), Content.Load<Texture2D>("erba"), Content.Load<Texture2D>("muschio"), Content.Load<Texture2D>("vetta"), 1f, 128, 128, 15f);

            Cmodel = new CModel(Content.Load<Model>("tank"), new Vector3(21, terrain.GetHeight(21, 61), 61),
                                new Quaternion(), new Vector3(0.01f, 0.01f, 0.01f), GraphicsDevice);
            var BulletTranslation = GetTransformPaths(Cmodel.Model.Bones[10]);
            BulletModel = new CModel(Content.Load<Model>("Bullet"), BulletTranslation.Translation,
                                 new Quaternion(), new Vector3(0.01f, 0.01f, 0.01f), GraphicsDevice);

            camera = new FreeCamera(new Vector3(11, terrain.GetHeight(11, 61)+2, 61), -20f, 0f, GraphicsDevice);

            LastMouseState = Mouse.GetState();
        }

        private void DrawSkybox()
        {
            SamplerState ss = new SamplerState();
            ss.AddressU = TextureAddressMode.Clamp;
            ss.AddressV = TextureAddressMode.Clamp;
            GraphicsDevice.SamplerStates[0] = ss;
            
            DepthStencilState dss = new DepthStencilState();
            dss.DepthBufferEnable = false;
            GraphicsDevice.DepthStencilState = dss;

            Matrix[] skyboxTransforms = new Matrix[skyboxModel.Bones.Count];
            skyboxModel.CopyAbsoluteBoneTransformsTo(skyboxTransforms);
            int i = 0;
            foreach (ModelMesh mesh in skyboxModel.Meshes)
            {
                foreach (Effect currentEffect in mesh.Effects)
                {
                    Matrix worldMatrix = skyboxTransforms[mesh.ParentBone.Index] * Matrix.CreateTranslation(Cmodel.Position);
                    currentEffect.CurrentTechnique = currentEffect.Techniques["Textured"];
                    currentEffect.Parameters["xWorld"].SetValue(worldMatrix);
                    currentEffect.Parameters["xView"].SetValue(camera.view);
                    currentEffect.Parameters["xProjection"].SetValue(camera.projection);
                    currentEffect.Parameters["xTexture"].SetValue(skyboxTextures[i++]);
                }
                mesh.Draw();
            }

            dss = new DepthStencilState();
            dss.DepthBufferEnable = true;
            GraphicsDevice.DepthStencilState = dss;
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

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

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
            if (!obstacleForward & !obstacleBackward)
            {
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
            }
            if (KState.IsKeyDown(Keys.Down))
            {
                if(!obstacleBackward)
                {
                    obstacleForward = false;
                    BodyRot -= delta * steelRot;
                    Cmodel.BoneTransform(0, Matrix.CreateRotationY(BodyRot));
                    CmodelTransform = GetTransformPaths(Cmodel.Model.Bones[0]);
                    Vector3 scale;
                    Quaternion rotation;
                    Vector3 translation;
                    CmodelTransform.Decompose(out scale, out rotation, out translation);
                    Vector3 CmodelForward = Vector3.Transform(Vector3.UnitZ, rotation);

                    Vector3 newPos = Cmodel.Position + translation - CmodelForward * 0.03f;

                    if (Math.Abs((terrain.GetHeight(Cmodel.Position.X - 3f, Cmodel.Position.Z - 3f)) - newPos.Y) <= 1000000f)
                    {
                        wheelRot -= 0.05f;
                        Cmodel.BoneTransform(2, Matrix.CreateRotationX(wheelRot));
                        Cmodel.BoneTransform(4, Matrix.CreateRotationX(wheelRot));
                        Cmodel.BoneTransform(6, Matrix.CreateRotationX(wheelRot));
                        Cmodel.BoneTransform(8, Matrix.CreateRotationX(wheelRot));
                        Cmodel.Position += translation - CmodelForward * 0.03f;
                        Cmodel.Position = new Vector3(Cmodel.Position.X, terrain.GetHeight(Cmodel.Position.X, Cmodel.Position.Z), Cmodel.Position.Z);              
                    }
                    else
                        obstacleBackward = true;
                }
         
            }
            if (KState.IsKeyDown(Keys.Up))
            {

                if (!obstacleForward)
                {
                    obstacleBackward = false;

                    BodyRot += delta * steelRot;
                    Cmodel.BoneTransform(0, Matrix.CreateRotationY(BodyRot));
                    CmodelTransform = GetTransformPaths(Cmodel.Model.Bones[0]);
                    Vector3 scale;
                    Quaternion rotation;
                    Vector3 translation;
                    CmodelTransform.Decompose(out scale, out rotation, out translation);
                    Vector3 CmodelForward = Vector3.Transform(Vector3.UnitZ, rotation);

                    Vector3 newPos = Cmodel.Position + translation + CmodelForward * 0.03f;

                    if (Math.Abs((terrain.GetHeight(Cmodel.Position.X + 3f, Cmodel.Position.Z + 3f)) - newPos.Y) <= 100000000f)
                    {
                        wheelRot += 0.05f;
                        Cmodel.BoneTransform(2, Matrix.CreateRotationX(wheelRot));
                        Cmodel.BoneTransform(4, Matrix.CreateRotationX(wheelRot));
                        Cmodel.BoneTransform(6, Matrix.CreateRotationX(wheelRot));
                        Cmodel.BoneTransform(8, Matrix.CreateRotationX(wheelRot));
                        Cmodel.Position += translation + CmodelForward * 0.03f;
                        Cmodel.Position = new Vector3(Cmodel.Position.X, terrain.GetHeight(Cmodel.Position.X, Cmodel.Position.Z), Cmodel.Position.Z);
                    }
                    else
                        obstacleForward = true;
                }      
            }


            if (KState.IsKeyDown(Keys.F))
            {
                BulletFired = true;

                Vector3 scale;
                Quaternion rotation;
                Vector3 translation;
                CanonRelTransform = GetTransformPaths(Cmodel.Model.Bones[10]);
                CanonRelTransform.Decompose(out scale, out rotation, out translation);

                bulletForward = Vector3.Transform(Vector3.UnitZ, rotation);
                
                BulletModel.Position = Cmodel.Position + new Vector3(0f, 1f, 0f);
                BulletModel.Rotation = rotation;

                BulletTime.Start();



            }
            if (BulletFired)
            {
                if (BulletTime.IsTimeEspired(gameTime))
                {
                    BulletModel.Position += bulletForward * 0.2f - 0.005f * Vector3.UnitY ;
                }
            }

            updateCamera(gameTime);

            base.Update(gameTime);
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
            //if (keyState.IsKeyDown(Keys.W)) translation += Vector3.Forward;
            //if (keyState.IsKeyDown(Keys.S)) translation += Vector3.Backward;
            //if (keyState.IsKeyDown(Keys.A)) translation += Vector3.Left;
            //if (keyState.IsKeyDown(Keys.D)) translation += Vector3.Right;

            ((FreeCamera)camera).position = Cmodel.Position + new Vector3(-4f, 3.5f, 0f);
            ((FreeCamera)camera).Move(translation);
           

            camera.Update();

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

            if (BulletFired)
            {
                BulletModel.Draw(camera.view, camera.projection);
            }

            Cmodel.Draw(camera.view, camera.projection);

            terrain.Draw(camera, effect);

            base.Draw(gameTime);
        }
    }
}
