using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Momentum
{
	public struct Parse
	{
		/// <summary>
		/// Parse Vector, Angles, Rotation or numerals to list
		/// </summary>
		public static List<float> ToList( object obj )
		{
			var list = new List<float>();
			string[] objectArray = obj.ToString().Split( ',' );

			for ( int i = 0; i < objectArray.Length; i++ )
			{
				var match = Regex.Match( objectArray[i], @"([-+]?[0-9]*\.?[0-9]+)" );
				list.Add( Convert.ToSingle( match.Groups[1].Value ) );
			}

			return list;
		}

		public static T FromListToEquatable<T>( List<float> list )
		{
			string[] array = list.Select( i => i.ToString() ).ToArray();
			var type = typeof( T ).ToString();
			string value = array[0];

			for ( int i = 1; i < array.Length; i++ )
				value = value + "," + array[i];

			switch ( type )
			{
				case "Angles":
					return (T)(object)Angles.Parse( value );
				case "Rotation":
					return (T)(object)Rotation.Parse( value );
				case "Vector2":
					return (T)(object)Vector2.Parse( value );
				case "Vector3":
					return (T)(object)Vector3.Parse( value );
				case "Vector4":
					return (T)(object)Vector4.Parse( value );
			}

			return (T)(object)value;
		}
	}
}
