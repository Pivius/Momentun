using Sandbox;

namespace Momentum
{
	public partial class Water : Accelerate
	{
		// # Source Water Movement

		[Net, Predicted] public float JumpTime { get; set; }
		[Net, Predicted] public Vector3 JumpVel { get; set; }
		[Net, Predicted] public float EntryTime { get; set; }
		public float MaxJumpLedge { get; private set; } = 8.0f;
		public float SinkSpeed { get; private set; } = 60.0f;
		public float JumpHeight { get; set; } = 256.0f;
		public WATERLEVEL WaterLevel { get; set; } = 0;
		public WATERLEVEL OldWaterLevel { get; set; } = 0;

		public virtual Vector3 CheckWaterJump( Vector3 velocity,
										Vector3 position,
										BaseController controller )
		{
			if ( JumpTime == 0 ) // Already water jumping.
			{
				if ( velocity.z >= -180.0f ) // only hop out if we are moving up
				{
					Vector3 forward = Input.Rotation.Forward;
					Vector3 viewDir = forward.WithZ( 0 ).Normal;
					Vector3 flatVelocity = velocity.WithZ( 0 );

					// Are we backing into water from steps or something? If so, don't pop forward
					if ( flatVelocity.Length != 0.0f && (flatVelocity.Dot( viewDir ) >= 0.0f) )
					{
						// Start line trace at waist height (using the center of the player for this here)
						var traceStart = position + (controller.GetPlayerMins() + controller.GetPlayerMaxs()) * 0.5f;
						var traceEnd = traceStart + (viewDir * 24.0f);
						var trace = TraceUtil.PlayerBBox( traceStart, traceEnd, controller );

						if ( trace.Fraction < 1.0f ) // solid at waist
						{
							traceStart = traceStart.WithZ( position.z + controller.ViewOffset + MaxJumpLedge );
							traceEnd = traceStart + (viewDir * 24.0f);
							JumpVel = trace.Normal * -50.0f;
							trace = TraceUtil.PlayerBBox( traceStart, traceEnd, controller );

							if ( trace.Fraction == 1.0f ) // open at eye level
							{
								traceStart = traceEnd; // Now trace down to see if we would actually land on a standable surface.
								traceEnd.z -= 1024.0f;
								trace = TraceUtil.PlayerBBox( traceStart, traceEnd, controller );

								if ( trace.Fraction < 1.0f && trace.Normal.z >= 0.7f )
								{
									velocity = velocity.WithZ( JumpHeight ); // Push Up
									JumpTime = 2000.0f; // Do this for 2 seconds
								}
							}
						}
					}
				}
			}
			return velocity;
		}

		public virtual Vector3 WaterJump( Vector3 velocity )
		{
			if ( JumpTime > 10000.0f )
				JumpTime = 10000.0f;

			if ( JumpTime != 0 )
			{
				JumpTime -= 1000.0f * Time.Delta;

				if ( JumpTime <= 0 || WaterLevel == 0 )
				{
					JumpTime = 0;
				}

				velocity = new Vector3( JumpVel.x, JumpVel.y, velocity.z );
			}

			return velocity;
		}

		public virtual bool InWater()
		{
			return WaterLevel > WATERLEVEL.Feet;
		}

		public virtual bool CheckWater( Vector3 position,
								 Vector3 mins,
								 Vector3 maxs,
								 float viewPos,
								 Entity pawn )
		{
			var point = TraceUtil.NewHull( position, position, mins, maxs, 1 )
				.HitLayer( CollisionLayer.All, false )
				.HitLayer( CollisionLayer.Water, true )
				.Ignore( pawn )
				.Run();

			// Assume that we are not in water at all.
			WaterLevel = WATERLEVEL.NotInWater;

			// Are we under water? (not solid and not empty?)
			if ( point.Fraction == 0.0f )
			{
				// We are at least at level one
				WaterLevel = WATERLEVEL.Feet;

				point = TraceUtil.NewHull( position, position, mins, maxs, maxs.z * 0.5f )
					.HitLayer( CollisionLayer.All, false )
					.HitLayer( CollisionLayer.Water, true )
					.Ignore( pawn )
					.Run();

				// Now check a point that is at the player hull midpoint.
				if ( point.Fraction == 0.0f )
				{
					// Set a higher water level.
					WaterLevel = WATERLEVEL.Waist;

					point = TraceUtil.NewHull( position, position, mins, maxs, viewPos )
						.HitLayer( CollisionLayer.All, false )
						.HitLayer( CollisionLayer.Water, true )
						.Ignore( pawn )
						.Run();
					// Now check the eye position. (view_ofs is relative to the origin)
					if ( point.Fraction == 0.0f )
						WaterLevel = WATERLEVEL.Eyes;
				}
				// TODO: Add Water current to basevelocity https://github.com/ValveSoftware/source-sdk-2013/blob/master/mp/src/game/shared/gamemovement.cpp#L3557
			}

			// if we just transitioned from not in water to in water, record the time it happened
			if ( (WATERLEVEL.NotInWater == OldWaterLevel)
				&& (WaterLevel > WATERLEVEL.NotInWater) )
			{
				EntryTime = Time.Now;
			}

			return InWater();
		}

