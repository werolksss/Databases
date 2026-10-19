using System.Data;
using System.Globalization;
using System.Text;
using Microsoft.Data.SqlClient;

Console.OutputEncoding = Encoding.UTF8;

using var connection = new SqlConnection();
bool running = true;

while (running)
{
    Console.WriteLine("""
        
        СТРАНЫ
        1 — Подключиться
        2 — Отключиться
        3 — Отчёты
        4 — Добавить
        5 — Изменить
        6 — Удалить
        7 — Хранимые процедуры
        0 — Выход
        """);

    try
    {
        string choice = Read("Пункт");

        if (choice == "0")
        {
            running = false;
            continue;
        }

        if (choice == "1")
        {
            Connect();
            continue;
        }

        if (choice == "2")
        {
            connection.Close();
            Console.WriteLine("Отключено.");
            continue;
        }

        if (connection.State != ConnectionState.Open)
        {
            Console.WriteLine("Сначала подключись через пункт 1.");
            continue;
        }

        switch (choice)
        {
            case "3": Reports(); break;
            case "4": Edit("add"); break;
            case "5": Edit("update"); break;
            case "6": Edit("delete"); break;
            case "7": Procedures(); break;
            default: Console.WriteLine("Нет такого пункта."); break;
        }
    }
    catch (EndOfStreamException)
    {
        break;
    }
    catch (Exception ex)
    {
        Console.WriteLine("Ошибка: " + ex.Message);
    }
}

string Read(string prompt)
{
    Console.Write(prompt + ": ");
    return Console.ReadLine()?.Trim()
        ?? throw new EndOfStreamException();
}

string Name(string prompt)
{
    string value = Read(prompt);

    if (value.Length is < 1 or > 100)
        throw new Exception("Введите от 1 до 100 символов.");

    return value;
}

int Id(string prompt = "ID")
{
    if (int.TryParse(Read(prompt), out int value) && value > 0)
        return value;

    throw new Exception("Нужен положительный целый ID.");
}

long Population()
{
    if (long.TryParse(Read("Население"), out long value) && value >= 0)
        return value;

    throw new Exception("Население — целое число от 0.");
}

decimal Area()
{
    string text = Read("Площадь, км²").Replace(',', '.');

    if (decimal.TryParse(text, NumberStyles.Number,
        CultureInfo.InvariantCulture, out decimal value)
        && value > 0 && value < 10000000000000m)
        return value;

    throw new Exception("Введите положительную площадь.");
}

bool Confirm(string prompt)
{
    return Read(prompt + " (да/нет)")
        .Equals("да", StringComparison.OrdinalIgnoreCase);
}

void Connect()
{
    if (connection.State == ConnectionState.Open)
    {
        Console.WriteLine("Уже подключено.");
        return;
    }

    Console.Write("Пароль SQL Server: ");
    var password = new StringBuilder();

    while (true)
    {
        var key = Console.ReadKey(true);

        if (key.Key == ConsoleKey.Enter)
            break;

        if (key.Key == ConsoleKey.Backspace)
        {
            if (password.Length > 0)
                password.Length--;
        }
        else if (!char.IsControl(key.KeyChar))
        {
            password.Append(key.KeyChar);
        }
    }

    Console.WriteLine();

    connection.ConnectionString = new SqlConnectionStringBuilder
    {
        DataSource = "localhost,1433",
        InitialCatalog = "СтраныДЗ",
        UserID = "sa",
        Password = password.ToString(),
        TrustServerCertificate = true,
        ConnectTimeout = 10
    }.ConnectionString;

    connection.Open();
    Console.WriteLine("Подключение успешно!");
}

SqlCommand Command(string sql, params object[] values)
{
    var command = new SqlCommand(sql, connection);

    for (int i = 0; i < values.Length; i++)
        command.Parameters.AddWithValue("@p" + i, values[i]);

    return command;
}

