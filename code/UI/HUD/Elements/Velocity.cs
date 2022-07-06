using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;


namespace TrickHop.UI
{
	public partial class Velocity : Elements
	{
		public Velocity()
		{
			SetStyleSheet( "/UI/HUD/Elements/Velocity.scss" );
			Label = Add.Label( "100", "label" );
		}

		public Label Label;

		public override void Tick()
		{
			var player = Local.Pawn;
			if ( player == null ) return;

			Label.Text = $"{player.Velocity.WithZ( 0 ).Length:n0}";
			//Sandbox.BetterLog.Info(Label.DataBind);
		}
	}
}
