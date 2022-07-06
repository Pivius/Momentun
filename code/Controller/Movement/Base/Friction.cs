using Sandbox;
using System;

namespace TrickHop.Movement
{
	public partial class Friction : Accelerate
	{
		// # Source Movement Friction

		public virtual Vector3 ApplyFriction( Vector3 velocity,
										float friction,
										float stopSpeed )
		{
			float speed = velocity.Length;
			float control = MathF.Max( speed, stopSpeed );
			float drop = control * friction * Time.Delta;
			float newSpeed = MathF.Max( speed - drop, 0 );

			if ( newSpeed != speed )
			{
				newSpeed /= speed;
				velocity *= newSpeed;
			}

			return velocity;
		}

		public virtual void Move( ref Vector3 velocity, float friction, float stopSpeed )
		{
			velocity = ApplyFriction( velocity, friction, stopSpeed );
		}
	}
}