DataTable Query(string sql, params object[] values)
{
    using var command = Command(sql, values);
    using var reader = command.ExecuteReader();

    var table = new DataTable();
    table.Load(reader);
    return table;
}

void Change(string sql, params object[] values)
{
    using var command = Command(sql, values);
    int count = command.ExecuteNonQuery();

    Console.WriteLine(count == 0
        ? "Запись не найдена или условие не выполнено."
        : "Изменения сохранены.");
}

string Value(object? value)
{
    return value == null || value == DBNull.Value
        ? "не указано"
        : Convert.ToString(value, CultureInfo.InvariantCulture) ?? "";
}

void Print(DataTable table)
{
    Console.WriteLine(string.Join(" | ",
        table.Columns.Cast<DataColumn>().Select(c => c.ColumnName)));

    foreach (DataRow row in table.Rows)
        Console.WriteLine(string.Join(" | ",
            row.ItemArray.Select(Value)));

    Console.WriteLine("Строк: " + table.Rows.Count);
}

void Output(DataTable table)
{
    if (Read("Вывод: 1 — экран, 2 — файл") != "2")
    {
        Print(table);
        return;
    }

    Directory.CreateDirectory("reports");

    string path = Path.GetFullPath(
        $"reports/report_{DateTime.Now:yyyyMMdd_HHmmss_fff}_{Guid.NewGuid():N}.csv");

    string Csv(string text) => "\"" + text.Replace("\"", "\"\"") + "\"";

    var lines = new List<string>
    {
        string.Join(";", table.Columns.Cast<DataColumn>()
            .Select(c => Csv(c.ColumnName)))
    };

    foreach (DataRow row in table.Rows)
        lines.Add(string.Join(";",
            row.ItemArray.Select(x => Csv(Value(x)))));

    File.WriteAllLines(path, lines, new UTF8Encoding(true));
    Console.WriteLine("Сохранено: " + path);
}

void Edit(string action)
{
    string kind = Read(
        "1 — страна, 2 — столица, 3 — крупный город, 4 — население");

    if (kind == "4")
    {
        EditPopulation(action);
        return;
    }

    if (kind == "1")
    {
        Print(Query("SELECT Id,Name FROM Countries ORDER BY Id"));

        if (action == "delete")
        {
            int id = Id("ID страны");

            if (!Confirm("Удалить страну и все её города?"))
                return;

            using var transaction = connection.BeginTransaction();

            try
            {
                using var command = Command("""
                    DELETE FROM Cities WHERE CountryId=@p0;
                    DELETE FROM Countries WHERE Id=@p0;
                    """, id);

                command.Transaction = transaction;
                int count = command.ExecuteNonQuery();
                transaction.Commit();

                Console.WriteLine(count > 0
                    ? "Страна и её города удалены."
                    : "Страна не найдена.");
            }
            catch
            {
                transaction.Rollback();
                throw;
            }

            return;
        }

        int countryId = action == "update" ? Id("ID страны") : 0;
        string name = Name("Название страны");
        long population = Population();
        decimal area = Area();

        Print(Query("SELECT * FROM Continents"));
        int continent = Id("ID части света");

        if (action == "add")
            Change("""
                INSERT INTO Countries(Name,Population,Area,ContinentId)
                VALUES(@p0,@p1,@p2,@p3)
                """, name, population, area, continent);
        else
            Change("""
                UPDATE Countries
                SET Name=@p0,Population=@p1,Area=@p2,ContinentId=@p3
                WHERE Id=@p4
                """, name, population, area, continent, countryId);

        return;
    }

    if (kind != "2" && kind != "3")
        throw new Exception("Нет такого пункта.");

    bool capital = kind == "2";

    Print(Query("""
        SELECT city.*,c.Name AS Country
        FROM Cities city
        JOIN Countries c ON c.Id=city.CountryId
        ORDER BY city.Id
        """));

    if (action == "delete")
    {
        int id = Id("ID города");

        if (!Confirm(capital
            ? "Убрать статус столицы, сохранив город?"
            : "Удалить крупный город?"))
            return;

        if (capital)
            Change("""
                UPDATE Cities SET IsCapital=0
                WHERE Id=@p0 AND IsCapital=1
                """, id);
        else
            Change("""
                DELETE FROM Cities
                WHERE Id=@p0 AND IsMajor=1 AND IsCapital=0
                """, id);

        return;
    }

    int cityId = action == "update" ? Id("ID города") : 0;
    string cityName = Name("Название города");
    long people = Population();

    Print(Query("SELECT Id,Name FROM Countries"));
    int country = Id("ID страны");
    bool major = capital ? Confirm("Это крупный город?") : true;

    if (action == "add")
    {
        var existing = Query("""
            SELECT Id FROM Cities
            WHERE CountryId=@p0 AND Name=@p1
            """, country, cityName);

        if (existing.Rows.Count > 0)
        {
            if (!capital)
                throw new Exception("Город уже существует. Выбери изменение.");

            Change("""
                UPDATE Cities
                SET IsCapital=1,IsMajor=@p0,Population=@p1
                WHERE Id=@p2
                """, major, people, existing.Rows[0][0]);
        }
        else
        {
            Change("""
                INSERT INTO Cities
                    (Name,Population,IsCapital,IsMajor,CountryId)
                VALUES(@p0,@p1,@p2,@p3,@p4)
                """, cityName, people, capital, major, country);
        }
    }
    else
    {
        string condition = capital ? "IsCapital=1" : "IsMajor=1";

        Change("""
            UPDATE Cities
            SET Name=@p0,Population=@p1,CountryId=@p2,IsMajor=@p3
            WHERE Id=@p4 AND
            """ + " " + condition,
            cityName, people, country, major, cityId);
    }
}

