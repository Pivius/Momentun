using Sandbox;
using System;

namespace Momentum
{
	public class MomentumController : WalkController
	{
		public MomentumController()
		{
		}

		public virtual MomentumPlayer GetPlayer()
		{
			return (MomentumPlayer)Pawn;
		}

		public override void Simulate()
		{
			base.Simulate();
		}

	}
}
