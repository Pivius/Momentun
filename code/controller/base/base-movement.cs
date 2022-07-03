using Sandbox;
using System;
using Trace = Sandbox.Trace;

namespace Momentum
{
	public abstract partial class BaseController : WalkController
	{
		// # Fields
		public new Gravity Gravity;
		public AirAccelerate AirAccelerate;
		public new Accelerate Accelerate;
		public Friction Friction;
		public float OverClip = 1.001f;

		public float GetWalkSpeed()
		{
			float defaultSpeed = Player.Properties.DefaultSpeed;
			bool isWalking = Input.Down( InputButton.Walk ) && Player.Properties.CanWalk;
			bool isRunning = Input.Down( InputButton.Run ) && Player.Properties.CanRun;
			float walkSpeed = isWalking ? Player.Properties.WalkSpeed :
							(isRunning ? Player.Properties.RunSpeed :
							defaultSpeed);

			return Player.Duck.IsDucked ? walkSpeed * (Player.Properties.DuckedWalkSpeed / defaultSpeed) : walkSpeed;
		}

		public static Vector3 WishVel( float strafeSpeed )
		{
			Vector3 forward = Input.Rotation.Forward;
			float forwardSpeed = Input.Forward * strafeSpeed;
			float sideSpeed = Input.Left * strafeSpeed;
			Vector3 forwardWish = new Vector3( forward.x, forward.y, 0 ).Normal * forwardSpeed;
			Vector3 sideWish = new Vector3( -forward.y, forward.x, 0 ).Normal * sideSpeed;

			return forwardWish + sideWish;
		}

		public static Vector3 ClipVelocity( Vector3 velocity, Vector3 normal )
		{
			return velocity - (normal * velocity.Dot( normal ));
		}

		public static Vector3 ClipVelocity( Vector3 velocity, Vector3 normal, float overBounce )
		{
			var backOff = velocity.Dot( normal );

			if ( backOff < 0 )
				backOff *= overBounce;
			else
				backOff /= overBounce;

			return velocity - (normal * backOff);
		}

		/// <summary>
		/// Consistent speed boosts when landing on a slope
		/// </summary>
		public virtual void AddSlopeSpeed()
		{
			TraceResult trace = TraceUtil.PlayerBBox( Position,
											Position.WithZ( Position.z - 2 ),
											this );
			Vector3 normal = trace.Normal;

			if ( normal.z < 1
				&& Velocity.z <= 0
				&& OnGround()
				&& Player.Properties.MoveState == STATE.INAIR )
			{
				Velocity -= normal * Velocity.Dot( normal );

				if ( Velocity.Dot( normal ) < 0 )
					Velocity = ClipVelocity( Velocity, normal );
			}
		}

		public override void CategorizePosition( bool bStayOnGround )
		{
			SurfaceFriction = 1.0f;

			var point = Position - Vector3.Up * 2;
			var vBumpOrigin = Position;

			//
			//  Shooting up really fast.  Definitely not on ground trimed until ladder shit
			//
			float MaxNonJumpVelocity = 140.0f;
			bool bMovingUpRapidly = Velocity.z > MaxNonJumpVelocity;
			_ = Velocity.z > 0;

			bool bMoveToEndPos = false;

			if ( GroundEntity != null ) // and not underwater
			{
				bMoveToEndPos = true;
				point.z -= StepSize;
			}
			else if ( bStayOnGround )
			{
				bMoveToEndPos = true;
				point.z -= StepSize;
			}

			if ( bMovingUpRapidly || Swimming ) // or ladder and moving up
			{
				ClearGroundEntity();
				return;
			}

			var pm = TraceBBox( vBumpOrigin, point, 4.0f );
			var angle = Vector3.GetAngle( Vector3.Up, pm.Normal );

			if ( pm.Entity == null ||
				angle > Player.Properties.StandableAngle )
			{
				ClearGroundEntity();
				bMoveToEndPos = false;
				
				if ( Velocity.z > 0 )
					SurfaceFriction = 0.25f;
			}
			else
			{
				base.UpdateGroundEntity( pm );
				IsSurfing = false;
			}

			if ( bMoveToEndPos && !pm.StartedSolid && pm.Fraction > 0.0f && pm.Fraction < 1.0f )
			{
				Position = pm.EndPosition;
			}
		}
		public override void ClearGroundEntity()
		{
			base.ClearGroundEntity();
		}
		public override void StepMove()
		{
			MoveHelper mover = new( Position, Velocity );
			mover.Trace = mover.Trace.Size( OBBMins, OBBMaxs ).Ignore( Pawn );
			mover.MaxStandableAngle = Player.Properties.StandableAngle;

			mover.TryMoveWithStep( Time.Delta, Player.Properties.StepSize );
			
			Position = mover.Position;
			Velocity = mover.Velocity;
		}

