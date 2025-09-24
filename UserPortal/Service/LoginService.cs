using System.Collections;
using System.Data;
using UserPortal.Reposiratory;

namespace UserPortal.Service
{
    public class LoginService : ILoginService
    {
        LoginRepo objLoginRepo;


        public LoginService(LoginRepo loginRepo)
        {
            objLoginRepo = loginRepo;
        }

        Hashtable ht = new Hashtable();

        public DataTable ValidateUser(string userName, string password)
        {
            ht.Clear();
            ht.Add("userName", userName);
            ht.Add("password", password);
            return objLoginRepo.ValidateUser(ht);
        }
        public string LogInHistory(string Code, string Name, string SessionId)
        {
            ht.Clear();
            ht.Add("Code", Code);
            ht.Add("Name", Name);
            ht.Add("LogInLogOut", "In");
            ht.Add("SessionId", SessionId);
            return objLoginRepo.LogInHistory(ht);

        }

        
    }
}
