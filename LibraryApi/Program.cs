using LibraryApi;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<LibraryStore>();
var app = builder.Build();

app.MapGet("/books", (LibraryStore store, int? page, int? size) =>
    Results.Ok(store.GetBooks(page ?? 1, size ?? 10)));

app.MapGet("/books/{id:int}", (LibraryStore store, int id) =>
    store.GetBook(id) is { } book ? Results.Ok(book) : Results.NotFound());

app.MapPost("/books", (LibraryStore store, Book book) =>
{
    var created = store.AddBook(book);
    return Results.Created($"/books/{created.Id}", created);
});

app.MapPost("/loans", (LibraryStore store, CreateLoanRequest request) =>
{
    var (loan, error) = store.CreateLoan(request.BookId, request.Member ?? "");
    return error switch
    {
        "not_found" => Results.NotFound(),
        "no_copies" => Results.Problem("No copies available.", statusCode: 409),
        _ => Results.Created($"/loans/{loan!.Id}", loan)
    };
});

app.MapPost("/loans/{id:int}/return", (LibraryStore store, int id) =>
{
    var (loan, error) = store.ReturnLoan(id);
    return error switch
    {
        "not_found" => Results.NotFound(),
        "already_returned" => Results.Problem("Loan already returned.", statusCode: 409),
        _ => Results.Ok(loan)
    };
});

app.Run();

public partial class Program { }
