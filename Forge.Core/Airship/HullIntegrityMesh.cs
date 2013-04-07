#region

using System;
using Forge.Core.Logic;
using Forge.Framework.Draw;
using Microsoft.Xna.Framework;

#endregion

namespace Forge.Core.Airship{
    internal class HullIntegrityMesh{
        //-creates overlay mesh for damaged portions
        //-removes hull portions if damage exceeds limits
        //-generates physics block for deflected blows
        //acts as the maanger of the boundingobject for physics
        //generates boundingobject spheres

        ProjectilePhysics.BoundingObject _boundingObject;
        Func<HullSection, bool> _disableHullSection;
        Func<HullSection, bool> _enableHullSection;
        ObjectBuffer<HullSection> _hullDamageOverlay;

        public HullIntegrityMesh(HullSection[][] hull, Func<HullSection, bool> enableHullSection, Func<HullSection, bool> disableHullSection){
            //create boundingobject

            //hulldamageoverlay eats vertexes from airship.cs
            //dont forget to resize+offset to make it slightly bigger

            throw new NotImplementedException();
        }

        public Matrix WorldMatrix { get; set; } //propagate to boundingobject

        void OnCollision(Vector3 position, Vector3 velocity){
            throw new NotImplementedException();
        }

        void UpdateDamageTexture(Vector3 position){
            //make sure position is unwrapped and is not in world coordinates
            throw new NotImplementedException();
        }
    }
}