		/// <summary>
		/// Does sliding when on a ramp or surf
		/// </summary>
		public virtual void TryPlayerMove()
		{
			var primalVelocity = Velocity;

			MoveHelper mover = new( Position, Velocity );
			mover.Trace = mover.Trace.Size( OBBMins, OBBMaxs ).Ignore( Pawn );
			mover.MaxStandableAngle = Player.Properties.StandableAngle;

			mover.TryMove( Time.Delta );

			var trace = mover.TraceFromTo( Position, Position );
			var angle = trace.Normal.Angle( Vector3.Up );
			
			if ( angle < mover.MaxStandableAngle && angle > 0 )
			{
				IsSurfing = true;
			}
			else
				IsSurfing = false;

			Position = mover.Position;
			Velocity = mover.Velocity;
		}

		public override void AirMove()
		{
			var velocity = Velocity;

			AirAccelerate.Move( ref velocity,
						WishVelocity,
						Player.Properties.MaxSpeed,
						Player.Properties.AirAccelerate );
			Velocity = velocity;
			Velocity += BaseVelocity;
			TryPlayerMove();
			Velocity -= BaseVelocity;
		}

		public override void StayOnGround()
		{
			var start = Position + Vector3.Up * 2;
			var end = Position + Vector3.Down * Player.Properties.StepSize;

			// See how far up we can go without getting stuck
			var trace = TraceBBox( Position, start );
			start = trace.EndPosition;

			// Now trace down from a known safe position
			trace = TraceBBox( start, end );

			if ( trace.Fraction <= 0 ) return;
			if ( trace.Fraction >= 1 ) return;
			if ( trace.StartedSolid ) return;
			if ( Vector3.GetAngle( Vector3.Up, trace.Normal ) > Player.Properties.StandableAngle ) return;

			// This is incredibly hacky. The real problem is that trace returning that strange value we can't network over.
			// float flDelta = fabs(mv->GetAbsOrigin().z - trace.m_vEndPos.z);
			// if (flDelta > 0.5f * DIST_EPSILON)

			Position = trace.EndPosition;
		}

		public virtual void ApplyAccelerate()
		{
			var wishDir = WishVelocity.Normal;
			var wishSpeed = WishVelocity.Length;
			Vector3 velocity;

			WishVelocity = WishVelocity.WithZ( 0 );
			WishVelocity = WishVelocity.Normal * wishSpeed;
			Velocity = Velocity.WithZ( 0 );
			velocity = Velocity;
			Accelerate.Move( ref velocity,
					WishVelocity,
					GetWalkSpeed(),
					Player.Properties.Accelerate );
			Velocity = velocity;
			Velocity = Velocity.WithZ( 0 );
		}

