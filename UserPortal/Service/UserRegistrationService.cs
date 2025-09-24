using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Data;
using UserPortal.Models.Entities;
using UserPortal.Reposiratory;

namespace UserPortal.Service
{
    public class UserRegistrationService : IUserRegistrationService
    {
        UserRegistrationRepo _objReg_Repo;

        List<UserRegistrationInfo> objUserInfo = new List<UserRegistrationInfo>();
        public UserRegistrationService(UserRegistrationRepo regrepo) 
        {
            _objReg_Repo = regrepo;
        }
        public string SaveUser(Hashtable ht)
        {
            return _objReg_Repo.SaveUser(ht);
        }

        public List<UserRegistrationInfo> GetUser(Hashtable ht)
        {
            DataTable dt = _objReg_Repo.GetUser(ht);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                objUserInfo.Add(new UserRegistrationInfo()
                {
                    UserCode = dt.Rows[i]["Code"].ToString(),
                    UserName = dt.Rows[i]["Name"].ToString(),
                    MobileNo = dt.Rows[i]["MobileNo"].ToString(),
                    Email = dt.Rows[i]["Email"].ToString(),
                    Designation = dt.Rows[i]["Designation"].ToString(),
                    Status = dt.Rows[i]["Status"].ToString(),
                    UserType = dt.Rows[i]["UserType"].ToString()

                });
            }

            return objUserInfo;


        }
    }
}
