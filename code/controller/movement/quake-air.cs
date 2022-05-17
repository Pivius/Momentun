using Sandbox;

namespace Momentum
{
	public class QuakeAirAccelerate : AirAccelerate
	{
		// # Vanilla Quake 3 Air Accel

		public override float GetVelDiff( Vector3 velocity, float length, Vector3 strafeDir )
		{
			return length - velocity.Dot( strafeDir );
		}

		public virtual float AdjustAccel( float dotVel,
									float accel,
									float strafeAccel,
									float airStopAccel )
		{
			if ( dotVel < 0.0f || (Input.Forward != 0.0f && Input.Left == 0) )
				accel = airStopAccel;

			if ( Input.Left != 0 && Input.Forward == 0 )
				accel = strafeAccel;

			return accel;
		}

		public virtual Vector3 GetFinalVelocity( Vector3 velocity,
											Vector3 strafeVel,
											float strafeVelLength,
											float accel,
											float strafeAccel,
											float airStopAccel )
		{
			Vector3 strafeDir = strafeVel.Normal;
			float velDiff = GetVelDiff( velocity, strafeVelLength, strafeDir );
			Vector3 accelSpeed = GetAccelSpeed( strafeDir,
										strafeVelLength,
										velDiff,
										AdjustAccel( velocity.Dot( strafeDir ), accel, strafeAccel, airStopAccel ) );

			return velocity + accelSpeed;
		}

		public virtual Vector3 AirControl( Vector3 velocity, Vector3 strafeDir, float airControl )
		{
			if ( Input.Forward >= 1 && Input.Left == 0 )
			{
				float velLength;
				float velDot;
				float velZ = velocity.z;
				float airControlSpeed = 32f;

				velocity.z = 0;
				velLength = velocity.Length;
				velocity = velocity.Normal;
				velDot = velocity.Dot( strafeDir );
				airControlSpeed *= airControl * velDot * velDot * Time.Delta;

				if ( velDot > 0 )
					velocity = ((velocity * velLength) + (strafeDir * airControlSpeed)).Normal;

				velocity *= velLength;
				velocity.z = velZ;
			}

			return velocity;
		}

		public override void Move( ref Vector3 velocity,
							Vector3 strafeVel,
							float maxSpeed,
							float sideStrafeMaxSpeed,
							float accel,
							float strafeAccel,
							float airStop,
							float airControl )
		{
			float strafeVelLength = CapWishSpeed( strafeVel.Length, maxSpeed );

			if ( Input.Left != 0 && Input.Forward == 0 )
				strafeVelLength = CapWishSpeed( strafeVel.Length, sideStrafeMaxSpeed );

			velocity = GetFinalVelocity( velocity, strafeVel, strafeVelLength, accel, strafeAccel, airStop );

			if ( airControl > 0 )
				velocity = AirControl( velocity, strafeVel.Normal, airControl );
		}
	}
}
