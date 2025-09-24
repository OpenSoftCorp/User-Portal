using System.Collections;
using System.Data;

namespace UserPortal.Service
{
    public interface ILoginService
    {
        DataTable ValidateUser(string userName,string password);
        string LogInHistory(string Code, string Name, string SessionId);

    }
}
