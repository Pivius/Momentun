using Sandbox.UI;

namespace TrickHop.UI
{
	public partial class HudEntity : Sandbox.HudEntity<RootPanel>
	{
		public HudEntity()
		{
			if ( IsClient )
			{
				RootPanel.StyleSheet.Load( "/UI/HUDEntity.scss" );
				RootPanel.AddChild<HUDModule>();
				//RootPanel.AddChild<Velocity>();
			}
		}
	}

}
