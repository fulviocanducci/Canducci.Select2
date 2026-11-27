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
                .OrderBy(c => c.Id)
                .Take(10)
                .ToSelectItemAsync(x => x.Id, x => x.Name);



            //dataContext.Sources
            //    .AsNoTracking()
            //    .ToSelectItem(x => x.Id, x => x.Name, new[] { 1, 2 });

            //dataContext.Sources
            //    .AsNoTracking()
            //    .ToSelectItem(x => x.Id, x => x.Name, new[] { 1, 2 }, new[] { 1, 3 });

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
            return Ok(s.ToSelect2());
        }
    }
}
