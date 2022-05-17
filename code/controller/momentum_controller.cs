using Sandbox;

namespace Momentum
{
	public class MomentumController : BaseController
	{
		public TimeSince DoubleJumpTime;
		public bool DoubleJumped { get; private set; }

		public MomentumController()
		{
			AirAccelerate = new QuakeAirAccelerate();
		}

		public override void AirMove()
		{
			var velocity = Velocity;
			AirAccelerate.Move( ref velocity,
						WishVelocity,
						(float)MoveProp["MaxSpeed"],
						(float)MoveProp["SideStrafeMaxSpeed"],
						(float)MoveProp["AirAccelerate"],
						(float)MoveProp["StrafeAcceleration"],
						(float)MoveProp["AirStopAcceleration"],
						(float)MoveProp["AirControl"] );
			Velocity = velocity;
			Velocity += BaseVelocity;
			//TryPlayerMove();
			StepMove();
			Velocity -= BaseVelocity;
		}

		public override void StepMove()
		{
			var primalVelocity = Velocity;

			MoveHelper mover = new( Position, Velocity );
			mover.Trace = mover.Trace.Size( OBBMins, OBBMaxs ).Ignore( Pawn );
			mover.MaxStandableAngle = (float)MoveProp["StandableAngle"];

			mover.TryMoveWithStep( Time.Delta, (float)MoveProp["StepSize"] );

			Position = mover.Position;
			Velocity = mover.Velocity;
			TryPlayerClip( in primalVelocity );
		}

		public override void CheckJumpButton()
		{

			if ( Water.JumpTime > 0.0f )
			{
				Water.JumpTime -= Time.Delta;

				if ( Water.JumpTime < 0.0f )
					Water.JumpTime = 0;

				return;
			}

			if ( Water.WaterLevel >= WATERLEVEL.Waist )
			{
				ClearGroundEntity();
				Velocity = Velocity.WithZ( 100 );
				return;
			}

			if ( !OnGround() )
				return;

			ClearGroundEntity();

			float jumpVelocity = (float)MoveProp["JumpPower"];

			if ( (float)MoveProp["DoubleJumpZ"] != 0 )
			{
				if ( DoubleJumpTime <= 0.4f )
				{
					jumpVelocity += (float)MoveProp["DoubleJumpZ"];
					DoubleJumped = true;
				}

				DoubleJumpTime = 0f;
				ShouldClip.Value = true;
				ClipTime = 0;
			}

			Velocity = Gravity.AddGravity( (float)MoveProp["Gravity"] * 0.5f, Velocity.WithZ( jumpVelocity ) );
			AddEvent( "jump" );

			Duck.JumpTime = Duck.JumpingTime;
			Duck.InDuckJump = true;
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
		}

	}
}
