using Sandbox;

namespace Momentum
{
	public partial class AirAccelerate : BaseNetworkable
	{
		// # Source Movement Air Accel

		public virtual float CapWishSpeed( float wishSpeed, float maxSpeed )
		{
			return MathX.Clamp( wishSpeed, 0, maxSpeed );
		}

		public virtual float GetVelDiff( Vector3 velocity, float length, Vector3 strafeDir )
		{
			return (length / 10.0f) - velocity.Dot( strafeDir );
		}

		public virtual Vector3 GetAccelSpeed( Vector3 strafeDir,
										float length,
										float velDiff,
										float accel )
		{
			return strafeDir * MathX.Clamp( length * accel * Time.Delta, 0, velDiff );
		}

		public virtual Vector3 GetFinalVelocity( Vector3 velocity,
											Vector3 strafeVel,
											float maxSpeed,
											float accel )
		{
			Vector3 strafeDir = strafeVel.Normal;
			float strafeVelLength = CapWishSpeed( strafeVel.Length, maxSpeed );
			float velDiff = GetVelDiff( velocity, strafeVelLength, strafeDir );
			Vector3 accelSpeed = GetAccelSpeed( strafeDir, strafeVelLength, velDiff, accel );

			return velocity + accelSpeed;
		}

		public virtual void Move( ref Vector3 velocity,
							Vector3 strafeVel,
							float maxSpeed,
							float accel )
		{
			velocity = GetFinalVelocity( velocity, strafeVel, maxSpeed, accel );
		}
		public virtual void Move( ref Vector3 velocity,
							Vector3 strafeVel,
							float maxSpeed,
							float sideStrafeMaxSpeed,
							float accel,
							float strafeAccelerate,
							float airStop,
							float airControl )
		{
		}
	}
}
