// This file is part of r2Poject.
//
// Copyright 2016 Tord Wessman
//
// r2Project is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// r2Project is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with r2Project. If not, see <http://www.gnu.org/licenses/>.
//
//
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Core
{
	/// <summary>
	/// Capable of invoking methods, properties and members of objects.
	/// </summary>
	public class ObjectInvoker
	{
		public ObjectInvoker ()
		{
			
		}

		/// <summary>
		/// Invoke method 'method' on object 'target' using input parameters 'parameters". Return the value (if any).
		/// </summary>
		/// <param name="target">Target.</param>
		/// <param name="method">Method.</param>
		/// <param name="parameters">Parameters.</param>
		public dynamic Invoke(object target, string method, ICollection<object> parameters = null) {

			MethodInfo methodInfo = target.GetType ().GetMethod (method);

			if (methodInfo == null) {

				throw new ArgumentException ($"Method '{method}' in '{target}' not found.");

			}

			ParameterInfo[] paramsInfo = methodInfo.GetParameters ();

			if (paramsInfo.Length != (parameters?.Count ?? 0)) {

				throw new ArgumentException ($"Wrong number of arguments for {method} in {target}. {parameters?.Count} provided but {paramsInfo.Length} are required.");

			}

			IList<object> p = new List<object> ();

			// Convert the dynamic parameters to the types required by the subject.
			for (int i = 0; i < paramsInfo.Length; i++) {

				if (paramsInfo [i].ParameterType != typeof(System.Object)) {

					// Primitive type
					p.Add (Convert.ChangeType (parameters.ToArray() [i], paramsInfo [i].ParameterType));

				} else {

					p.Add (parameters.ToArray() [i]);

				}

			}

			return target.GetType ().GetMethod (method).Invoke (target, p.ToArray());

		}

		/// <summary>
		/// Set the 'value' of a member or property named 'property' on object 'target'.
		/// </summary>
		/// <param name="target">Target.</param>
		/// <param name="property">Property.</param>
		/// <param name="value">Value.</param>
		public void Set (object target, string property, object value) {
		
			PropertyInfo propertyInfo = target.GetType().GetProperty(property);

			if (propertyInfo == null) {

				// If no property found. Try to find a member variable instead.

				MemberInfo[] members = target.GetType ().GetMember (property);

				if (members.Length == 0) { 

					throw new ArgumentException ("Property '{property}' not found in '{target}'.");

				} else if (!(members [0] is FieldInfo)) {

					throw new ArgumentException ("Unable to access property '{property}' in '{target}'.");

				}

				FieldInfo fieldInfo = (members [0] as FieldInfo);

				// Do not convert complex types
				if (fieldInfo.FieldType != typeof(System.Object)) {

					fieldInfo.SetValue (target, Convert.ChangeType (value, fieldInfo.FieldType));

				} else {

					fieldInfo.SetValue (target, value);

				}

			} else {

				// Do not convert complex types
				if (propertyInfo.PropertyType != typeof(System.Object)) {

					propertyInfo.SetValue (target, Convert.ChangeType (value, propertyInfo.PropertyType), null);

				} else {

					propertyInfo.SetValue (target, value);

				}

			}

		}

	}

}