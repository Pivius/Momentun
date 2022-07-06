using Sandbox;
using System;
using TrickHop.Movement;
using TrickHop.Player;
using TrickHop.Utility;

namespace TrickHop.Controller
{
	public partial class Controller : BaseController
	{
		[Net, Predicted]
		public TimeSince DoubleJumpTime { get; set; }
		[Net, Predicted]
		public bool DoubleJumped { get; private set; }
		[Net, Predicted]
		public TimeSince ClipTime { get; set; }
		public TimeAssociatedMap<bool> ShouldClip { get; set; }
		public AirAccelerate SurfAccelerate { get; set; }
		[Net, Predicted]
		public bool IsSliding { get; set; }
		[Net, Predicted]
		public bool WasOnGround { get; set; }


		public Controller()
		{
			AirAccelerate = new QuakeAirAccelerate();
			SurfAccelerate = new AirAccelerate();
			ShouldClip = new TimeAssociatedMap<bool>( 1f, GetShouldClip );
		}

		public bool GetShouldClip()
		{
			if ( Player.IsServer )
				return ClipTime <= Player.Properties.ClipTime;
			else
				return ClipTime <= Player.Properties.ClipTime;
		}

		public virtual void TryPlayerClip( in Vector3 primalVelocity )
		{
			if ( ShouldClip.Value )
			{
				Velocity = primalVelocity;
			}
			else
			{
				ShouldClip.Value = false;
			}
		}
		public override void TryPlayerMove()
		{
			var primalVelocity = Velocity;

			base.TryPlayerMove();
			TryPlayerClip( in primalVelocity );
		}

		public override void AirMove()
		{
			var velocity = Velocity;
			MoveHelper mover = new( Position, Velocity );

			mover.Trace = mover.Trace.Size( OBBMins, OBBMaxs ).Ignore( Pawn );
			mover.MaxStandableAngle = Player.Properties.StandableAngle;
			

			var trace = mover.TraceFromTo( Position, Position + Velocity * Time.Delta );
			var angle = trace.Normal.Angle( Vector3.Up );

			if ( angle >= mover.MaxStandableAngle && angle < 90 )
			{
				IsSurfing = true;
				Player.Duck.JumpTime = Player.Duck.JumpingTime;
			}
			else
				IsSurfing = false;

			if ( IsSurfing )
			{
				SurfAccelerate.Move(
					ref velocity,
					WishVelocity,
					Player.Properties.MaxSpeed,
					Player.Properties.SurfAccelerate );
			}
			else
			{
				AirAccelerate.Move(
					ref velocity,
					WishVelocity,
					Player.Properties.MaxSpeed,
					Player.Properties.SideStrafeMaxSpeed,
					Player.Properties.AirAccelerate,
					Player.Properties.StrafeAcceleration,
					Player.Properties.AirStopAcceleration,
					Player.Properties.AirControl );
			}
			Velocity = velocity;
			Velocity += BaseVelocity;
			StepMove();
			Velocity -= BaseVelocity;
		}

		public override void StepMove()
		{
			var primalVelocity = Velocity;

			MoveHelper mover = new( Position, Velocity );
			mover.Trace = mover.Trace.Size( OBBMins, OBBMaxs ).Ignore( Pawn );
			mover.MaxStandableAngle = Player.Properties.StandableAngle;

			mover.TryMoveWithStep( Time.Delta, Player.Properties.StepSize );

			Position = mover.Position;
			Velocity = mover.Velocity;
			TryPlayerClip( in primalVelocity );
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

			float jumpVelocity = Player.Properties.JumpPower;

			if ( Player.Properties.DoubleJumpZ != 0 )
			{
				if ( DoubleJumpTime <= 0.4f )
				{
					jumpVelocity += Player.Properties.DoubleJumpZ;
					DoubleJumped = true;
				}

				DoubleJumpTime = 0f;
				ShouldClip.Value = true;
				ClipTime = 0;
			}

			Velocity = Gravity.AddGravity( Player.Properties.Gravity * 0.5f, Velocity.WithZ( jumpVelocity ) );
			AddEvent( "jump" );

			Player.Duck.JumpTime = Player.Duck.JumpingTime;
			Player.Duck.InDuckJump = true;
			ShouldClip.Value = true;
			ClipTime = 0;
		}

		public override void ApplyAccelerate()
		{
			if ( IsSliding )
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
						1 );
				Velocity = velocity;
				Velocity = Velocity.WithZ( 0 );
			}
			else
			{
				base.ApplyAccelerate();
			}
		}

		public virtual void OverBounce()
		{
			if (OldVelocity.z < -750 && Velocity.WithZ(0).LengthSquared <= MathF.Pow(100,2) && WasOnGround == false && GroundEntity != null )
			{
				Vector3 position = Position + Vector3.Up;
				Vector3 point = position - Vector3.Up * 10.25f ;
				TraceResult trace = TraceBBox( position, point );
				BetterLog.Info( Vector3.GetAngle( Vector3.Up, trace.Normal ) );
				/*
				if ( trace.Fraction <= 0 )
					return;
				if ( trace.Fraction >= 1 )
					return;*/
				//if ( trace.StartedSolid )
					//return;
				if ( Vector3.GetAngle( Vector3.Up, trace.Normal ) > Player.Properties.StandableAngle )
					return;
				if ( Player.Properties.CanOverBounce && trace.Hit )
				{
					//Log.Info( "Normal Vel: " +  Velocity );

					float speed = OldVelocity.Length;
					Position = Position + Vector3.Up;
					Velocity = ClipVelocity( OldVelocity, trace.Normal, OverClip );
					//Log.Info( "Clipped Vel: " + Velocity );
					Velocity = OldVelocity.Normal * speed;
					///Log.Info( "End Vel: " + Velocity );
					CategorizePosition( false );
				}
			}
		}

		public override void ApplyFriction()
		{
			if ( OnGround() )
			{
				if ( Player.Duck.IsDucked && Velocity.WithZ( 0 ).Length > 300 )
				{
					var velocity = Velocity.WithZ( 0 );
	
					Friction.Move( ref velocity, 0.25f, Player.Properties.StopSpeed );
					Velocity = velocity;
					IsSliding = true;
				}
				else
				{
					base.ApplyFriction();
					IsSliding = false;
				}
			}
			else
			{
				IsSliding = false;
			}
		}

		public override void Simulate()
		{
			if ( StartMove() )
				return;

			Player.Grind.Simulate( Client );

			if ( SetupMove() )
				return;
			OverBounce();
			EndMove();


			Player.Wallrun.Simulate( Client );
			WasOnGround = GroundEntity != null;
		}

	}
}
