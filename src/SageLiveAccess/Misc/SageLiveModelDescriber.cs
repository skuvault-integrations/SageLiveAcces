using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SageLiveAccess.sforce;

namespace SageLiveAccess.Misc
{
	class SageLiveModelDescriber
	{
		private readonly SforceService _binding;

		public SageLiveModelDescriber( SforceService binding )
		{
			this._binding = binding;
		}

		public IEnumerable< string > GetAllFieldsDescribed( Type obj )
		{
			var describerResult = this._binding.describeSObjects( new string[] { obj.Name } );
			var describedObject = describerResult[ 0 ];
			var acc = "";
			for ( int i = 0; i < describedObject.fields.Length; i++ )
			{
				yield return describedObject.fields[ i ].name;
				// Get the field 
				//				acc = acc + describedObject.fields[ i ].name;
				//				if ( i != describedObject.fields.Length - 1 )
				//				{
				//					acc = acc + ", ";
				//				}
			}
//			return acc;
		}
	}
}
