using System;

namespace ReallySimpleMl
{
    public class Program
    {
        private static void Main(string[] args)
        {
            Yelp yelp = new Yelp();
            var model = yelp.TrainMl();
            yelp.ExecuteMl(model);
        }
    }
}
