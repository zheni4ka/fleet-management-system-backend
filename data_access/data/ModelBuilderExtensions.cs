using business_logic.Entities;
using business_logic.Constants;
using Microsoft.EntityFrameworkCore;
namespace data_access.data
{
    public static class ModelBuilderExtensions
    {
        public static void SeedData(this ModelBuilder builder)
        {
             builder.Entity<Location>().HasData(
     
             new Location { Id = 1, City = "Kyiv", Country = LocationConstraints.UkraineCountryName, TimeZoneId = LocationConstraints.UkraineTimeZoneId },
             new Location { Id = 2, City = "Lviv", Country = LocationConstraints.UkraineCountryName, TimeZoneId = LocationConstraints.UkraineTimeZoneId },
             new Location { Id = 3, City = "Odessa", Country = LocationConstraints.UkraineCountryName, TimeZoneId = LocationConstraints.UkraineTimeZoneId },
             new Location { Id = 4, City = "Kharkiv", Country = LocationConstraints.UkraineCountryName, TimeZoneId = LocationConstraints.UkraineTimeZoneId },
             new Location { Id = 5, City = "Dnipro", Country = LocationConstraints.UkraineCountryName, TimeZoneId = LocationConstraints.UkraineTimeZoneId },
             new Location { Id = 6, City = "Zaporizhzhia", Country = LocationConstraints.UkraineCountryName, TimeZoneId = LocationConstraints.UkraineTimeZoneId },
             new Location { Id = 7, City = "Ivano-Frankivsk", Country = LocationConstraints.UkraineCountryName, TimeZoneId = LocationConstraints.UkraineTimeZoneId },
             new Location { Id = 8, City = "Ternopil", Country = LocationConstraints.UkraineCountryName, TimeZoneId = LocationConstraints.UkraineTimeZoneId },
             new Location { Id = 9, City = "Chernivtsi", Country = LocationConstraints.UkraineCountryName, TimeZoneId = LocationConstraints.UkraineTimeZoneId },
             new Location { Id = 10, City = "Vinnytsia", Country = LocationConstraints.UkraineCountryName, TimeZoneId = LocationConstraints.UkraineTimeZoneId },
             new Location { Id = 11, City = "Rivne", Country = LocationConstraints.UkraineCountryName, TimeZoneId = LocationConstraints.UkraineTimeZoneId },
             new Location { Id = 12, City = "Lutsk", Country = LocationConstraints.UkraineCountryName, TimeZoneId = LocationConstraints.UkraineTimeZoneId },
             new Location { Id = 13, City = "Poltava", Country = LocationConstraints.UkraineCountryName, TimeZoneId = LocationConstraints.UkraineTimeZoneId },
             new Location { Id = 14, City = "Chernihiv", Country = LocationConstraints.UkraineCountryName, TimeZoneId = LocationConstraints.UkraineTimeZoneId },
             new Location { Id = 15, City = "Cherkasy", Country = LocationConstraints.UkraineCountryName, TimeZoneId = LocationConstraints.UkraineTimeZoneId },
             new Location { Id = 16, City = "Khmelnytskyi", Country = LocationConstraints.UkraineCountryName, TimeZoneId = LocationConstraints.UkraineTimeZoneId },
             new Location { Id = 17, City = "Zhytomyr", Country = LocationConstraints.UkraineCountryName, TimeZoneId = LocationConstraints.UkraineTimeZoneId },
             new Location { Id = 18, City = "Uzhhorod", Country = LocationConstraints.UkraineCountryName, TimeZoneId = LocationConstraints.UkraineTimeZoneId },
             new Location { Id = 19, City = "Mykolaiv", Country = LocationConstraints.UkraineCountryName, TimeZoneId = LocationConstraints.UkraineTimeZoneId },
             new Location { Id = 20, City = "Kherson", Country = LocationConstraints.UkraineCountryName, TimeZoneId = LocationConstraints.UkraineTimeZoneId },
             new Location { Id = 21, City = "Sumy", Country = LocationConstraints.UkraineCountryName, TimeZoneId = LocationConstraints.UkraineTimeZoneId },
             new Location { Id = 22, City = "Kropyvnytskyi", Country = LocationConstraints.UkraineCountryName, TimeZoneId = LocationConstraints.UkraineTimeZoneId },


             new Location { Id = 24, City = "Warsaw", Country = LocationConstraints.PolandCountryName, TimeZoneId = LocationConstraints.PolandTimeZoneId },
             new Location { Id = 25, City = "Krakow", Country = LocationConstraints.PolandCountryName, TimeZoneId = LocationConstraints.PolandTimeZoneId },
             new Location { Id = 26, City = "Wroclaw", Country = LocationConstraints.PolandCountryName, TimeZoneId = LocationConstraints.PolandTimeZoneId },
             new Location { Id = 27, City = "Gdansk", Country = LocationConstraints.PolandCountryName, TimeZoneId = LocationConstraints.PolandTimeZoneId },
             new Location { Id = 28, City = "Poznan", Country = LocationConstraints.PolandCountryName, TimeZoneId = LocationConstraints.PolandTimeZoneId },
             new Location { Id = 29, City = "Lodz", Country = LocationConstraints.PolandCountryName, TimeZoneId = LocationConstraints.PolandTimeZoneId },
             new Location { Id = 30, City = "Katowice", Country = LocationConstraints.PolandCountryName, TimeZoneId = LocationConstraints.PolandTimeZoneId },
             new Location { Id = 31, City = "Lublin", Country = LocationConstraints.PolandCountryName, TimeZoneId = LocationConstraints.PolandTimeZoneId },
             new Location { Id = 32, City = "Rzeszow", Country = LocationConstraints.PolandCountryName, TimeZoneId = LocationConstraints.PolandTimeZoneId },
             new Location { Id = 33, City = "Szczecin", Country = LocationConstraints.PolandCountryName, TimeZoneId = LocationConstraints.PolandTimeZoneId },
             new Location { Id = 34, City = "Bydgoszcz", Country = LocationConstraints.PolandCountryName, TimeZoneId = LocationConstraints.PolandTimeZoneId },
             new Location { Id = 35, City = "Gdynia", Country = LocationConstraints.PolandCountryName, TimeZoneId = LocationConstraints.PolandTimeZoneId },
             new Location { Id = 36, City = "Bialystok", Country = LocationConstraints.PolandCountryName, TimeZoneId = LocationConstraints.PolandTimeZoneId },
             new Location { Id = 37, City = "Czestochowa", Country = LocationConstraints.PolandCountryName, TimeZoneId = LocationConstraints.PolandTimeZoneId },
             new Location { Id = 38, City = "Radom", Country = LocationConstraints.PolandCountryName, TimeZoneId = LocationConstraints.PolandTimeZoneId },
             new Location { Id = 39, City = "Torun", Country = LocationConstraints.PolandCountryName, TimeZoneId = LocationConstraints.PolandTimeZoneId },
             new Location { Id = 40, City = "Kielce", Country = LocationConstraints.PolandCountryName, TimeZoneId = LocationConstraints.PolandTimeZoneId },
             new Location { Id = 41, City = "Gliwice", Country = LocationConstraints.PolandCountryName, TimeZoneId = LocationConstraints.PolandTimeZoneId }
 );
        }
    }
}
