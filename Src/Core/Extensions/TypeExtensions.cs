using System;

namespace R2Core
{
	public static class TypeExtensions {

		/// <summary>
		/// Returns the default(null, 0 etc) value for the Type
		/// </summary>
		/// <returns>The value.</returns>
		/// <param name="self">Self.</param>
		public static object DefaultValue(this Type self) {

			if (self.IsValueType) {
				
				return Activator.CreateInstance(self);
			
			}

			return null;
		
		}
	
	}

}