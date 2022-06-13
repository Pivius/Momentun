using Sandbox;

namespace Momentum
{
	public partial class MomentumPlayer : Player
	{
		protected ulong SpawnButtons = (ulong)InputButton.Forward
									| (ulong)InputButton.Right
									| (ulong)InputButton.Left
									| (ulong)InputButton.Back
									| (ulong)InputButton.Jump;
		public int Flags = 0;

		[Net]
		public Properties Properties { get; set; }

		public MomentumPlayer() { }

		public override void Respawn()
		{
			CreateNewComponents();
			SetModel( "models/citizen/citizen.vmdl" );
			Properties = new Properties();
			Controller = new MomentumController();
			Animator = new StandardPlayerAnimator();
			CameraMode = new MomentumCamera();

			EnableAllCollisions = true;
			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;
			base.Respawn();
		}

		public virtual void AddFlag( PlayerFlags flag ) => Flags |= (int)flag;

		public virtual bool GetFlag( PlayerFlags flag ) => (Flags &= ((int)flag)) != 0;

		public virtual void RemoveFlag( PlayerFlags flag ) => Flags &= ~(int)flag;

		public override void BuildInput( InputBuilder input )
		{
			base.BuildInput( input );
			ProcessMoveButtons();
		}

		public override void Simulate( Client client )
		{
			ProcessMoveButtons();

			if ( LifeState != LifeState.Dead )
			{
				var controller = GetActiveController();

				controller?.Simulate( client, this, GetActiveAnimator() );
			}
			else
			{
				if ( KeyPressed( SpawnButtons ) && (IsServer) )
					Respawn();

				return;
			}
		}

		public override void FrameSimulate( Client client )
		{
			if ( LifeState != LifeState.Dead )
			{
				base.FrameSimulate( client );
			}
		}

		public override void OnKilled()
		{
			base.OnKilled();
			EnableDrawing = false;
		}
	}
}
