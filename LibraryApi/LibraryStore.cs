namespace LibraryApi;

public class LibraryStore
{
    private readonly object _lock = new();
    private readonly List<Book> _books;
    private readonly List<Loan> _loans = new();
    private int _nextBookId;
    private int _nextLoanId = 1;

    public LibraryStore()
    {
        (string Title, string Author, string Isbn, int Year, int Copies)[] seed =
        [
            ("Pride and Prejudice", "Jane Austen", "9780141439518", 1813, 3),
            ("1984", "George Orwell", "9780451524935", 1949, 4),
            ("To Kill a Mockingbird", "Harper Lee", "9780061120084", 1960, 2),
            ("The Great Gatsby", "F. Scott Fitzgerald", "9780743273565", 1925, 3),
            ("Moby-Dick", "Herman Melville", "9781503280786", 1851, 1),
            ("War and Peace", "Leo Tolstoy", "9781400079988", 1869, 2),
            ("The Catcher in the Rye", "J.D. Salinger", "9780316769488", 1951, 3),
            ("The Hobbit", "J.R.R. Tolkien", "9780547928227", 1937, 5),
            ("Brave New World", "Aldous Huxley", "9780060850524", 1932, 2),
            ("Jane Eyre", "Charlotte Bronte", "9780141441146", 1847, 2),
            ("Wuthering Heights", "Emily Bronte", "9780141439556", 1847, 1),
            ("Crime and Punishment", "Fyodor Dostoevsky", "9780140449136", 1866, 2),
            ("The Odyssey", "Homer", "9780140268867", -700, 3),
            ("Frankenstein", "Mary Shelley", "9780141439471", 1818, 2),
            ("Dracula", "Bram Stoker", "9780141439846", 1897, 2),
            ("Fahrenheit 451", "Ray Bradbury", "9781451673319", 1953, 4),
            ("Lord of the Flies", "William Golding", "9780399501487", 1954, 3),
            ("Animal Farm", "George Orwell", "9780451526342", 1945, 4),
            ("The Alchemist", "Paulo Coelho", "9780061122415", 1988, 3),
            ("Don Quixote", "Miguel de Cervantes", "9780060934347", 1605, 1),
            ("Dune", "Frank Herbert", "9780441172719", 1965, 3),
            ("Neuromancer", "William Gibson", "9780441569595", 1984, 2),
            ("The Handmaid's Tale", "Margaret Atwood", "9780385490818", 1985, 2),
            ("Beloved", "Toni Morrison", "9781400033416", 1987, 1),
            ("One Hundred Years of Solitude", "Gabriel Garcia Marquez", "9780060883287", 1967, 2),
        ];
        _books = seed.Select((s, i) => new Book
        {
            Id = i + 1, Title = s.Title, Author = s.Author, Isbn = s.Isbn, Year = s.Year, CopiesAvailable = s.Copies
        }).ToList();
        _nextBookId = _books.Count + 1;
    }

    public IReadOnlyList<Book> GetBooks(int page, int size)
    {
        lock (_lock)
        {
            // BUG (planted for demo): pages are 1-based but Skip uses page * size
            return _books.OrderBy(b => b.Id).Skip(page * size).Take(size).Select(Clone).ToList();
        }
    }

    public Book? GetBook(int id)
    {
        lock (_lock) return _books.FirstOrDefault(b => b.Id == id) is { } b ? Clone(b) : null;
    }

    public Book AddBook(Book book)
    {
        lock (_lock)
        {
            book.Id = _nextBookId++;
            _books.Add(book);
            return Clone(book);
        }
    }

    public (Loan? Loan, string? Error) CreateLoan(int bookId, string member)
    {
        lock (_lock)
        {
            var book = _books.FirstOrDefault(b => b.Id == bookId);
            if (book is null) return (null, "not_found");
            if (book.CopiesAvailable <= 0) return (null, "no_copies");
            book.CopiesAvailable--;
            var now = DateTime.UtcNow;
            var loan = new Loan { Id = _nextLoanId++, BookId = bookId, Member = member, LoanedAt = now, DueAt = now.AddDays(14) };
            _loans.Add(loan);
            return (loan, null);
        }
    }

    public (Loan? Loan, string? Error) ReturnLoan(int loanId)
    {
        lock (_lock)
        {
            var loan = _loans.FirstOrDefault(l => l.Id == loanId);
            if (loan is null) return (null, "not_found");
            if (loan.ReturnedAt is not null) return (null, "already_returned");
            loan.ReturnedAt = DateTime.UtcNow;
            _books.First(b => b.Id == loan.BookId).CopiesAvailable++;
            return (loan, null);
        }
    }

    private static Book Clone(Book b) => new()
    {
        Id = b.Id, Title = b.Title, Author = b.Author, Isbn = b.Isbn, Year = b.Year, CopiesAvailable = b.CopiesAvailable
    };
}
