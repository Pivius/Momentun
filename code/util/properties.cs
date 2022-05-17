using System.Collections.Generic;
using System.Linq;

namespace Sandbox
{

	public sealed class Property : Dictionary<string, object>
	{
		private readonly Dictionary<string, object> _dictionary = new();

		internal Property() { }

		internal Property( Dictionary<string, object> dictionary )
		{
			foreach ( var item in dictionary )
			{
				object value = null;

				if ( item.Value is not null )
					value = item.Value;

				_dictionary.TryAdd( item.Key, value );
			}
		}

		public new ICollection<object> Values => _dictionary.Values;

		public new object this[string key]
		{
			get
			{
				if ( _dictionary.TryGetValue( key, out object value ) )
					return value;

				return null;
			}

			set => _dictionary[key] = value;
		}

		public new bool TryAdd( string key, object value )
		{
			var item = value;

			if ( !_dictionary.TryAdd( key, item ) )
			{
				_dictionary[key] = item;

				return true;
			}

			return false;
		}

		public object[] ToArray() => _dictionary.Values.ToArray();

		public new bool Remove( string key, out object value )
		{
			return _dictionary.Remove( key, out value );
		}

		/*
				public static void Print(string header, dynamic items)
				{
					Console.WriteLine(header);

					foreach (KeyValuePair<string, PropItem> item in items.ToArray())
						Console.WriteLine("  {0}", item.Value());

					Console.WriteLine();
				}*/
	}
}
