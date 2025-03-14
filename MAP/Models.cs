using System.Collections.Generic;

namespace ABSProject
{
    public class Book
    {
        public string id { get; set; }
        public Media media { get; set; }
        public string path { get; set; }
    }

    public class Media
    {
        public Metadata metadata { get; set; }
    }

    public class Metadata
    {
        public string title { get; set; }
        public string authorName { get; set; }
        public string seriesName { get; set; }
    }

    public class ApiResponse
    {
        public List<Book> results { get; set; }
    }

    public class LibrarySettings
    {
        public string Name { get; set; }
        public string ApiUrl { get; set; }
        public string ApiKey { get; set; }
        public string LibraryId { get; set; }
        public override string ToString() => Name;
    }

    public class BookStatus
    {
        public bool Processed { get; set; }
        public bool EBookUnavailable { get; set; }
        public bool AudiobookUnavailable { get; set; }
    }

    public class ConfigModel
    {
        public List<LibrarySettings> Libraries { get; set; } = new List<LibrarySettings>();
        public Dictionary<string, BookStatus> BookTracking { get; set; } = new Dictionary<string, BookStatus>();
    }

    public class ComparisonResult
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public string Series { get; set; }
        public string MissingVersion { get; set; }
        public double MatchScore { get; set; }
    }
}
