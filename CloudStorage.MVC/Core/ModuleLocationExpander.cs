using Microsoft.AspNetCore.Mvc.Razor;

namespace CloudStorage.MVC.Core
{
    public class ModuleLocationExpander:IViewLocationExpander
    {
        public void PopulateValues(ViewLocationExpanderContext context)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            return new []{"/Modules/{1}/{0}.cshtml",}
        }
    }
}
