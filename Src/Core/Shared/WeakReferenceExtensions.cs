using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R2Core {

    public static class WeakReferenceExtensions {

        /// <summary>
        /// Returns all references that is null.
        /// </summary>
        /// <returns>The empty.</returns>
        /// <param name="self">Self.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
       public static List<WeakReference<T>> RemoveEmpty<T>(this List<WeakReference<T>> self) where T: class {

            List<WeakReference<T>> objects = new List<WeakReference<T>>();

            objects.Sequential((obj) => objects.Add(new WeakReference<T>(obj)));

            return objects;

        }

        /// <summary>
        /// Executes block(T) with each unwrapped non-null object contained in the squence.
        /// </summary>
        /// <param name="self">Self.</param>
        /// <param name="block">Block.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static void Sequential<T>(this IEnumerable<WeakReference<T>> self, Action<T> block) where T : class {

            foreach (WeakReference<T> reference in self) {

                T obj = reference.GetTarget();

                if (obj != default(T)) { 
                
                    block(obj);

               }

            }

        }

        /// <summary>
        /// Asynchronously executes block(T) in parallell with each unwrapped non-null object contained in the squence.
        /// </summary>
        /// <param name="self">Self.</param>
        /// <param name="block">Block.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static Task InParallell<T>(this IEnumerable<WeakReference<T>> self, Action<T> block) where T : class {

            lock (self) {

                List<WeakReference<T>> list = new List<WeakReference<T>>(self);

                return new TaskFactory().StartNew(() => {

                    list.AsParallel().ForAll((reference) => {

                        T obj = reference.GetTarget();

                        if (obj != default(T)) {

                            block(obj);

                        }

                    });

                });
               
            }

        }

        public static T GetTarget<T>(this WeakReference<T> self) where T : class {

            T obj;

            if (self.TryGetTarget(out obj))  {

                return obj;

            }

            return default(T);

        }

    }

}
