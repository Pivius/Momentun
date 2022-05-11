using Sandbox;
using Sandbox.UI.Construct;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace Momentum
{
	/// <summary>
	/// This is your game class. This is an entity that is created serverside when
	/// the game starts, and is replicated to the client. 
	/// 
	/// You can use this to create things like HUDs and declare which player class
	/// to use for spawned players.
	/// </summary>
	public partial class MomentumGame : Sandbox.Game
	{
		public MomentumGame()
		{
			if ( IsServer )
			{
				new HudEntity();
			}
		}

		public override void ClientJoined(Client client)
		{
			base.ClientJoined(client);

			var player = new MomentumPlayer();
			client.Pawn = player;
			player.Respawn();
		}
	}

}
