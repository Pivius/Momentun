namespace Momentum
{
	public partial class Accelerate : AirAccelerate
	{
		// Source Movement Accelerate

		public override float GetVelDiff( Vector3 velocity, float length, Vector3 strafeDir )
		{
			return length - velocity.Dot( strafeDir );
		}

		public override void Move( ref Vector3 velocity, Vector3 strafeVel, float maxSpeed, float accel )
		{
			velocity = GetFinalVelocity( velocity, strafeVel, maxSpeed, accel );
		}
	}
}
