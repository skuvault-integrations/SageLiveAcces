using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SageLiveAccess.Misc
{
	static class ListExtensions
	{
		public static IEnumerable<List<T>> Partition<T>( this IList<T> source, Int32 size )
		{
			for ( int i = 0; i < Math.Ceiling( source.Count / ( Double ) size ); i++ )
				yield return new List<T>( source.Skip( size * i ).Take( size ) );
		}
	}
}
