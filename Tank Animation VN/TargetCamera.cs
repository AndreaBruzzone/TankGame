using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TankAnimationVN
{
    public class TargetCamera : Camera
    {
        public Vector3 position { get; set; }
        public Vector3 target { get; set; }
        public TargetCamera( Vector3 position, Vector3 target, GraphicsDevice GD) : base (GD)
        {
            this.position = position;
            this.target = target;
        }
        public override void Update()
        {
            Vector3 forward = target - position;
            Vector3 side = Vector3.Cross(forward, Vector3.Up);
            Vector3 up = Vector3.Cross(side, forward);
            this.view = Matrix.CreateLookAt(position, target, up);
        }
    }
}
