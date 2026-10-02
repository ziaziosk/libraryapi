using System.Net;
using System.Net.Http.Json;
using LibraryApi;
using Microsoft.AspNetCore.Mvc.Testing;

namespace LibraryApi.Tests;

public class LibraryApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public LibraryApiTests(WebApplicationFactory<Program> factory) => _client = factory.CreateClient();

    [Fact]
    public async Task GetBook_ReturnsSeededBook()
    {
        var book = await _client.GetFromJsonAsync<Book>("/books/1");
        Assert.Equal("Pride and Prejudice", book!.Title);
    }

    [Fact]
    public async Task GetBook_Unknown_Returns404()
    {
        var response = await _client.GetAsync("/books/9999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PostBook_Creates()
    {
        var response = await _client.PostAsJsonAsync("/books",
            new Book { Title = "New", Author = "A", Isbn = "9780306406157", Year = 2020, CopiesAvailable = 1 });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Loan_And_Return_Flow()
    {
        var loanResponse = await _client.PostAsJsonAsync("/loans", new { BookId = 2, Member = "alice" });
        Assert.Equal(HttpStatusCode.Created, loanResponse.StatusCode);
        var loan = await loanResponse.Content.ReadFromJsonAsync<Loan>();

        var ret = await _client.PostAsync($"/loans/{loan!.Id}/return", null);
        Assert.Equal(HttpStatusCode.OK, ret.StatusCode);
        var again = await _client.PostAsync($"/loans/{loan.Id}/return", null);
        Assert.Equal(HttpStatusCode.Conflict, again.StatusCode);
    }

    [Fact]
    public async Task Loan_WithNoCopies_Returns409()
    {
        await _client.PostAsJsonAsync("/loans", new { BookId = 5, Member = "a" });
        var response = await _client.PostAsJsonAsync("/loans", new { BookId = 5, Member = "b" });
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
