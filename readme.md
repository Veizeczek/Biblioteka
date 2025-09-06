# 📚 Aplikacja Biblioteka

Temat: **System zarządzania biblioteką** — desktopowa aplikacja w języku **C#** z lokalną bazą danych **SQLite**.  
Umożliwia dodawanie książek i użytkowników, zarządzanie wypożyczeniami oraz przeglądanie historii operacji. Posiada wersje kliencka jak i administratora, z przyjaznym UI.

---

## 📑 Spis treści

1. [Funkcjonalności](#-funkcjonalności)  
2. [Technologie](#-technologie)  
3. [Wymagania](#-wymagania)  
4. [Instalacja](#-instalacja)  
5. [Schemat bazy danych](#-schemat-bazy-danych)  
6. [Bezpieczeństwo](#-bezpieczeństwo)  
7. [Kontrybucja](#-kontrybucja)  
8. [Licencja](#-licencja)   

---

## ✅ Funkcjonalności

| Moduł                  | Opis                                                                                       |
|------------------------|--------------------------------------------------------------------------------------------|
| Zarządzanie książkami  | Dodawanie, edycja, usuwanie oraz przeglądanie książek dostępnych w zasobach biblioteki     |
| Użytkownicy            | Rejestracja, edycja i zarządzanie danymi czytelników oraz ich kontami                      |
| Wypożyczenia           | Obsługa procesu wypożyczeń i zwrotów książek przez użytkowników                            |
| Historia operacji      | Przegląd i filtrowanie historii wypożyczeń według dat, użytkowników lub pozycji            |
| Walidacja danych       | Wbudowana walidacja formularzy: puste pola, długości, formaty, unikalność danych itp.      |
| Import / Eksport danych| Hurtowe dodawanie książek i użytkowników z plików (np. CSV) oraz tworzenie kopii zapasowych z zachowaniem walidacji danych i spójności systemu |
| GUI                    | Intuicyjny, nowoczesny interfejs graficzny zbudowany w technologii WPF                     |



---

## 🧰 Technologie

| Warstwa  | Technologie                           |
|----------|----------------------------------------|
| Backend  | C# (.NET 7 / .NET Framework)           |
| GUI      | WPF                       |
| Baza danych | SQLite |
| Dev tools | Visual Studio / JetBrains Rider       |

---

## 🖥️ Wymagania

- System: Windows 10 / 11  
- .NET SDK: 7.0 lub wyższy  
- Visual Studio 2022+ lub JetBrains Rider  
- SQLite Browser (opcjonalnie – do podglądu bazy)

---

## 🚀 Instalacja

1. Sklonuj repozytorium lub pobierz ZIP:
   ```bash
   git clone https://github.com/Veizeczek/Biblioteka
   ```
2. Otwórz plik `.sln` w Visual Studio / Rider  
3. Przywróć paczki NuGet:
   ```bash
   dotnet restore
   ```
4. Pierwsze uruchomienie:
 w Pliku appsettings.json znajduje się plik konfiguracyjny, który pozwala na stworzenie przykładowej bazy danych(domyślne).
5. Uruchom aplikację (`F5`)

Baza danych `biblioteka.db` zostanie utworzona automatycznie w folderze `Database/` (lub innym wskazanym).

---

## 🗄️ Schemat bazy danych

## 🗄️ Schemat bazy danych

### 📘 `books`

| Kolumna     | Typ     | Uwagi                        |
|-------------|---------|------------------------------|
| id          | INTEGER | PK, autoinkrement            |
| title       | TEXT    | Tytuł książki, wymagany      |
| author      | TEXT    | Autor, wymagany              |
| isbn        | TEXT    | Unikalny identyfikator ISBN  |
| publisher   | TEXT    | Opcjonalny wydawca           |
| year        | INTEGER | Rok wydania                  |
| pages       | INTEGER | Liczba stron                 |
| genre       | TEXT    | Gatunek literacki            |
| language    | TEXT    | Język                        |

---

### 📗 `copies` (egzemplarze książek)

| Kolumna | Typ     | Uwagi                                       |
|---------|---------|---------------------------------------------|
| id      | INTEGER | PK, autoinkrement                           |
| book_id | INTEGER | FK → `books.id`, wymagane                   |
| status  | INTEGER | Status (np. 0 = dostępna, 1 = wypożyczona) |
| notes   | TEXT    | Uwagi do egzemplarza (opcjonalne)           |

---

### 👤 `users`

| Kolumna           | Typ     | Uwagi                                     |
|-------------------|---------|-------------------------------------------|
| id                | INTEGER | PK, autoinkrement                         |
| login             | TEXT    | Unikalny login, wymagany                  |
| password          | TEXT    | Haszowane hasło (np. SHA-256), wymagane   |
| created_at        | TEXT    | Data utworzenia konta                     |
| is_admin          | INTEGER | 1 = admin, 0 = zwykły użytkownik          |
| security_question | INTEGER | FK → `security_questions.question_id`     |
| security_answer   | TEXT    | Odpowiedź na pytanie zabezpieczające      |

---

### 🧠 `security_questions`

| Kolumna    | Typ     | Uwagi                       |
|------------|---------|-----------------------------|
| question_id| INTEGER | PK, autoinkrement           |
| question   | TEXT    | Treść pytania, unikalna     |

---

### 📄 `user_details`

| Kolumna        | Typ     | Uwagi                            |
|----------------|---------|----------------------------------|
| user_id        | INTEGER | PK + FK → `users.id`             |
| first_name     | TEXT    | Imię                             |
| last_name      | TEXT    | Nazwisko                         |
| date_of_birth  | TEXT    | Data urodzenia                   |
| phone          | TEXT    | Numer telefonu (opcjonalny)      |
| email          | TEXT    | Adres e-mail (unikalny, opcjonalny) |

📌 **Indeks**: `user_details(email)`

---

### 🔄 `loans` (wypożyczenia)

| Kolumna     | Typ     | Uwagi                                      |
|-------------|---------|--------------------------------------------|
| id          | INTEGER | PK, autoinkrement                          |
| user_id     | INTEGER | FK → `users.id`                            |
| copy_id     | INTEGER | FK → `copies.id`                           |
| loan_date   | TEXT    | Data wypożyczenia                          |
| return_date | TEXT    | Data zwrotu (może być NULL, jeśli nie zwrócono) |

📌 **Indeksy**: `loans(user_id)`, `loans(copy_id)`

---

### ⚙️ `settings`

| Kolumna | Typ   | Uwagi                                            |
|---------|-------|--------------------------------------------------|
| name    | TEXT  | Klucz ustawienia, unikalny (PK)                 |
| note    | TEXT  | Opis ustawienia (opcjonalny)                    |
| value   | TEXT  | Wartość jako tekst (np. liczba dni = "14")      |

📌 Przykładowe ustawienia:
- `loan_period_days`: liczba dni na zwrot książki (np. 14)
- `max_simultaneous_loans`: maksymalna liczba wypożyczeń jednocześnie
- `max_monthly_loans`: maksymalna liczba wypożyczeń miesięcznie

---

### 🧩 Indeksy

- `idx_user_details_email` → `user_details(email)`
- `idx_books_isbn` → `books(isbn)`
- `idx_copies_book_id` → `copies(book_id)`
- `idx_loans_user_id` → `loans(user_id)`
- `idx_loans_copy_id` → `loans(copy_id)`

## 🔐 Bezpieczeństwo

- Walidacja danych wejściowych (formularze)  
- Zabezpieczenie dostępu do danych przez warstwę logiczną  
- Możliwość weryfikacji rekordów przed usunięciem

---

## 🤝 Kontrybucja

Projekt zaliczeniowy – nie przewiduje się dalszego rozwoju, ale mile widziane zgłoszenia błędów lub sugestie (Issues).

---

## 📜 Licencja

Projekt edukacyjny — brak licencji komercyjnej. Można używać dowolnie.

---
