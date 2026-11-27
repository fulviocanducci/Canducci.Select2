using Canducci.Select2;
using Canducci.Select2.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAppSelect2.Interfaces;
using WebAppSelect2.Models;

namespace WebAppSelect2.Controllers
{
    public class UsersController(IUserApi userApi, DataContext dataContext) : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Find(ISelectTerm selectTerm)
        {
            var s = await dataContext
                .Sources
                .AsNoTracking()
                .ToSelectItemAsync(x => x.Id, x => x.Name);

            //var s = await dataContext
            //    .Sources
            //    .AsNoTracking()
            //    .ToSelectItemAsync<Source, long>(config =>
            //    {
            //        config.Value = x => x.Id;
            //        config.Description = x => x.Name;
            //        config.
            //    });


            var listOfUser = await userApi.GetUsersAsync(selectTerm.Q);
            var a = Enumerable.Empty<int>();
            var b = Enumerable.Empty<int>();
            return Ok(new { results = listOfUser.Results.ToSelectItem(x => x.Id, x => x.Name, a, b) });
        }
    }
}