void EditPopulation(string action)
{
    string kind = Read("1 — население страны, 2 — население города");

    string table = kind switch
    {
        "1" => "Countries",
        "2" => "Cities",
        _ => throw new Exception("Нет такого пункта.")
    };

    Print(Query($"SELECT Id,Name,Population FROM {table}"));
    int id = Id();

    if (action == "delete")
    {
        if (Confirm("Удалить сведения о населении?"))
            Change($"UPDATE {table} SET Population=NULL WHERE Id=@p0", id);

        return;
    }

    long population = Population();
    string condition = action == "add" ? " AND Population IS NULL" : "";

    Change($"UPDATE {table} SET Population=@p0 WHERE Id=@p1" + condition,
        population, id);
}

void Procedures()
{
    string choice = Read("""
        1 — Топ-3 стран по населению
        2 — Топ-3 столиц по населению
        3 — Самая населённая страна
        4 — Самый населённый город
        Выбери
        """);

    string name = choice switch
    {
        "1" => "dbo.Top3Countries",
        "2" => "dbo.Top3Capitals",
        "3" => "dbo.MostPopulatedCountry",
        "4" => "dbo.MostPopulatedCity",
        _ => throw new Exception("Нет такого пункта.")
    };

    using var command = Command(name);
    command.CommandType = CommandType.StoredProcedure;

    var table = new DataTable();

    using (var reader = command.ExecuteReader())
        table.Load(reader);

    Output(table);
}

