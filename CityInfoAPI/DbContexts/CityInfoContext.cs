using CityInfoAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace CityInfoAPI.DbContexts
{
    public class CityInfoContext : DbContext
    {
        public CityInfoContext(DbContextOptions<CityInfoContext> options)
            : base(options)
        {
        }

        public DbSet<City> Cities { get; set; }
        public DbSet<PointOfInterest> PointsOfInterest { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<City>().HasData(
                               new City("New York City")
                               {
                                   Id = 1,
                                   Description = "The one with that big park."
                               },
                                new City("Antwerp")
                                {
                                    Id = 2,
                                    Description = "The one with the cathedral that was never really finished."
                                },
                                new City("Paris")
                                {
                                    Id = 3,
                                    Description = "The one with that big tower."
                                });

            modelBuilder.Entity<PointOfInterest>().HasData(
                               new PointOfInterest("Central Park")
                               {
                                   Id = 1,
                                   CityId = 1,
                                   Description = "A very tall building."
                               },
                                new PointOfInterest("Empire State Building")
                                {
                                    Id = 2,
                                    CityId = 1,
                                    Description = "A very tall building."
                                },
                                new PointOfInterest("The Cloisters")
                                {
                                    Id = 3,
                                    CityId = 1,
                                    Description = "The Met Cloisters."
                                },
                                new PointOfInterest("Cathedral of Our Lady")
                                {
                                    Id = 4,
                                    CityId = 2,
                                    Description = "A Gothic style cathedral, conceived by architects Jan and Pieter Appelmans."
                                },
                                new PointOfInterest("Antwerp Central Station")
                                {
                                    Id = 5,
                                    CityId = 2,
                                    Description = "The the finest example of railway architecture in Belgium."
                                },
                                new PointOfInterest("Eiffel Tower")
                                {
                                    Id = 6,
                                    CityId = 3,
                                    Description = "A wrought iron lattice tower on the Champ de Mars, named after engineer Gustave Eiffel."
                                },
                                new PointOfInterest("The Louvre")
                                {
                                    Id = 7,
                                    CityId = 3,
                                    Description = "The world's largest art museum and a historic monument in Paris, France."
                                });
        }
    }
}
