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
using System.Collections;
using R2Core.Device;
using System.Dynamic;

namespace R2Core {
    /// <summary>
    /// Capable of invoking methods, properties and members of objects.
    /// </summary>
    public class ObjectInvoker : DeviceBase {

        public ObjectInvoker() : base(Settings.Identifiers.ObjectInvoker()) { }

        /// <summary>
        /// Invoke method 'method' on object 'target' using input parameters 'parameters". Return the value(if any).
        /// </summary>
        /// <param name="target">Target.</param>
        /// <param name="method">Method.</param>
        /// <param name="parameters">Parameters.</param>
        public dynamic Invoke(object target, string method, ICollection<object> parameters = null) {

            MethodInfo methodInfo = target.GetType().GetMethod(method);

            if (methodInfo == null && target is IInvokable) {

                dynamic[] para = parameters?.Select(ppp => ppp as dynamic).ToArray();
                return (target as IInvokable).Invoke(method, para);

            }

            if (methodInfo == null) {

                throw new ArgumentException($"Method '{method}' in '{target}' not found.");

            }

            ParameterInfo[] paramsInfo = methodInfo.GetParameters();

            if (paramsInfo.Length != (parameters?.Count ?? 0)) {

                throw new ArgumentException($"Wrong number of arguments for {method} in {target}. {parameters?.Count} provided but {paramsInfo.Length} are required.");

            }

            List<object> p = new List<object>();

            // Convert the dynamic parameters to the types required by the subject.
            for (int i = 0; i < paramsInfo.Length; i++) {

                object parameter = parameters.ToArray()[i];
                Type requiredType = paramsInfo[i].ParameterType;

                p.Add(requiredType.ConvertObject(parameter));

            }


            dynamic invocationResult = target.GetType().GetMethod(method).Invoke(target, p.ToArray());

            if (invocationResult == null) {

                return invocationResult;
            
            }

            return methodInfo.ReturnType.ConvertObject((object)invocationResult); ;

        }

        /// <summary>
        /// Returns the value of the property or member of the `target`.
        /// </summary>
        /// <param name="target">Target.</param>
        /// <param name="property">Property.</param>
        public dynamic Get(object target, string property) {

            PropertyInfo propertyInfo = target.GetType().GetProperty(property);

            if (propertyInfo == null && target is IInvokable) {

                return (target as IInvokable).Get(property);

            }

            if (propertyInfo == null) {

                // If no property found. Try to find a member variable instead.

                MemberInfo[] members = target.GetType().GetMember(property);

                if (members.Length == 0) {

                    throw new ArgumentException($"Property '{property}' not found in '{target}'.");

                }

                if (!(members[0] is FieldInfo)) {

                    throw new ArgumentException($"Get property: Unable to access '{property}' in '{target}'.");

                }

                FieldInfo fieldInfo = (members[0] as FieldInfo);

                return fieldInfo.GetValue(target);

            }

            return propertyInfo.GetValue(target);

        }

        /// <summary>
        /// Set the 'value' of a member or property named 'property' on object 'target'.
        /// </summary>
        /// <param name="target">Target.</param>
        /// <param name="property">Property.</param>
        /// <param name="value">Value.</param>
        public void Set(object target, string property, object value) {

            PropertyInfo propertyInfo = target.GetType().GetProperty(property);

            if (propertyInfo == null && target is IInvokable) {

                (target as IInvokable).Set(property, value as dynamic);
                return;

            }

            if (propertyInfo == null) {

                // If no property found. Try to find a member variable instead.

                MemberInfo[] members = target.GetType().GetMember(property);

                if (members.Length == 0) {

                    throw new ArgumentException($"Property '{property}' not found in '{target}'.");

                }

                if (!(members[0] is FieldInfo)) {

                    throw new ArgumentException($"Set property: Unable to access '{property}' in '{target}'.");

                }

                FieldInfo fieldInfo = (members[0] as FieldInfo);

                fieldInfo.SetValue(target, fieldInfo.FieldType.ConvertObject(value));

            } else {

                propertyInfo.SetValue(target, propertyInfo.PropertyType.ConvertObject(value));

            }

        }

        /// <summary>
        /// Returns true if `target` contains the property or member `property`
        /// </summary>
        /// <returns><c>true</c>, if property or member was containsed, <c>false</c> otherwise.</returns>
        /// <param name="target">Target.</param>
        /// <param name="property">Property.</param>
        public bool ContainsMember(object target, string property) {

            return target.GetType().GetMember(property).Length > 0 ||
                ((target is R2Dynamic) && (target as R2Dynamic).Has(property));

        }

    }

}