		public override void WalkMove()
		{
			ApplyAccelerate();

			// Add in any base velocity to the current velocity.
			Velocity += BaseVelocity;

			try
			{
				if ( Velocity.Length < 1.0f )
				{
					Velocity = Vector3.Zero;
					return;
				}

				// first try just moving to the destination	
				var dest = (Position + Velocity * Time.Delta).WithZ( Position.z );

				var pm = TraceUtil.PlayerBBox( Position, dest, this );

				if ( pm.Fraction == 1 )
				{
					Position = pm.EndPosition;
					StayOnGround();
					return;
				}

				StepMove();
			}
			finally
			{
				// Now pull the base velocity back out.  Base velocity is set if you are on a moving object, like a conveyor (or maybe another monster?)
				Velocity -= BaseVelocity;
			}

			StayOnGround();
		}

		public override void CheckJumpButton()
		{

			if ( Player.Water.JumpTime > 0.0f )
			{
				Player.Water.JumpTime -= Time.Delta;

				if ( Player.Water.JumpTime < 0.0f )
					Player.Water.JumpTime = 0;

				return;
			}

			if ( Player.Water.WaterLevel >= WATERLEVEL.Waist )
			{
				ClearGroundEntity();
				Velocity = Velocity.WithZ( 100 );
				return;
			}

			if ( !OnGround() )
				return;

			ClearGroundEntity();
			Velocity = Gravity.AddGravity( Player.Properties.Gravity * 0.5f, Velocity.WithZ( Player.Properties.JumpPower ) );
			AddEvent( "jump" );

			Player.Duck.JumpTime = Player.Duck.JumpingTime;
			Player.Duck.InDuckJump = true;
		}

		public override void CheckLadder()
		{
			if ( IsTouchingLadder && Input.Pressed( InputButton.Jump ) )
			{
				Velocity = LadderNormal * 100.0f;
				IsTouchingLadder = false;
				return;
			}

			const float ladderDistance = 1.0f;
			var start = Position;
			Vector3 end = start
				+ (IsTouchingLadder ? (LadderNormal * -1.0f) : WishVelocity.Normal)
				* ladderDistance;

			var pm = Trace.Ray( start, end )
						.Size( OBBMins, OBBMaxs )
						.HitLayer( CollisionLayer.All, false )
						.HitLayer( CollisionLayer.LADDER, true )
						.Ignore( Pawn )
						.Run();

			IsTouchingLadder = false;

			if ( pm.Hit )
			{
				IsTouchingLadder = true;
				LadderNormal = pm.Normal;
			}
		}

		public virtual void OverBounce()
		{
			Vector3 point = Position - Vector3.Up * 0.25f;
			TraceResult trace = TraceBBox( Position, point );
			DebugOverlay.ScreenText( (Position.z).ToString(), 2 );
			DebugOverlay.ScreenText( (trace.EndPosition.z).ToString(), 3 );
			DebugOverlay.ScreenText( (Position.z - trace.EndPosition.z).ToString() );
			
			if ( trace.Fraction <= 0 ) 
				return;
			if ( trace.Fraction >= 1 ) 
				return;
			if ( trace.StartedSolid )
				return;
			if ( Vector3.GetAngle( Vector3.Up, trace.Normal ) > Player.Properties.StandableAngle ) 
				return;
			if ( Player.Properties.CanOverBounce && trace.Hit && Velocity.z < 150 )
			{
				//Log.Info( "Normal Vel: " +  Velocity );

				float speed = Velocity.Length;
					
				Velocity = ClipVelocity( Velocity, trace.Normal, OverClip );
				//Log.Info( "Clipped Vel: " + Velocity );
				Velocity = Velocity.Normal * speed;
				///Log.Info( "End Vel: " + Velocity );
				CategorizePosition( false );
			}
		}

		public virtual void ApplyFriction()
		{
			if ( OnGround() )
			{
				Velocity = Velocity.WithZ( 0 );
				var velocity = Velocity;
				Friction.Move( ref velocity, Player.Properties.Friction, Player.Properties.StopSpeed );
				Velocity = velocity;
			}
		}

