using Sandbox;

namespace Momentum
{
	public partial class MomentumController : BaseController
	{
		[Net, Predicted]
		public TimeSince DoubleJumpTime { get; set; }
		[Net, Predicted]
		public bool DoubleJumped { get; private set; }
		[Net, Predicted]
		public TimeSince ClipTime { get; set; }
		public TimeAssociatedMap<bool> ShouldClip { get; set; }
		[Net, Predicted]
		public AirAccelerate SurfAccelerate { get; set; }

		public MomentumController()
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

		public override void Simulate()
		{
			if ( StartMove() )
				return;

			if ( SetupMove() )
				return;

			EndMove();

			Player.Walljump.Simulate( Client );
		}

	}
}
