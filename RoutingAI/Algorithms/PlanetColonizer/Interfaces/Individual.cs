using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutingAI.Algorithms.PlanetColonizer.Interfaces
{
    /// <summary>
    /// Represents an individual entity in a genetic algorithm
    /// </summary>
    public interface Individual
    {
        Int32 ID { get; set; }
        Int32 Fitness { get; set; }

        void Optimize();
        void Mutate();
        void Crossover(Individual parentA, Individual parentB);
    }
}