		/// <summary>
		/// Runs first in the simulate method
		/// </summary>
		/// <returns>
		/// return true to not run anything past this event
		/// </returns>
		public virtual bool StartMove()
		{
			//if (Host.IsServer)
			//Player.Duck.TryDuck();
			//Log.Info( GetViewOffset() );
			EyeLocalPosition = Vector3.Up * GetViewOffset() * Pawn.Scale;
			EyeRotation = Input.Rotation;
			WishVelocity = WishVel( Player.Properties.MaxMove );
			UpdateBBox();

			if ( Unstuck.TestAndFix() )
				return true;

			// RunLadderMode
			CheckLadder();

			return false;
		}

		/// <summary>
		/// Runs in the middle of the simulate method
		/// </summary>
		/// <returns>
		/// return true to not run anything past this event
		/// </returns>
		public virtual bool SetupMove()
		{
			Swimming = Player.Water.CheckWater( Position, OBBMins, OBBMaxs, GetViewOffset(), Pawn );

			//
			// Start Gravity
			//
			if ( !Swimming && !IsTouchingLadder )
			{
				Velocity = Gravity.AddGravity( Player.Properties.Gravity * 0.5f, Velocity );
				Velocity += new Vector3( 0, 0, BaseVelocity.z ) * Time.Delta;
				BaseVelocity = BaseVelocity.WithZ( 0 );
			}

			if ( Player.Water.JumpTime > 0.0f )
			{
				Velocity = Player.Water.WaterJump( Velocity );
				TryPlayerMove();
				return true;
			}

			if ( Player.Water.WaterLevel >= WATERLEVEL.Waist )
			{
				if ( Player.Water.WaterLevel == WATERLEVEL.Waist )
					Velocity = Player.Water.CheckWaterJump( Velocity, Position );

				if ( Velocity.z < 0.0f && Player.Water.JumpTime > 0.0f )
					Player.Water.JumpTime = 0.0f;

				if ( Player.Properties.AutoJump ? Input.Down( InputButton.Jump ) : Input.Pressed( InputButton.Jump ) )
					CheckJumpButton();

				Player.Water.Simulate( Client );
				CategorizePosition( OnGround() );

				if ( OnGround() )
					Velocity = Velocity.WithZ( 0 );

				Player.Properties.MoveState = STATE.WATER;
			}
			else
			{

				if ( Player.Properties.AutoJump ? Input.Down( InputButton.Jump ) : Input.Pressed( InputButton.Jump ) )
					CheckJumpButton();

				OverBounce();

				ApplyFriction();

				Player.Duck.UpdateDuckJumpEyeOffset();
				Player.Duck.Simulate( Client );

				if ( !IsTouchingLadder )
					WishVelocity = WishVelocity.WithZ( 0 );

				bool bStayOnGround = false;

				if ( IsTouchingLadder )
				{
					LadderMove();
					Player.Properties.MoveState = STATE.LADDER;
				}
				else if ( OnGround() )
				{
					bStayOnGround = true;
					WalkMove();
					Player.Properties.MoveState = STATE.GROUND;
				}
				else
				{
					AirMove();
					Player.Properties.MoveState = STATE.INAIR;

					if ( MathF.Abs( Velocity.x ) == 1 && MathF.Abs( Velocity.y ) == 1 )
						Velocity = Velocity.WithX( 0 ).WithY( 0 );
				}

				// FinishGravity
				if ( !IsTouchingLadder )
					Velocity = Gravity.AddGravity( Player.Properties.Gravity * 0.5f, Velocity );

				if ( OnGround() )
				{
					AddSlopeSpeed();
					Velocity = Velocity.WithZ( 0 );
				}

				CategorizePosition( bStayOnGround );
			}
			return false;
		}

		/// <summary>
		/// Is the last to run in the simulate method
		/// </summary>
		public virtual void EndMove()
		{
			OldVelocity = Velocity;
		}
	}
}
