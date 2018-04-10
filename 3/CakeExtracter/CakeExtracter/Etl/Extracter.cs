using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace CakeExtracter.Etl
{
    public abstract class Extracter<T> : IDisposable
    {
        private int added;
        private readonly BlockingCollection<T> items = new BlockingCollection<T>(5000);
        private readonly object locker = new object();

        public  Thread Start()
        {
            var thread = new Thread(Extract);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return thread;
        }

        public void End()
        {
            items.CompleteAdding();
        }

        public bool Done
        {
            get { return items.IsAddingCompleted; }
        }

        public IEnumerable<T> EnumerateAll()
        {
            return items.GetConsumingEnumerable();
        }

        public int Count
        {
            get { return items.Count; }
        }

        public int Added
        {
            get { lock (locker) return added; }
        }

        protected void Add(IEnumerable<T> extracted)
        {
            foreach (var item in extracted)
            {
                items.Add(item);
                lock (locker) added++;
            }
        }

        protected void Add(T item)
        {
            Add(new List<T> { item });
        }

        /// <summary>
        /// The derived class implements this method, which calls Add() for each item
        /// extracted and then calls End() when complete.
        /// </summary>
        protected abstract void Extract();

        // http://msdn.microsoft.com/en-us/library/system.idisposable.aspx
        // Implement IDisposable. 
        // Do not make this method virtual. 
        // A derived class should not be able to override this method. 
        public void Dispose()
        {
            Dispose(true);
            // http://msdn.microsoft.com/en-us/library/system.idisposable.aspx
            // This object will be cleaned up by the Dispose method. 
            // Therefore, you should call GC.SupressFinalize to 
            // take this object off the finalization queue 
            // and prevent finalization code for this object 
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        private bool disposed;

        // http://msdn.microsoft.com/en-us/library/system.idisposable.aspx
        // Dispose(bool disposing) executes in two distinct scenarios. 
        // If disposing equals true, the method has been called directly 
        // or indirectly by a user's code. Managed and unmanaged resources 
        // can be disposed. 
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                // If disposing equals false, the method has been called by the 
                // runtime from inside the finalizer and you should not reference 
                // other objects. Only unmanaged resources can be disposed. 
                if (disposing)
                {
                    // Dispose managed resource
                    items.Dispose();
                }
                // Dispose unmanaged resource
                disposed = true;
            }
        }

        // http://msdn.microsoft.com/en-us/library/system.idisposable.aspx
        // Use C# destructor syntax for finalization code. 
        // This destructor will run only if the Dispose method 
        // does not get called. 
        // It gives your base class the opportunity to finalize. 
        // Do not provide destructors in types derived from this class.
        //~Extracter()
        //{
        //    Dispose(false);
        //}
    }
}
