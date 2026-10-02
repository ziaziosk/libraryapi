namespace LibraryApi;

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Author { get; set; } = "";
    public string Isbn { get; set; } = "";
    public int Year { get; set; }
    public int CopiesAvailable { get; set; }
}

public class Loan
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public string Member { get; set; } = "";
    public DateTime LoanedAt { get; set; }
    public DateTime DueAt { get; set; }
    public DateTime? ReturnedAt { get; set; }
}

public record CreateLoanRequest(int BookId, string? Member);
