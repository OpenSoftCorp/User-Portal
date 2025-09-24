using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Data;

namespace UserPortal.Components
{
    public class SearchPopUpViewComponent:ViewComponent
    {
        Hashtable ht = new Hashtable();

        public async Task<IViewComponentResult> InvokeAsync(DataTable Model)
        {
            IViewComponentResult tr = await Task.FromResult((IViewComponentResult)View("SearchPopUp", Model));
            return tr;

        }
    }
}
