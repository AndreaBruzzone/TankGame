using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TankAnimationVN
{
    public abstract class Camera
    {
        public Matrix view { get; set; }
        public Matrix projection { get; set; }
        protected GraphicsDevice graphicsDevice { get; set; }
        public Camera(GraphicsDevice GD)
        {
            graphicsDevice = GD;
            generatePerspectiveProjectionMatrix(MathHelper.PiOver4);
        }

        private void generatePerspectiveProjectionMatrix(float fieldOfView)
        {
            PresentationParameters pp = graphicsDevice.PresentationParameters;
            float aspectRatio = (float)pp.BackBufferWidth / (float)pp.BackBufferHeight;
            projection = Matrix.CreatePerspectiveFieldOfView(fieldOfView, 
                                                            aspectRatio, 0.1f, 10000.0f);
        }
        public virtual void Update()
        {

        }

    }
}
