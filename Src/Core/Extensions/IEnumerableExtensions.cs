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
using System.Linq;

namespace R2Core
{
	public static class IEnumerableExtensions {

		/// <summary>
		/// Removes objects qualifying for the specified predicate
		/// </summary>
		/// <param name="predicate">Predicate.</param>
		/// <typeparam name="T">The generic object type.</typeparam>
		public static IEnumerable<T> Remove<T>(this ICollection<T> list, Predicate<T> predicate) {

            IEnumerable<T> removed = list.Where(item => predicate(item)).Select(client => client).ToList();

            foreach (T item in removed) {

                list.Remove(item);

            }

            return removed;

		}

	}

}