# Canducci.Select2

[![NuGet](https://img.shields.io/nuget/v/Canducci.Select2.svg)](https://www.nuget.org/packages/Canducci.Select2)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Canducci.Select2.svg)](https://www.nuget.org/packages/Canducci.Select2)
[![Build and test](https://github.com/fulviocanducci/Canducci.Select2/actions/workflows/tests.yml/badge.svg)](https://github.com/fulviocanducci/Canducci.Select2/actions/workflows/tests.yml)

**Canducci.Select2** provides ASP.NET Core MVC model binding for Select2 request parameters and extensions to convert in-memory collections and synchronous or asynchronous `IQueryable<T>` queries into Select2-compatible results.

> **Release version:** this repository is being prepared for **v2.0.1**. The release workflow publishes the NuGet package when the `v2.0.1` tag is pushed and its checks succeed. The NuGet badges show the published package state.

## What's new in v2.0.1

- Adds synchronous `IQueryable<T>.ToSelectItem` overloads.
- Adds identifier-collection overloads for synchronous and asynchronous `IQueryable<T>` conversions.
- Adds `ToSelect2()` to wrap converted items in a `{ results: [...] }` response.
- Keeps the existing `IEnumerable<T>` conversion API.

## Features

- Automatic model binding for `ISelectTerm` in ASP.NET Core MVC.
- Reads the Select2 request values `term`, `q`, `_type` and `page`.
- Converts in-memory `IEnumerable<T>` collections to `SelectItem<TId>`.
- Converts `IQueryable<T>` sources synchronously with `ToSelectItem` or asynchronously with `ToSelectItemAsync`.
- Supports selected and disabled states using Boolean expressions or identifier collections.
- Wraps a list in the Select2 `{ results: [...] }` response shape with `ToSelect2()`.
- Leaves filtering, ordering, page size and pagination decisions under the application's control.

## Requirements

- ASP.NET Core MVC.
- .NET 8.0 or later.
- The asynchronous IQueryable overload requires a query provider that implements IAsyncEnumerable (for example, Entity Framework Core).

## Installation

~~~bash
dotnet add package Canducci.Select2
~~~

Or use the NuGet Package Manager:

~~~powershell
Install-Package Canducci.Select2
~~~

## 1. Configure the application

Register Select2 before MVC:

~~~csharp
using Canducci.Select2.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSelect2();
builder.Services.AddControllersWithViews();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
~~~

AddSelect2 registers the SelectTermModelBinderProvider in MVC options. The provider recognizes action parameters declared as ISelectTerm and uses SelectTermModelBinder to create a SelectTerm instance.

Call AddSelect2 before AddControllersWithViews so the provider is added as part of the MVC options configuration.

## 2. Receive Select2 parameters with ISelectTerm

Use ISelectTerm directly as an action parameter. You do not need to instantiate SelectTerm or add a binder attribute when AddSelect2 is registered.

~~~csharp
using Canducci.Select2;
using Microsoft.AspNetCore.Mvc;

namespace MyApplication.Controllers
{
    public class UsersController : Controller
    {
        [HttpGet]
        public IActionResult Search(ISelectTerm selectTerm)
        {
            string term = selectTerm.Q ?? selectTerm.Term ?? string.Empty;

            // Apply your own filtering and pagination here.
            return Json(new
            {
                search = term,
                selectTerm.Type,
                selectTerm.Page
            });
        }
    }
}
~~~

The binder maps the following request values:

| Select2 request value | ISelectTerm property | Meaning |
| --- | --- | --- |
| term | Term | Search term sent by Select2 |
| q | Q | Query text |
| _type | Type | Select2 request type |
| page | Page | Requested page number |

For example:

~~~text
GET /Users/Search?term=ana&q=ana&_type=query&page=2
~~~

The action receives an ISelectTerm with Term = "ana", Q = "ana", Type = "query" and Page = 2.

Missing string values are null. If page is missing or cannot be parsed as an integer, Page is 0.

The library only binds these values. It does **not** apply filtering, sorting, page size, Skip, Take, or Select2's pagination.more calculation. Those rules belong to the application.

## 3. Convert an IEnumerable collection

Use ToSelectItem when you already have an in-memory collection, such as a List, an array, or the result of a synchronous query.

The extension maps each source object to SelectItem:

- Id: identifier sent to Select2.
- Text: text displayed in the dropdown.
- Selected: whether the item is selected.
- Disabled: whether the item is disabled.

Assume this model:

~~~csharp
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsSelected { get; set; }
    public bool IsDisabled { get; set; }
}
~~~

### 3.1. Basic conversion

~~~csharp
IEnumerable<User> users = new[]
{
    new User { Id = 1, Name = "Ana" },
    new User { Id = 2, Name = "Bruno" },
    new User { Id = 3, Name = "Carlos" }
};

List<SelectItem<int>> items = users.ToSelectItem(
    user => user.Id,
    user => user.Name);
~~~

No item is selected or disabled by default.

### 3.2. Select one item by identifier

~~~csharp
List<SelectItem<int>> items = users.ToSelectItem(
    user => user.Id,
    user => user.Name,
    selectedValue: 2);
~~~

Only the item whose identifier is 2 is marked as selected.

### 3.3. Select and disable items using identifier collections

The synchronous `IEnumerable<T>` overload continues to accept the selected and disabled collections directly:

~~~csharp
int[] selectedIds = { 1, 3 };
int[] disabledIds = { 2, 3 };

List<SelectItem<int>> items = users.ToSelectItem(
    user => user.Id,
    user => user.Name,
    selectedIds,
    disabledIds);
~~~

The resulting states are:

| Id | Text | Selected | Disabled |
| --- | --- | --- | --- |
| 1 | Ana | true | false |
| 2 | Bruno | false | true |
| 3 | Carlos | true | true |

Both collections are optional. Passing null for them means that no items are selected or disabled.

### 3.4. Select and disable using Boolean expressions

When the source model already contains the state, use the expression overloads:

~~~csharp
List<SelectItem<int>> items = users.ToSelectItem(
    user => user.Id,
    user => user.Name,
    user => user.IsSelected,
    user => user.IsDisabled);
~~~

You can also provide only the selected expression:

~~~csharp
List<SelectItem<int>> items = users.ToSelectItem(
    user => user.Id,
    user => user.Name,
    user => user.IsSelected);
~~~

In this case, Disabled remains false for every item.

## 4. Convert an IQueryable query

Use `ToSelectItemAsync` with an asynchronous query provider, such as Entity Framework Core, to enumerate without blocking the calling thread. The result is a `Task<List<SelectItem<TId>>>`.

Keep filtering, ordering and pagination in the query **before** calling ToSelectItemAsync.

~~~csharp
using Canducci.Select2;
using Canducci.Select2.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class UsersController : Controller
{
    private readonly ApplicationDbContext _context;

    public UsersController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Search(ISelectTerm selectTerm)
    {
        string term = selectTerm.Q ?? selectTerm.Term ?? string.Empty;
        int page = selectTerm.Page < 1 ? 1 : selectTerm.Page;
        const int pageSize = 20;

        IQueryable<User> query = _context.Users
            .AsNoTracking()
            .Where(user => user.Name.Contains(term))
            .OrderBy(user => user.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        List<SelectItem<int>> results = await query.ToSelectItemAsync(
            user => user.Id,
            user => user.Name);

        return Ok(results.ToSelect2());
    }
}
~~~

The example applies the application's own filtering and pagination to the query. `ToSelectItemAsync` converts the selected rows, and `ToSelect2()` wraps them as `{ "results": [...] }`. Pagination metadata is not added by the library.

> **Important:** do not call ToList() or ToListAsync() before ToSelectItemAsync when you want asynchronous query enumeration. Calling ToList() first turns the operation into an in-memory collection workflow.

### 4.1. Select using a Boolean expression

~~~csharp
List<SelectItem<int>> items = await query.ToSelectItemAsync(
    user => user.Id,
    user => user.Name,
    user => user.IsSelected);
~~~

### 4.2. Select and disable using Boolean expressions

~~~csharp
List<SelectItem<int>> items = await query.ToSelectItemAsync(
    user => user.Id,
    user => user.Name,
    user => user.IsSelected,
    user => user.IsDisabled);
~~~

### 4.3. Select and disable using identifier collections

Pass selected IDs as the third argument; optionally pass disabled IDs as the fourth. The ID collection element type must match the type returned by the `value` expression:

~~~csharp
List<SelectItem<long>> selectedItems = await query.ToSelectItemAsync(
    source => source.Id,
    source => source.Name,
    new long[] { 1, 3 });

List<SelectItem<long>> selectedAndDisabledItems = await query.ToSelectItemAsync(
    source => source.Id,
    source => source.Name,
    new long[] { 1, 3 },
    new long[] { 2, 3 });
~~~

The collections become `Contains` predicates in the query projection. With no disabled collection, `Disabled` is false for every result.

### 4.4. Async provider requirement

ToSelectItemAsync requires the source query to implement IAsyncEnumerable. Entity Framework Core queries support asynchronous enumeration. A plain List.AsQueryable() does not provide an asynchronous query source and will throw InvalidOperationException.

If your data is already in memory, use the synchronous `IEnumerable<T>` overload instead.

### 4.5. Synchronous IQueryable conversion

Use `ToSelectItem` when synchronous enumeration is required. The projection is composed into the `IQueryable` and then synchronously enumerated. Apply filtering, ordering and pagination before conversion.

~~~csharp
List<SelectItem<long>> selected = query.ToSelectItem(
    source => source.Id,
    source => source.Name,
    selectedValue: 1L);

List<SelectItem<long>> selectedAndDisabled = query.ToSelectItem(
    source => source.Id,
    source => source.Name,
    selectedValues: new long[] { 1, 2 },
    disabledValues: new long[] { 1, 3 });
~~~

Boolean expressions are also supported when the state comes from the source model:

~~~csharp
List<SelectItem<long>> items = query.ToSelectItem(
    source => source.Id,
    source => source.Name,
    source => source.Selected,
    source => source.Disabled);
~~~

Unlike `ToSelectItemAsync`, synchronous enumeration does not require the provider to implement `IAsyncEnumerable`, but synchronous database calls block the calling thread. Prefer the async overloads for request handling when the provider supports asynchronous enumeration.

## 5. Available extension methods

### IEnumerable

| Method | Description |
| --- | --- |
| ToSelectItem(value, description) | Converts items without selected or disabled states. |
| ToSelectItem(value, description, selectedValue) | Selects one item by identifier. |
| ToSelectItem(value, description, selected) | Determines selection with a Boolean expression. |
| ToSelectItem(value, description, selected, disabled) | Determines both states with Boolean expressions. |
| ToSelectItem(value, description, selectedValues, disabledValues) | Determines states using identifier collections. |

### IQueryable

| Method | Description |
| --- | --- |
| ToSelectItem(value, description) | Synchronously converts query results. |
| ToSelectItem(value, description, selectedValue) | Selects one item by identifier. |
| ToSelectItem(value, description, selected) | Synchronously determines selection with a Boolean expression. |
| ToSelectItem(value, description, selected, disabled) | Synchronously determines both states with Boolean expressions. |
| ToSelectItem(value, description, selectedValues, disabledValues) | Determines states using identifier collections. |
| ToSelectItemAsync(value, description) | Asynchronously converts query results. |
| ToSelectItemAsync(value, description, selected) | Asynchronously determines selection with a Boolean expression. |
| ToSelectItemAsync(value, description, selected, disabled) | Asynchronously determines both states with Boolean expressions. |
| ToSelectItemAsync(value, description, selectedValues, disabledValues) | Asynchronously determines states using identifier collections; disabledValues is optional. |

All conversion methods return `List<SelectItem<TId>>` (or a task containing that list for asynchronous methods). To return the Select2 response shape without changing those methods, wrap the list with `ToSelect2()`:

~~~csharp
List<SelectItem<long>> results = await query.ToSelectItemAsync(
    item => item.Id,
    item => item.Name);

return Ok(results.ToSelect2());
~~~

The wrapper serializes as `{ "results": [...] }` with ASP.NET Core's default JSON settings. It contains only `results`; pagination metadata is not included.

## 6. Package information

| Property | Value |
| --- | --- |
| Package | Canducci.Select2 |
| Release version configured in this repository | 2.0.1 |
| Target frameworks | .NET 8, .NET 9, .NET 10 and .NET 11 |
| Repository | [GitHub](https://github.com/fulviocanducci/Canducci.Select2) |

Live package information:

- [NuGet package page](https://www.nuget.org/packages/Canducci.Select2)
- [NuGet version history and download statistics](https://www.nuget.org/packages/Canducci.Select2#versions-body-tab)

## License

The repository's license has not yet been specified. Add the chosen license before publishing the package.

## Contributing

Issues and pull requests are welcome at [github.com/fulviocanducci/Canducci.Select2](https://github.com/fulviocanducci/Canducci.Select2).
