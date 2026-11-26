using Refit;
using WebAppSelect2.Models;

namespace WebAppSelect2.Interfaces
{
    public interface IUserApi
    {
        [Get("/users/search")]
        Task<Response<User>> GetUsersAsync(string q);
    }
}
