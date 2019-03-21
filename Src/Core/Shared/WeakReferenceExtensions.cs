using System;
using System.Collections.Generic;

namespace R2Core {

    public static class WeakReferenceExtensions {

        /// <summary>
        /// Removes all references that is null.
        /// </summary>
        /// <returns>The empty.</returns>
        /// <param name="self">Self.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
       public static List<WeakReference<T>> RemoveEmpty<T>(this List<WeakReference<T>> self) where T: class {

            List<WeakReference<T>> objects = new List<WeakReference<T>>();

            foreach (WeakReference<T> reference in self) {

                T obj;

                if (reference.TryGetTarget(out obj)) {

                    objects.Add(new WeakReference<T>(obj));

                }

            }

            return objects;

        }


    }

}
