using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Data;
using UserPortal.Models.Entities;

namespace UserPortal.Service
{
    public interface IUserRegistrationService
    {
        public string SaveUser(Hashtable ht);

        public List<UserRegistrationInfo> GetUser(Hashtable ht);

    }
}
