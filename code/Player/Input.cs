using Sandbox;

namespace TrickHop.Player
{
	public partial class Player
	{
		ulong Buttons { get; set; }
		ulong OldButtons { get; set; }
		public ulong[] ValidMoveButtons =
		{
			(ulong)InputButton.Jump,
			(ulong)InputButton.Duck,
			(ulong)InputButton.Forward,
			(ulong)InputButton.Back,
			(ulong)InputButton.Left,
			(ulong)InputButton.Right,
			(ulong)InputButton.Run,
			(ulong)InputButton.Walk
		};

		private void ProcessMoveButtons()
		{
			ulong buttons = 0;

			foreach ( ulong but in ValidMoveButtons )
			{
				if ( Input.Down( (InputButton)but ) )
					buttons |= but << 1;
			}

			//if (Input.MouseWheel != 0)
			//	buttons &= ((ulong)InputButton.Jump) << 1;

			OldButtons = Buttons;
			Buttons = buttons;
		}

		public bool KeyDown( object button )
		{
			var buttons = Buttons;

			return (buttons &= ((ulong)button) << 1) != 0;
		}

		public bool KeyPressed( object button )
		{
			var buttons = Buttons;
			var oldButtons = OldButtons;

			if ( (oldButtons != buttons) && (Buttons > OldButtons) )
			{
				buttons &= ((ulong)button) << 1;
				oldButtons &= ((ulong)button) << 1;

				return buttons != oldButtons && buttons != 0;
			}

			return false;
		}

		public bool KeyReleased( object button )
		{
			var buttons = Buttons;
			var oldButtons = OldButtons;

			if ( (oldButtons != buttons) && (OldButtons > Buttons) )
			{
				buttons &= ((ulong)button) << 1;
				oldButtons &= ((ulong)button) << 1;

				return buttons != oldButtons && oldButtons != 0;
			}

			return false;
		}
	}
}
