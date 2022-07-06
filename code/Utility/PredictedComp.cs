using Sandbox;
using TrickHop.Player;
using TrickHop.Controller;

namespace TrickHop.Utility
{
	public partial class PredictedComponent : Entity
	{
		public MomentumPlayer Player
		{
			get => Owner as MomentumPlayer;
			set => SetOwner( value );
		}

		public MomentumController Controller { get; set; }

		public void SetOwner( Entity player )
		{
			Owner = player;
			Parent = player;
			Controller = (player as MomentumPlayer).Controller as MomentumController;
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
