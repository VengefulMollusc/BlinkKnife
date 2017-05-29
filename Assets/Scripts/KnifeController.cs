using System;
using UnityEngine;

namespace AssemblyCSharp
{
	public interface KnifeController
	{
        /*
         * Passes the knifecontroller and parameter spin speed to the knife
         */
		void Setup (PlayerKnifeController _controller, Vector3 _gravityDir, float _spinSpeed);

        /*
         * Throws the knife at the given velocity
         */
		void Throw (Vector3 _velocity);

		Vector3 GetPosition();

        /*
         * Returns position player will warp to
         * 
         * this eventually needs to be converted to the closest place
         * for the player collider to move
         */
		Vector3 GetWarpPosition();

        /*
         * Gets the current velocity of the knife
         * 
         * bool determines whether to return the velocity at point of throw
         */
		Vector3 GetVelocity(bool throwVelocity);

        /*
         * Returns the object the knife collided with (if existing)
         */
		GameObject GetObjectCollided();

        Vector3 GetGravVector();

		bool HasCollided();

        bool ShiftGravity();
	}
}

