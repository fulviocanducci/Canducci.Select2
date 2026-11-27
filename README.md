# Canducci.Select2

[![NuGet](https://img.shields.io/nuget/v/Canducci.Select2.svg)](https://www.nuget.org/packages/Canducci.Select2)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Canducci.Select2.svg)](https://www.nuget.org/packages/Canducci.Select2)
[![Build and test](https://github.com/fulviocanducci/Canducci.Select2/actions/workflows/tests.yml/badge.svg)](https://github.com/fulviocanducci/Canducci.Select2/actions/workflows/tests.yml)

**Canducci.Select2** provides ASP.NET Core MVC model binding for Select2 request parameters and extension methods to convert IEnumerable collections and asynchronous IQueryable queries into Select2-compatible items.

> **Package version:** the project is currently configured for version **1.0.1**. This version is a project/repository update; it is not considered published on NuGet.org until the package is actually pushed. The NuGet badges above show live package information after publication.

## Version update — v1.0.1

This update introduces factory methods for creating `SelectItemValues<TId>` instances and updates the asynchronous query documentation and examples to use the current API.

### What's new

- Adds `SelectItemValues<TId>.Create(IEnumerable<TId> selectedValues)` to create an options instance containing selected identifiers.
- Adds `SelectItemValues<TId>.Create(IEnumerable<TId> selectedValues, IEnumerable<TId> disabledValues)` to create an options instance containing both selected and disabled identifiers.
- Keeps `SetSelectValues` and `SetDisabledValues` available for fluent configuration.
- Updates asynchronous `IQueryable<T>` examples to pass a `SelectItemValues<TId>` object instead of passing selected and disabled identifier collections as separate arguments.
- Adds unit tests for both factory overloads and updates asynchronous and Entity Framework Core InMemory tests to exercise the factory methods.

### Creating SelectItemValues

Create an instance with selected identifiers only:

~~~csharp
SelectItemValues<int> options = SelectItemValues<int>.Create(
    new[] { 1, 2, 3 });
~~~

Create an instance with selected and disabled identifiers:

~~~csharp
SelectItemValues<int> options = SelectItemValues<int>.Create(
    new[] { 1, 2, 3 },
    new[] { 4, 5 });
~~~

The first argument always represents selected identifiers. The optional second argument represents disabled identifiers. The factory methods assign the supplied collections to the corresponding properties; they do not execute a query or change the source collection.

## Features

- Automatic model binding for ISelectTerm in ASP.NET Core MVC.
- Reads the Select2 request values term, q, _type and page.
- Converts in-memory IEnumerable collections to SelectItem.
- Converts asynchronous IQueryable sources with ToSelectItemAsync.
- Supports selected and disabled items by identifier collections or Boolean expressions.
- Provides SelectItemValues<TId> factory methods for reusable selected/disabled identifier configuration in asynchronous IQueryable conversion.
- Leaves filtering, ordering and pagination under the application's control.

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

## 4. Convert an asynchronous IQueryable query

Use ToSelectItemAsync when working with an asynchronous query provider, such as Entity Framework Core. The query is asynchronously enumerated and the result is returned as Task<List<SelectItem<TId>>>.

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

        return Json(new
        {
            results,
            pagination = new
            {
                more = results.Count == pageSize
            }
        });
    }
}
~~~

The example applies the application's own filtering and pagination. ToSelectItemAsync only converts the items returned by that query; it does not decide how many records belong on a page or whether another page exists.

> **Important:** do not call ToList() or ToListAsync() before ToSelectItemAsync when you want asynchronous query enumeration. Calling ToList() first turns the operation into an in-memory collection workflow.

### 4.1. Select and disable by identifier collections with SelectItemValues

For asynchronous `IQueryable<T>` conversion, selected and disabled identifiers are supplied through a `SelectItemValues<TId>` instance. This options object groups the two collections into a single argument.

**Select items only:**

~~~csharp
SelectItemValues<int> options = SelectItemValues<int>.Create(
    new[] { 10, 20 });

List<SelectItem<int>> items = await query.ToSelectItemAsync(
    user => user.Id,
    user => user.Name,
    options);
~~~

**Select and disable items:**

~~~csharp
SelectItemValues<int> options = SelectItemValues<int>.Create(
    new[] { 10, 20 },
    new[] { 30 });

List<SelectItem<int>> items = await query.ToSelectItemAsync(
    user => user.Id,
    user => user.Name,
    options);
~~~

The first collection contains identifiers to select; the second contains identifiers to disable. An identifier may be present in both collections, in which case the resulting item is both selected and disabled.

You can also configure the object using the fluent methods:

~~~csharp
SelectItemValues<int> options = new SelectItemValues<int>()
    .SetSelectValues(new[] { 10, 20 })
    .SetDisabledValues(new[] { 30 });
~~~

Or use the configuration callback overload:

~~~csharp
List<SelectItem<int>> items = await query.ToSelectItemAsync(
    user => user.Id,
    user => user.Name,
    options =>
    {
        options.SetSelectValues(new[] { 10, 20 });
        options.SetDisabledValues(new[] { 30 });
    });
~~~

The callback receives a new `SelectItemValues<TId>` instance. Configure whichever collections are needed; leaving one collection unset means that no items will receive that state.

### 4.2. Select and disable using Boolean expressions

~~~csharp
List<SelectItem<int>> items = await query.ToSelectItemAsync(
    user => user.Id,
    user => user.Name,
    user => user.IsSelected,
    user => user.IsDisabled);
~~~

To use only the selected expression:

~~~csharp
List<SelectItem<int>> items = await query.ToSelectItemAsync(
    user => user.Id,
    user => user.Name,
    user => user.IsSelected);
~~~

### 4.3. Async provider requirement

ToSelectItemAsync requires the source query to implement IAsyncEnumerable. Entity Framework Core queries support asynchronous enumeration. A plain List.AsQueryable() does not provide an asynchronous query source and will throw InvalidOperationException.

If your data is already in memory, use the synchronous ToSelectItem overload instead.

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
| ToSelectItemAsync(value, description) | Asynchronously converts query results. |
| ToSelectItemAsync(value, description, selectedValue) | Asynchronously converts and selects one identifier. |
| ToSelectItemAsync(value, description, selected) | Determines selection with a Boolean expression. |
| ToSelectItemAsync(value, description, selected, disabled) | Determines both states with Boolean expressions. |
| ToSelectItemAsync(value, description, options) | Determines selected and disabled states using SelectItemValues<TId>. |
| ToSelectItemAsync(value, description, configureOptions) | Configures selected and disabled identifiers through an Action<SelectItemValues<TId>> callback. |

All conversion methods return List<SelectItem<TId>> (or a task containing that list for asynchronous methods).

## 6. Package information

| Property | Value |
| --- | --- |
| Package | Canducci.Select2 |
| Current project version | 1.0.1 |
| Target frameworks | .NET 8, .NET 9, .NET 10 and .NET 11 |
| Repository | [GitHub](https://github.com/fulviocanducci/Canducci.Select2) |

Live package information:

- [NuGet package page](https://www.nuget.org/packages/Canducci.Select2)
- [NuGet version history and download statistics](https://www.nuget.org/packages/Canducci.Select2#versions-body-tab)

## License

The repository's license has not yet been specified. Add the chosen license before publishing the package.

## Contributing

Issues and pull requests are welcome at [github.com/fulviocanducci/Canducci.Select2](https://github.com/fulviocanducci/Canducci.Select2).
