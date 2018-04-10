using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

namespace CakeExtracter.Mef
{
    public class Composer<T>
    {
        public Composer(T obj)
        {
            Object = obj;
        }

        public void Compose()
        {
            AggregateCatalog = new AggregateCatalog();
            //AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(T).Assembly));
            AggregateCatalog.Catalogs.Add(new DirectoryCatalog(@".", "CakeExtracter.dll")); 
            ComposistionContainer = new CompositionContainer(AggregateCatalog);
            ComposistionContainer.ComposeParts(Object);
        }

        public T Object { get; set; }

        public CompositionContainer ComposistionContainer { get; set; }

        public AggregateCatalog AggregateCatalog { get; set; }
    }
}
