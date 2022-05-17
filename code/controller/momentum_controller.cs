namespace Momentum
{
	public class MomentumController : BaseController
	{
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
			TryPlayerMove();
			Velocity -= BaseVelocity;
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
