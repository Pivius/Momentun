﻿using Sandbox;
using TrickHop.Player;
using TrickHop.UI;

namespace TrickHop
{
	/// <summary>
	/// This is your game class. This is an entity that is created serverside when
	/// the game starts, and is replicated to the client. 
	/// 
	/// You can use this to create things like HUDs and declare which player class
	/// to use for spawned players.
	/// </summary>
	public partial class Game : Sandbox.Game
	{
		public Game()
		{
			if ( IsServer )
			{
				new HudEntity();
			}
		}

		public override void ClientJoined( Client client )
		{
			base.ClientJoined( client );

			var player = new Player.Player();
			client.Pawn = player;
			player.Respawn();
		}
	}

}
