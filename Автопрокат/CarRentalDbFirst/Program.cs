using System;
using System.Linq;
using System.Text;
using System.Data.Entity;

namespace CarRentalDbFirst
{
    internal class Program
    {
        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;

            using (var db = new CarRentalDbEntities())
            {
                // 0. Добавляем марки, если их ещё нет
                var toyota = db.Brands.FirstOrDefault(b => b.Name == "Toyota");
                if (toyota == null)
                {
                    toyota = new Brands { Name = "Toyota", Country = "Japan" };
                    db.Brands.Add(toyota);
                }

                var bmw = db.Brands.FirstOrDefault(b => b.Name == "BMW");
                if (bmw == null)
                {
                    bmw = new Brands { Name = "BMW", Country = "Germany" };
                    db.Brands.Add(bmw);
                }

                db.SaveChanges();

                // Заполняем пустую таблицу автомобилей
                if (!db.Cars.Any())
                {
                    db.Cars.Add(new Cars
                    {
                        Model = "Camry",
                        ReleaseYear = 2020,
                        PricePerDay = 50,
                        BrandId = toyota.Id
                    });
                    db.Cars.Add(new Cars
                    {
                        Model = "Corolla",
                        ReleaseYear = 2019,
                        PricePerDay = 40,
                        BrandId = toyota.Id
                    });
                    db.Cars.Add(new Cars
                    {
                        Model = "X5",
                        ReleaseYear = 2022,
                        PricePerDay = 100,
                        BrandId = bmw.Id
                    });
                    db.SaveChanges();
                }

                // 1. Список автомобилей по названию
                Console.WriteLine("1. Все автомобили:");
                var cars = db.Cars.OrderBy(c => c.Model).ToList();
                foreach (var car in cars)
                    Console.WriteLine($"{car.Model}: {car.PricePerDay}");

                // 2. Поиск по модели
                Console.WriteLine("\n2. Поиск Camry:");
                var found = db.Cars.FirstOrDefault(c => c.Model == "Camry");
                if (found != null)
                    Console.WriteLine($"{found.Model}, {found.ReleaseYear}");
                else
                    Console.WriteLine("Не найдено");

                // 3. Поиск по первичному ключу
                int id = cars.First().Id;
                Console.WriteLine($"\n3. Поиск по Id = {id}:");
                var byId = db.Cars.Find(id);
                Console.WriteLine(byId == null ? "Не найдено" : byId.Model);

                // 4. Марка и страна через навигационное свойство
                Console.WriteLine("\n4. Автомобили и марки:");
                foreach (var car in db.Cars.Include(c => c.Brands).ToList())
                    Console.WriteLine(
                        $"{car.Model}: {car.Brands.Name}, {car.Brands.Country}");

                // 5. Добавление автомобиля
                Console.WriteLine("\n5. Добавление автомобиля:");
                var added = new Cars
                {
                    Model = "X5",
                    ReleaseYear = 2023,
                    PricePerDay = 150,
                    BrandId = bmw.Id
                };
                db.Cars.Add(added);
                db.SaveChanges();
                Console.WriteLine($"Добавлен {added.Model}, Id = {added.Id}");

                // 6. Изменение стоимости по Id
                Console.WriteLine("\n6. Изменение стоимости:");
                var updated = db.Cars.Find(added.Id);
                if (updated != null)
                {
                    updated.PricePerDay = 175;
                    db.SaveChanges();
                    Console.WriteLine(
                        $"{updated.Model}: новая цена {updated.PricePerDay}");
                }
                else
                    Console.WriteLine("Не найдено");
            }

            Console.WriteLine("\nНажми Enter для выхода.");
            Console.ReadLine();
        }
    }
}