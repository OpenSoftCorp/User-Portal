using System.Collections;
using UserPortal.Models.Entities;

namespace UserPortal.Service
{
    public interface IMenuService
    {
        public List<MenuesInfo> GetMenues(Hashtable ht);


    }
}
