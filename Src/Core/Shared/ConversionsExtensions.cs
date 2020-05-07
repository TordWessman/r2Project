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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace R2Core {

    public static class ConversionsExtensions {

        /// <summary>
        /// Converts ´parameter´ to the System.Type required. Allows conversion even if ´parameter´ is defined
        /// as a non-specific type (i.e. ´object´ or ´dynamic´).
        /// </summary>
        /// <returns>The object.</returns>
        /// <param name="requiredType">Required type.</param>
        /// <param name="parameter">Parameter.</param>
        public static dynamic ConvertObject(this Type requiredType, object parameter) {

            if (requiredType.IsGenericType) {

                return ConvertToGeneric(requiredType, parameter);

            }

            if (requiredType.IsEnum) {

                return Enum.ToObject(requiredType, parameter);

            }

            if (requiredType.IsValueType && !requiredType.IsPrimitive) {

                return ConvertToStruct(requiredType, parameter);

            }

            if (typeof(IConvertible).IsAssignableFrom(requiredType)) {

                // Primitive type
                return Convert.ChangeType(parameter, requiredType);

            }

            return parameter;

        }


        private static dynamic ConvertToStruct(Type requiredType, object parameter) {

            var newStruct = requiredType.DefaultValue();

            IDictionary<string, dynamic> structProperties;

            if (parameter is ExpandoObject) {

                structProperties = new R2Dynamic(parameter as ExpandoObject);

            } else {

                structProperties = (IDictionary<string, dynamic>)parameter;

            }

            ObjectInvoker inv = new ObjectInvoker();

            foreach (KeyValuePair<string, dynamic> property in structProperties) {

                inv.Set(newStruct, property.Key, property.Value);

            }

            return newStruct;

        }

        private static dynamic ConvertToGeneric(Type requiredType, object parameter) {

            // Check if it's a generic IEnumerable
            if (typeof(IEnumerable).IsAssignableFrom(requiredType)) {

                var enumeratedParameter = parameter as IEnumerable;

                if (enumeratedParameter == null) {

                    throw new ArgumentException($"IEnumerable parameter required, but was of type: {parameter}.");

                }

                Type[] containedTypes = requiredType.GetGenericArguments();

                if (containedTypes.Length == 2) {

                    // Assuming Dictionary<T,S>.
                    Type genericDictionaryType = typeof(Dictionary<,>);
                    Type dictionaryType = genericDictionaryType.MakeGenericType(containedTypes);

                    IDictionary dictionary = (IDictionary)Activator.CreateInstance(dictionaryType);

                    ObjectInvoker invoker = new ObjectInvoker();

                    foreach (object p in enumeratedParameter) {

                        dynamic key = invoker.Get(p, "Key");
                        dynamic value = invoker.Get(p, "Value");

                        dictionary.Add(key, value);

                    }

                    return dictionary;

                }

                if (containedTypes.Length == 1) {

                    // Assuming IEnumerable<T>
                    Type listGenericType = typeof(List<>);
                    Type listType = listGenericType.MakeGenericType(containedTypes);

                    IList list = (IList)Activator.CreateInstance(listType);

                    foreach (object p in enumeratedParameter) {

                        list.Add(containedTypes.FirstOrDefault().ConvertObject(p));

                    }

                    return list;

                }

            }

            throw new ArgumentException($"Dynamic conversion not implemented for type {requiredType} (using parameters {parameter})");

        }

    }

}
