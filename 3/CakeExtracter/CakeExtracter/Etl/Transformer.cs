using System;
using System.Collections.Generic;
using System.Linq;

namespace CakeExtracter.Etl
{
    
    public class Transformer<TIn, TOut> : Extracter<TOut>
        where TIn : class
        where TOut : class
    {
        // An extracter is passed in
        private readonly Extracter<TIn> extracter;

        // A loader is passed in
        private readonly Loader<TOut> loader;

        protected Transformer(Extracter<TIn> extracter, Loader<TOut> loader)
        {
            this.extracter = extracter;
            this.loader = loader;
        }

        public void Run()
        {
            // The loader thread is started, connecting it to the passed in 
            // extracter.
            var loaderThread = loader.Start(this);

            // The extracter thread is started
            var extracterThread = Start();

            loaderThread.Join();
            extracterThread.Join();
        }

        // The client calls Start() on the Transformer (which derives from Extracter),
        // which in turn causes this sealed implementation of Extract() to be called.
        //
        // Act as a pass through, selecting all items from the passed in extracter, 
        // and using the virtual function Transform<TIn> to convert it to a type that 
        // is finally passed to Add() and ultimately processed by the passed in loader.
        protected sealed override void Extract()
        {
            Add(extracter.EnumerateAll().Select(c => Transform(c)));
        }

        // Default transform just tries to cast input to output.
        // This should be overrided in derived class to do someting useful.
        protected virtual TOut Transform(TIn item)
        {
            return item as TOut;
        }
    }
}