		protected Vector3 GetSwimVel( float maxSpeed, bool isJumping )
		{
			Vector3 forward = Input.Rotation.Forward;
			Vector3 side = Input.Rotation.Left;
			float forwardSpeed = Input.Forward * maxSpeed;
			float sideSpeed = Input.Left * maxSpeed;
			float upSpeed = Input.Up * maxSpeed;
			Vector3 strafeVel = (forward * forwardSpeed) + (side * sideSpeed);

			if ( isJumping )
			{
				strafeVel.z += maxSpeed;
			}
			else if ( forwardSpeed == 0 && sideSpeed == 0 && upSpeed == 0 ) // Sinking after no other movement occurs
			{
				strafeVel.z -= SinkSpeed;
			}
			else
			{
				strafeVel.z += upSpeed + CapWishSpeed( forwardSpeed * forward.z * 2.0f, maxSpeed );
			}

			return strafeVel;
		}

		protected static float GetNewSpeed( float speed,
									 float friction,
									 ref Vector3 velocity )
		{
			float newSpeed;

			if ( speed > 0 )
			{
				newSpeed = speed - Time.Delta * speed * friction;

				if ( newSpeed < 0.1f )
					newSpeed = 0;

				velocity *= newSpeed / speed;
			}
			else
			{
				newSpeed = 0;
			}

			return newSpeed;
		}

		public virtual void Move( BaseController controller,
						   Vector3 strafeVel = new Vector3() )
		{
			strafeVel = GetSwimVel( (float)controller.MoveProp["MaxSpeed"],
						  controller.GetPlayer().KeyDown( InputButton.Jump ) );
			Vector3 velocity = controller.Velocity;
			Vector3 position = controller.Position;
			Vector3 strafeDir = strafeVel.Normal;
			Vector3 startTrace;
			Vector3 endTrace;
			TraceResult trace;
			float speed = velocity.Length;
			float newSpeed = GetNewSpeed( speed,
								(float)controller.MoveProp["WaterFriction"],
								ref velocity );
			float strafeVelLength = CapWishSpeed( strafeVel.Length, (float)controller.MoveProp["SwimSpeed"] );

			// water acceleration
			if ( strafeVelLength >= 0.1f )
			{
				float addSpeed = strafeVelLength - newSpeed;

				if ( addSpeed <= 0 )
					addSpeed = strafeVelLength - newSpeed - velocity.Dot( strafeDir );

				float accelSpeed = CapWishSpeed( (float)controller.MoveProp["WaterAccelerate"]
									  * strafeVelLength
									  * Time.Delta, addSpeed );
				velocity += accelSpeed * strafeDir;
			}

			velocity += controller.BaseVelocity;
			endTrace = position + velocity * Time.Delta;
			trace = TraceUtil.PlayerBBox( position, endTrace, controller );

			if ( trace.Fraction == 1.0f )
			{
				startTrace = endTrace;

				if ( (bool)controller.MoveProp["AllowAutoMovement"] )
					startTrace.WithZ( startTrace.z + (float)controller.MoveProp["StepSize"] + 1 );

				trace = TraceUtil.PlayerBBox( startTrace, endTrace, controller );

				if ( !trace.StartedSolid )
				{
					//float stepDist = trace.EndPos.z - pos.z;
					//mv->m_outStepHeight += stepDist;
					controller.Position = trace.EndPosition;
					controller.Velocity = velocity - controller.BaseVelocity;
					return;
				}

				controller.TryPlayerMove();
			}
			else
			{
				if ( !controller.OnGround() )
				{
					controller.TryPlayerMove();
					controller.Velocity = velocity - controller.BaseVelocity;
					return;
				}

				controller.StepMove();
			}

			controller.Velocity = velocity - controller.BaseVelocity;
		}
	}
}
