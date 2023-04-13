using System;
using System.Collections.Generic;

namespace ReallySimpleMl
{
    public class Program
    {
        private static readonly Dictionary<int, string> keys = new Dictionary<int, string>()
        {
            {0, "Yelp!" },
            {1, "Descobre se é vaga" },
            {2, "Turtles!" }
        };

        private static void Main(string[] args)
        {
            Console.WriteLine("Selecione um modelo abaixo de ML:");
            foreach (var key in keys)
            {
                Console.WriteLine($"{key.Key} - {key.Value}");
            }
            int value;
            while (!int.TryParse(Console.ReadLine(), out value))
            {
                Console.WriteLine("Selecione um valor valido!");
            }

            switch (value)
            {
                case 0:
                    Yelp yelp = new Yelp();
                    var model = yelp.TrainMl();
                    yelp.ExecuteMl(model);
                    break;

                case 1:
                    MlVacancies mlVacancies = new MlVacancies();
                    var vacancyModel = mlVacancies.TrainMl();
                    mlVacancies.ExecuteMl(vacancyModel);
                    break;

                case 2:
                    Turtles turtles = new Turtles();
                    var turtleModels = turtles.TrainModel();
                    turtles.ExecuteModel(turtleModels);
                    break;
                default:
                    break;
            }

        }
    }
}
