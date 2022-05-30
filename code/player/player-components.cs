using Sandbox;

namespace Momentum
{
	public partial class MomentumPlayer : Player
	{
		[Net, Predicted]
		public Water Water { get; set; }
		[Net, Predicted]
		public Duck Duck { get; set; }

		public void CreateNewComponents()
		{
			Water = new() {Player = this};
			Duck = new() {Player = this};
		}
    }
}
