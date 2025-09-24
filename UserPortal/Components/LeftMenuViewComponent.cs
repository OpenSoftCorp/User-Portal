using Microsoft.AspNetCore.Mvc;
using System.Collections;
using UserPortal.Models.Entities;
using UserPortal.Service;

namespace UserPortal.Components
{
    public class LeftMenuViewComponent:ViewComponent
    {
        private readonly IMenuService obMenues;
        Hashtable ht = new Hashtable();

        public LeftMenuViewComponent(IMenuService menuService)
        {
            obMenues=menuService;
        }
        public async Task<IViewComponentResult> InvokeAsync(string Code, string Role)
        {

            ht.Clear();
            ht.Add("Code", Code);
            ht.Add("Role", Role);
            var Model = obMenues.GetMenues(ht);
            IViewComponentResult ivc = await Task.FromResult((IViewComponentResult)View("LeftMenu", Model));
            return ivc;
        }
    }
}
