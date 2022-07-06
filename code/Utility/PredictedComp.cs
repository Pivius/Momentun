using Sandbox;
using TrickHop.Player;
using TrickHop.Controller;

namespace TrickHop.Utility
{
	public partial class PredictedComponent : Entity
	{
		public Player.Player Player
		{
			get => Owner as Player.Player;
			set => SetOwner( value );
		}

		public Controller.Controller Controller { get; set; }

		public void SetOwner( Entity player )
		{
			Owner = player;
			Parent = player;
			Controller = (player as Player.Player).Controller as Controller.Controller;
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
