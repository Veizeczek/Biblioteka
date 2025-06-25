// Services/CsvService.cs
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Biblioteka.Data.Entities;
using Biblioteka.Data.Repositories;

namespace Biblioteka.Services
{
    public class CsvService
    {
        private readonly BookRepository _bookRepo;

        public CsvService(BookRepository bookRepo)
        {
            _bookRepo = bookRepo;
        }

        public void ImportBooks(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Plik nie istnieje: {filePath}");

            var lines = File.ReadAllLines(filePath, Encoding.UTF8)
                            .Where(l => !string.IsNullOrWhiteSpace(l))
                            .ToList();
            if (lines.Count < 2)
                throw new Exception("Plik CSV musi zawierać nagłówek i co najmniej jeden wiersz danych.");

            // Parsujemy nagłówek, uwzględniając cudzysłowy i przecinki w polach
            var header = ParseCsvLine(lines[0]);

            // Wymagane kolumny (bez Id – CSV nie zawiera już kolumny Id)
            string[] required = { "Title", "Author", "ISBN" };
            foreach (var col in required)
                if (!header.Contains(col, StringComparer.OrdinalIgnoreCase))
                    throw new Exception($"Brak kolumny '{col}' w nagłówku CSV.");

            // Mapujemy każdą kolumnę na pozycję w tablicy fields
            var idx = header
                .Select((h, i) => new { Name = h.Trim(), Index = i })
                .ToDictionary(x => x.Name, x => x.Index, StringComparer.OrdinalIgnoreCase);

            var toImport = new List<Book>();
            for (int row = 1; row < lines.Count; row++)
            {
                var fields = ParseCsvLine(lines[row]);
                if (fields.Length != header.Length)
                    throw new Exception($"Nieprawidłowa liczba pól w wierszu {row + 1}.");

                string title  = fields[idx["Title"]].Trim();
                string author = fields[idx["Author"]].Trim();
                string isbn   = fields[idx["ISBN"]].Trim();
                if (string.IsNullOrEmpty(title) ||
                    string.IsNullOrEmpty(author) ||
                    string.IsNullOrEmpty(isbn))
                    throw new Exception($"Brak wymaganych danych w wierszu {row + 1} (Title, Author, ISBN).");

                var book = new Book
                {
                    Title     = title,
                    Author    = author,
                    ISBN      = isbn,
                    Publisher = TryGet(idx, fields, "Publisher")?.Trim(),
                    Year      = int.TryParse(TryGet(idx, fields, "Year"), out var y) ? y : (int?)null,
                    Pages     = int.TryParse(TryGet(idx, fields, "Pages"), out var p) ? p : (int?)null,
                    Genre     = TryGet(idx, fields, "Genre")?.Trim(),
                    Language  = TryGet(idx, fields, "Language")?.Trim()
                };
                toImport.Add(book);
            }

            // Wszystkie wiersze poprawne – dodajemy książki
            foreach (var b in toImport)
                _bookRepo.Add(b);
        }

        public void ExportBooks(string filePath)
        {
            var books = _bookRepo.GetAll().ToList();
            using var w = new StreamWriter(filePath, false, Encoding.UTF8);
            // Nagłówek bez kolumny Id
            w.WriteLine("Title,Author,ISBN,Publisher,Year,Pages,Genre,Language");
            foreach (var b in books)
            {
                w.WriteLine(string.Join(",",
                    Escape(b.Title),
                    Escape(b.Author),
                    Escape(b.ISBN),
                    Escape(b.Publisher),
                    b.Year?.ToString() ?? "",
                    b.Pages?.ToString() ?? "",
                    Escape(b.Genre),
                    Escape(b.Language)
                ));
            }
        }

        // Parsuje linię CSV z uwzględnieniem cudzysłowów i przecinków wewnątrz pól
        private static string[] ParseCsvLine(string line)
        {
            var result = new List<string>();
            var sb = new StringBuilder();
            bool inQuotes = false;
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        sb.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(sb.ToString());
                    sb.Clear();
                }
                else
                {
                    sb.Append(c);
                }
            }
            result.Add(sb.ToString());
            return result.ToArray();
        }

        private static string TryGet(Dictionary<string, int> idx, string[] fields, string column)
            => idx.TryGetValue(column, out var i) && i < fields.Length ? fields[i] : null;

        private static string Escape(string s)
            => string.IsNullOrEmpty(s)
               ? ""
               : (s.Contains(',') || s.Contains('"')
                  ? $"\"{s.Replace("\"", "\"\"")}\""
                  : s);
    }
}
