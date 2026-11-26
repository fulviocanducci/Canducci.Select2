using Canducci.Select2;
using Canducci.Select2.Extensions;
using Microsoft.AspNetCore.Mvc;
using WebAppSelect2.Interfaces;

namespace WebAppSelect2.Controllers
{
    public class UsersController(IUserApi userApi) : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Find(ISelectTerm selectTerm)
        {
            var listOfUser = await userApi.GetUsersAsync(selectTerm.Q);
            return Ok(new { results = listOfUser.Results.ToSelectItem(x => x.Id, x => x.Name) });
        }
    }
}
