using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Momentum
{
    public partial class MomentumPlayer : Player
	{
        protected ulong SpawnButtons = ((ulong) InputButton.Forward | (ulong) InputButton.Right | (ulong) InputButton.Left | (ulong) InputButton.Back | (ulong) InputButton.Jump);
		public MomentumPlayer(){}


		public override void Respawn()
		{
			SetModel("models/citizen/citizen.vmdl");
			Controller = new MomentumController();
			Animator = new StandardPlayerAnimator();
			CameraMode = new MomentumCamera();
			
			EnableAllCollisions = true;
			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;
			base.Respawn();
		}

		public override void BuildInput(InputBuilder input)
		{
			base.BuildInput(input);
			ProcessMoveButtons();
		}

		public override void Simulate(Client client)
		{
			if (IsServer)
				ProcessMoveButtons();

			if (LifeState != LifeState.Dead)
			{
				var controller = GetActiveController();

				controller?.Simulate(client, this, GetActiveAnimator());
			}
			else
			{
				if (KeyPressed(SpawnButtons) && (IsServer))
					Respawn();

				return;
			}
		}

		public override void FrameSimulate(Client client)
		{
			if (LifeState != LifeState.Dead)
			{
				base.FrameSimulate(client);
			}
		}

		public override void OnKilled()
		{
			base.OnKilled();
			EnableDrawing = false;
		}
	}
}
