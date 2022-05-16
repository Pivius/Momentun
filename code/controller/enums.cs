using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Momentum
{
	public enum STATE : int
	{
		GROUND = 0,
		INAIR,
		LADDER,
		WATER
	}

	public enum WATERLEVEL : int
	{
		NotInWater = 0,
		Feet,
		Waist,
		Eyes
	}

	public enum PlayerFlags : int
	{
		NONE = 0,
		DUCKING,
	}
}