void Reports()
{
    Console.WriteLine("""
        1 — Все страны
        2 — Выбранные поля стран
        3 — Конкретная страна
        4 — Города страны
        5 — Страны на указанную букву
        6 — Столицы на указанную букву
        7 — Топ-3 малонаселённых столиц
        8 — Топ-3 малонаселённых стран
        9 — Среднее население столиц по частям света
        10 — Топ-3 малонаселённых стран каждой части света
        11 — Топ-3 самых населённых стран каждой части света
        12 — Среднее население городов страны
        13 — Самый малонаселённый город страны
        """);

    string choice = Read("Отчёт");
    string sql;
    object[] values = [];

    switch (choice)
    {
        case "1":
            sql = "SELECT * FROM CountryReport ORDER BY Id";
            break;

        case "2":
            string[] fields =
            [
                "Name", "Population", "Area",
                "Continent", "Capital", "CapitalPopulation"
            ];

            Console.WriteLine("""
                1 — Название, 2 — Население, 3 — Площадь,
                4 — Часть света, 5 — Столица, 6 — Население столицы
                """);

            var indexes = Read("Номера полей через запятую")
                .Split(',')
                .Select(x => int.Parse(x.Trim()) - 1)
                .Distinct()
                .ToArray();

            if (indexes.Any(x => x < 0 || x >= fields.Length))
                throw new Exception("Нужны номера от 1 до 6.");

            sql = "SELECT " + string.Join(",", indexes.Select(x => fields[x]))
                + " FROM CountryReport";
            break;

        case "3":
            sql = "SELECT * FROM CountryReport WHERE Name=@p0";
            values = [Name("Название страны")];
            break;

        case "4":
            sql = """
                SELECT city.*
                FROM Cities city
                JOIN Countries c ON c.Id=city.CountryId
                WHERE c.Name=@p0
                """;
            values = [Name("Название страны")];
            break;

        case "5":
        case "6":
            string letter = Name("Одна начальная буква");

            if (letter.Length != 1 || !char.IsLetter(letter[0]))
                throw new Exception("Введите одну букву.");

            sql = choice == "5"
                ? """
                  SELECT * FROM CountryReport
                  WHERE LOWER(LEFT(Name,1))=LOWER(@p0)
                  """
                : """
                  SELECT Name,Population FROM Cities
                  WHERE IsCapital=1
                  AND LOWER(LEFT(Name,1))=LOWER(@p0)
                  """;

            values = [letter];
            break;

        case "7":
            sql = """
                SELECT TOP(3) Name,Population FROM Cities
                WHERE IsCapital=1 AND Population IS NOT NULL
                ORDER BY Population,Id
                """;
            break;

        case "8":
            sql = """
                SELECT TOP(3) Name,Population FROM Countries
                WHERE Population IS NOT NULL
                ORDER BY Population,Id
                """;
            break;

        case "9":
            sql = """
                SELECT Continent,
                    AVG(CAST(CapitalPopulation AS decimal(20,2)))
                        AS AveragePopulation
                FROM CountryReport
                GROUP BY Continent
                """;
            break;

        case "10":
        case "11":
            string order = choice == "10" ? "ASC" : "DESC";

            sql = $"""
                WITH ranked AS
                (
                    SELECT Name,Continent,Population,
                        ROW_NUMBER() OVER
                        (
                            PARTITION BY Continent
                            ORDER BY Population {order},Id
                        ) AS Position
                    FROM CountryReport
                    WHERE Population IS NOT NULL
                )
                SELECT * FROM ranked
                WHERE Position<=3
                ORDER BY Continent,Position
                """;
            break;

        case "12":
            sql = """
                SELECT c.Name,
                    AVG(CAST(city.Population AS decimal(20,2)))
                        AS AverageCityPopulation
                FROM Countries c
                LEFT JOIN Cities city ON city.CountryId=c.Id
                WHERE c.Name=@p0
                GROUP BY c.Id,c.Name
                """;
            values = [Name("Название страны")];
            break;

        case "13":
            sql = """
                SELECT TOP(1) city.Name,city.Population
                FROM Cities city
                JOIN Countries c ON c.Id=city.CountryId
                WHERE c.Name=@p0 AND city.Population IS NOT NULL
                ORDER BY city.Population,city.Id
                """;
            values = [Name("Название страны")];
            break;

        default:
            throw new Exception("Нет такого отчёта.");
    }

    Output(Query(sql, values));
}