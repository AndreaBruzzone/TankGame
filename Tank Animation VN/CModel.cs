﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TankAnimationVN
{
    public class CModel
    {
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        public Vector3 Scale { get; set; }
        public Model Model { get; private set; }
        public Matrix[] modelTransforms;
        public Matrix[] originalTransforms;
        public Matrix baseworld;
        public float rotation;
        private BoundingSphere boundingSphere;
        public CModel(Model Model, Vector3 Position, Quaternion Rotation,
                       Vector3 Scale, GraphicsDevice graphicsDevice)
        {
            this.Model = Model;
            modelTransforms = new Matrix[Model.Bones.Count];
            originalTransforms = new Matrix[Model.Bones.Count];

            Model.CopyBoneTransformsTo(originalTransforms);

            this.Position = Position;
            this.Rotation = Rotation;
            this.Scale = Scale;
            rotation = 0f;
        }



        public BoundingSphere BoundingSphere
        {
            get
            {
                // No need for rotation, as this is a sphere
                Matrix worldTransform = Matrix.CreateScale(Scale)
                    * Matrix.CreateTranslation(Position);

                BoundingSphere sphere = new BoundingSphere();

                foreach (ModelMesh mesh in Model.Meshes)
                {
                    if (sphere.Radius == 0)
                        sphere = mesh.BoundingSphere;
                    else
                        sphere = BoundingSphere.CreateMerged(sphere, mesh.BoundingSphere);
                }

                return sphere;

            }
        }

        public void Draw(Matrix View, Matrix Projection)
        {
            baseworld = Matrix.CreateScale(Scale) *
                               Matrix.CreateFromQuaternion(Rotation) *
                               Matrix.CreateTranslation(Position);

            Model.CopyAbsoluteBoneTransformsTo(modelTransforms);
            foreach (ModelMesh mesh in Model.Meshes)
            {
                Matrix localWorld = modelTransforms[mesh.ParentBone.Index] * baseworld;
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    BasicEffect effect = (BasicEffect)meshPart.Effect;
                    effect.World = localWorld;
                    effect.View = View;
                    effect.Projection = Projection;

                    effect.EnableDefaultLighting();
                }
                mesh.Draw();
            }

        }


        private void buildBoundingSphere()
        {
            BoundingSphere sphere = new BoundingSphere(Vector3.Zero, 0);

            // Merge all the model's built in bounding spheres
            foreach (ModelMesh mesh in Model.Meshes)
            {
                BoundingSphere transformed = mesh.BoundingSphere.Transform(
                    modelTransforms[mesh.ParentBone.Index]);

                sphere = BoundingSphere.CreateMerged(sphere, transformed);
            }

            this.boundingSphere = sphere;
        }


        public void BoneTransform(int BoneIndex, Matrix MatTransform)
        {
            Model.Bones[BoneIndex].Transform = MatTransform * originalTransforms[BoneIndex];
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

