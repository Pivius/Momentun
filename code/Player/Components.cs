using Sandbox;
using TrickHop.Movement;
using Duck = TrickHop.Movement.Duck;
using Water = TrickHop.Movement.Water;

namespace TrickHop.Player
{
	public partial class Player
	{
		[Net, Predicted]
		public Water Water { get; set; }
		[Net, Predicted]
		public Duck Duck { get; set; }
		[Net, Predicted]
		public Wallrun Wallrun { get; set; }
		[Net, Predicted]
		public Grind Grind { get; set; }
		public void CreateNewComponents()
		{
			Water = new() { Player = this };
			Duck = new() { Player = this };
			Wallrun = new() { Player = this };
			Grind = new() { Player = this };
		}
	}
}
