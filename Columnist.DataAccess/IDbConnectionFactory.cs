using System.Data;

namespace Columnist.DataAccess
{
    public interface IDbConnectionFactory
    {
        IDbConnection Create();
    }
}
