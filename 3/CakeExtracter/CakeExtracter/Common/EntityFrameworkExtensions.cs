using System;
using System.Data.Entity;
using System.Linq;

namespace CakeExtracter.Common
{
    static class EntityFrameworkExtensions
    {
        public static T FindOrCreateByKey<T>(this DbContext db, object pk, Func<T> createNew) where T : class
        {
            T result;
            var set = db.Set<T>();
            result = set.Find(pk);
            if (result == null)
            {
                result = createNew();
                set.Add(result);
            }
            return result;
        }

        public static T FindOrCreateByPredicate<T>(this DbContext db, Func<T, bool> predicate, Func<T> createNew) where T : class
        {
            T result;
            var set = db.Set<T>();
            result = set.FirstOrDefault(predicate);
            if (result == null)
            {
                result = createNew();
                set.Add(result);
            }
            return result;
        }

        public static string ChangeCountsAsString(this DbContext db)
        {
            return string.Join(" ", from count in
                                        (from stateName in Enum.GetNames(typeof(EntityState))
                                         select new
                                         {
                                             State = stateName,
                                             Count = (from entry in db.ChangeTracker.Entries()
                                                      where entry.State == (EntityState)Enum.Parse(typeof(EntityState), stateName)
                                                      select entry).Count()

                                         })
                                    select string.Format("[{0} {1}]", count.Count, count.State));
        }
    }
}
