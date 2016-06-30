using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TankAnimationVN
{
    class Particle
    {

        #region Static Fields
        public static GraphicsDevice GraphicsDevice;
        private static Vector3 Gravity = new Vector3(0, -5, 0);
        private static VertexBuffer vertexBuffer;
        private static IndexBuffer indexBuffer;
        #endregion

        #region Instance Fields
        private Vector3 position;
        private Vector3 velocity;
        public float duration;
        private float initialDuration;
        private float scale;
        #endregion

        #region Properties
        public bool IsActive { get { return (duration > 0); } }
        #endregion

        #region Constructor
        public Particle()
        {
            if (vertexBuffer == null)
            {
                InitializeParticles();
            }
        }
        #endregion

        #region Static Methods
        public static void InitializeParticles()
        {
            VertexPositionTexture[] vertices = new VertexPositionTexture[4];

            vertices[0] = new VertexPositionTexture(
                new Vector3(0, 1, 0), new Vector2(0, 0));
            vertices[1] = new VertexPositionTexture(
                new Vector3(1, 1, 0), new Vector2(1, 0));
            vertices[2] = new VertexPositionTexture(
                new Vector3(0, 0, 0), new Vector2(0, 1));
            vertices[3] = new VertexPositionTexture(
                new Vector3(1, 0, 0), new Vector2(1, 1));

            vertexBuffer = new VertexBuffer(
                GraphicsDevice,
                typeof(VertexPositionTexture),
                vertices.Length,
                BufferUsage.WriteOnly);

            vertexBuffer.SetData(vertices);

            int indexCount = 6;
            short[] indices = new short[indexCount];

            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;
            indices[3] = 2;
            indices[4] = 1;
            indices[5] = 3;

            indexBuffer = new IndexBuffer(
                GraphicsDevice,
                IndexElementSize.SixteenBits,
                indices.Length,
                BufferUsage.WriteOnly);
            indexBuffer.SetData(indices);
        }
        #endregion

        #region Particle Activation
        public void Activate(
            Vector3 position,
            Vector3 velocity,
            float duration,
            float scale)
        {
            this.duration = duration;
            initialDuration = duration;
            this.position = position;
            this.velocity = velocity;
            this.scale = scale;
        }
        #endregion

        #region Update
        public void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            duration -= elapsed;
            velocity += (Gravity * elapsed);
            position += (velocity * elapsed);
        }
        #endregion

        #region Draw
        public void Draw(FreeCamera camera, Effect effect)
        {
            Matrix billboard = Matrix.CreateBillboard(
                position,
                camera.position,
                Vector3.Up,
                null);

            effect.Parameters["World"].SetValue(
                Matrix.CreateScale(scale) * billboard);

            effect.Parameters["alphaValue"].SetValue(
                duration / initialDuration);

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.SetVertexBuffer(vertexBuffer);
                GraphicsDevice.Indices = indexBuffer;
                GraphicsDevice.DrawIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    0,
                    0,
                    6,
                    0,
                    2);
            }
        }
        #endregion

    }
}
