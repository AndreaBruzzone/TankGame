using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TankAnimationVN
{
    static class ParticleManager
    {

        #region Fields
        private static GraphicsDevice graphicsDevice;
        private static List<Particle> particles = new List<Particle>();
        private static Effect particleEffect;
        private static Texture2D particleTexture;
        private static Random rand = new Random();
        #endregion

        #region Initialization
        public static void Initialize(
            GraphicsDevice device,
            Effect effect,
            Texture2D texture)
        {
            graphicsDevice = device;
            particleEffect = effect;
            particleTexture = texture;
            Particle.GraphicsDevice = device;
            for (int x = 0; x < 300; x++)
            {
                particles.Add(new Particle());
            }
        }
        #endregion

        #region Particle Creation
        public static void AddParticle(
            Vector3 position,
            Vector3 velocity,
            float duration,
            float scale)
        {
            for (int x = 0; x < particles.Count; x++)
            {
                if (!particles[x].IsActive)
                {
                    particles[x].Activate(position, velocity, duration, scale);
                    return;
                }
            }
        }
        #endregion

        #region Update
        public static void Update(GameTime gameTime)
        {
            foreach (Particle particle in particles)
            {
                if (particle.IsActive)
                    particle.Update(gameTime);
            }
        }
        #endregion

        #region Draw
        public static void Draw(FreeCamera camera)
        {
            graphicsDevice.BlendState = BlendState.Additive;

            particleEffect.CurrentTechnique =
                particleEffect.Techniques["ParticleTechnique"];

            particleEffect.Parameters["particleTexture"].SetValue(
                particleTexture);

            particleEffect.Parameters["View"].SetValue(camera.view);

            particleEffect.Parameters["Projection"].SetValue(
                camera.projection);

            graphicsDevice.RasterizerState = RasterizerState.CullNone;
            graphicsDevice.BlendState = BlendState.Additive;
            graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;

            foreach (Particle particle in particles)
            {
                if (particle.IsActive)
                    particle.Draw(camera, particleEffect);
            }
            graphicsDevice.RasterizerState =
               RasterizerState.CullCounterClockwise;
            graphicsDevice.BlendState = BlendState.Opaque;
            graphicsDevice.DepthStencilState = DepthStencilState.Default;
        }
        #endregion

        #region Helper Methods
        public static void MakeExplosion(Vector3 position, int particleCount)
        {
            for (int i = 0; i < particleCount; i++)
            {
                float duration = (float)(rand.Next(0, 5)) / 1000f + 2;
                float x = ((float)(rand.Next(0, 5)) - 0.2f) * 1.5f;
                float y = ((float)(rand.Next(0, 5)) - 0.2f) * 1.5f;
                float z = ((float)(rand.Next(0, 5)) - 0.2f) * 1.5f;
                float s = (float)(rand.Next(0, 5)) + 0.0f;
                Vector3 direction = Vector3.Normalize(
                    new Vector3(x, y, z)) *
                    ((1 * 3f) + 2f);

                AddParticle(
                    position,
                    direction,
                    0.4f, 0.3f);
            }
        }
        #endregion

    }
}
