using System;
using UnityEngine;

namespace AssemblyCSharp
{
    public class KnifeController : MonoBehaviour
    {
        private PlayerKnifeController playerKnifeController;
        [HideInInspector]
        public Rigidbody rb;

        private bool stuckInSurface;
        private GameObject objectStuck;
        private Vector3 collisionNormal;

        private GravityPanel gravPanel;

        public const string ShowKnifeMarkerNotification = "KnifeController.ShowKnifeMarkerNotification";
        public const string KnifeBounceNotification = "KnifeController.KnifeBounceNotification";

        /*
         * Passes the knifecontroller and parameter spin speed to the knife
         */
        public virtual void Setup(PlayerKnifeController _controller)
        {
            playerKnifeController = _controller;
            rb = GetComponent<Rigidbody>();

            stuckInSurface = false;
            collisionNormal = Vector3.zero;

            transform.LookAt(transform.position + _controller.transform.forward, _controller.transform.up); //? <-(why is this question mark here?)
        }

        void FixedUpdate()
        {
            if (HasStuck() || rb == null)
                return;

            if (rb.velocity != Vector3.zero)
                transform.forward = rb.velocity;
        }

        /*
         * Throws the knife at the given velocity
         */
        public virtual void Throw(Vector3 _velocity)
        {
            Debug.LogError("Throw method must be overridden");
        }

        /*
        * Sticks knife into surface when colliding with an object
        */
        public void StickToSurface(Vector3 _normal, GameObject _other)
        {
            // disable rigidbody
            rb.detectCollisions = false;
            rb.isKinematic = true;

            stuckInSurface = true;
            collisionNormal = _normal;

            // stick knife out of surface at collision point
            rb.velocity = Vector3.zero;
            //visuals.transform.forward = transform.forward; // obsolete as knife no longer spinning

            // parent knife to other gameobject (to handle moving objects)
            transform.SetParent(_other.transform);
            objectStuck = _other;

            // Prepare to shift gravity if warping to GravityPanel
            if (objectStuck.GetComponent<GravityPanel>() != null)
            {
                gravPanel = objectStuck.GetComponent<GravityPanel>();
                Vector3 gravVector = gravPanel.GetGravityVector();
                if (gravVector != Vector3.zero)
                    collisionNormal = -gravVector;
            }

            // activate knife marker ui
            Info<Transform, bool> info = new Info<Transform, bool>(transform, gravPanel != null);
            this.PostNotification(ShowKnifeMarkerNotification, info);
        }

        public virtual Vector3 GetPosition()
        {
            return transform.position;
        }

        /*
         * Returns position player will warp to
         * 
         * this eventually needs to be converted to the closest place
         * for the player collider to move
         */
        public virtual Vector3 GetWarpPosition()
        {
            return transform.position + (collisionNormal * 0.5f);
        }

        /*
         * Gets the current velocity of the knife
         * 
         * bool determines whether to return the velocity at point of throw
         */
        public virtual Vector3 GetVelocity()
        {
            return rb.velocity;
        }

        /*
         * Returns the object the knife collided with (if existing)
         */
        public virtual GameObject GetStuckObject()
        {
            return objectStuck;
        }

        public virtual Vector3 GetGravVector()
        {
            return -collisionNormal;
        }

        public virtual bool HasStuck()
        {
            return stuckInSurface;
        }

        /*
         * whether to shift gravity on warp
         */
        public virtual bool ShiftGravity()
        {
            return (stuckInSurface && gravPanel != null);
        }
    }
}

