using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TankAnimationVN
{
    public class FreeCamera : Camera
    {
        public float yaw { get; set; }
        public float pitch { get; set; }
        public Vector3 position { get; set; }
        public Vector3 target { get; set; }
        private Vector3 translation;

        public FreeCamera(Vector3 Position, float Yaw, float Pitch, GraphicsDevice GD) : base(GD)
        {
            position = Position;
            yaw = Yaw;
            pitch = Pitch;
            translation = Vector3.Zero;
        }
        public void Rotate(float YawChange, float PitchChange)
        {
            yaw += YawChange;
            pitch += PitchChange;
        }

        public void Move(Vector3 Translation)
        {
            translation += Translation;
        }

        public override void Update()
        {
            Matrix rotation = Matrix.CreateFromYawPitchRoll(yaw, pitch, 0);

            translation = Vector3.Transform(translation, rotation);
            position += translation;
            translation = Vector3.Zero;

            Vector3 forward = Vector3.Transform(Vector3.Forward, rotation);
            target = position + forward;
            Vector3 up = Vector3.Transform(Vector3.Up, rotation);
            view = Matrix.CreateLookAt(position, target, up);

        }

    }
}
