using System;
using System.Security.Policy;
using UnityEngine;

namespace AssemblyCSharp
{
    public class KnifeController : MonoBehaviour
    {
        //private PlayerKnifeController playerKnifeController;
        [HideInInspector]
        public Rigidbody rb;

        private WarpLookAheadCollider warpLookAheadCollider;

        private bool stuckInSurface;
        private GameObject objectStuck;
        private Vector3 collisionPositionOffset;

        private GravityPanel gravPanel;
        private Vector3 gravShiftVector;

        public const string ShowKnifeMarkerNotification = "KnifeController.ShowKnifeMarkerNotification";
        public const string KnifeBounceNotification = "KnifeController.KnifeBounceNotification";

        /*
         * Passes the knifecontroller and parameter spin speed to the knife
         */
        public virtual void Setup(PlayerKnifeController _controller, WarpLookAheadCollider _lookAhead)
        {
            //playerKnifeController = _controller;
            warpLookAheadCollider = _lookAhead;
            rb = GetComponent<Rigidbody>();

            stuckInSurface = false;

            gravShiftVector = Vector3.zero;

            //transform.LookAt(transform.position + _controller.transform.forward, _controller.transform.up); //? <-(why is this question mark here?)
        }

        //void FixedUpdate()
        //{
        //    //if (HasStuck() || rb == null)
        //    //    return;

        //    //if (rb.velocity != Vector3.zero)
        //    //    transform.forward = rb.velocity;

        //    //if (transform.rotation != GlobalGravityControl.GetGravityRotation())
        //    //    transform.rotation = GlobalGravityControl.GetGravityRotation();
        //}

        /*
         * Throws the knife at the given velocity
         */
        public virtual void Throw(Vector3 _velocity)
        {
            Debug.LogError("Throw method must be overridden");
        }

        public void AttachWarpCollider()
        {
            if (warpLookAheadCollider != null)
                warpLookAheadCollider.LockToKnife(gameObject);
        }

        /*
        * Sticks knife into surface when colliding with an object
        */
        public void StickToSurface(Vector3 _position, Vector3 _normal, GameObject _other)
        {
            // disable rigidbody
            //rb.detectCollisions = true;
            rb.isKinematic = true;

            stuckInSurface = true;
            collisionPositionOffset = _position - transform.position;
            transform.rotation = Quaternion.FromToRotation(Vector3.up, _normal);
            //transform.up = _normal;

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
                gravShiftVector = gravPanel.GetGravityVector();
            }

            // activate knife marker ui
            Info<Transform, bool> info = new Info<Transform, bool>(transform, gravPanel != null);
            this.PostNotification(ShowKnifeMarkerNotification, info);
        }

        public virtual Vector3 GetPosition()
        {
            return transform.position;
        }

        public virtual Vector3 GetCollisionPosition()
        {
            return transform.position + collisionPositionOffset;
        }

        public virtual Vector3 GetCollisionNormal()
        {
            if (!HasStuck())
                return Vector3.zero;

            return transform.up;
        }

        /*
         * Returns position player will warp to
         * 
         * this eventually needs to be converted to the closest place
         * for the player collider to move
         */
        public virtual Vector3 GetWarpPosition()
        {
            return warpLookAheadCollider != null ? warpLookAheadCollider.WarpPosition() : GetWarpTestPosition();
        }

        /*
         * Gives the position needed to be tested by the warp lookahead collider
         */
        public virtual Vector3 GetWarpTestPosition()
        {
            return transform.position + (transform.up * 0.5f); // need to change this depending on gravity angle to match distance to edge of collider
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
            return gravShiftVector;
        }

        public virtual bool HasStuck()
        {
            return stuckInSurface;
        }

        public virtual bool IsBounceKnife()
        {
            return false;
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

