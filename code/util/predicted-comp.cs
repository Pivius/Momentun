using Sandbox;

namespace Momentum
{
	public partial class PredictedComponent : Entity
	{
		public MomentumPlayer Player
		{
			get => (MomentumPlayer)Owner;
			set => SetOwner( value );
		}

		public MomentumController Controller { get; set; }

		public void SetOwner( Entity player )
		{
			Owner = player;
			Parent = player;
			Controller = (MomentumController)((MomentumPlayer)player).Controller;
		}

		public override void Spawn()
		{
			Transmit = TransmitType.Owner;
		}

		public virtual void Simulate() { }

		public override void Simulate( Client client )
		{
			SetOwner( client.Pawn );
			Simulate();
		}
	}
}
