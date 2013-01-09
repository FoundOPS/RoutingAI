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
        /// <summary>
        /// Gets a unique ID representing the individual
        /// </summary>
        Int32 ID { get; set; }
        /// <summary>
        /// Gets fitness value of the individual
        /// </summary>
        Int32 Fitness { get; set; }

        /// <summary>
        /// Starts to optimize the individual
        /// </summary>
        void Optimize();
        /// <summary>
        /// Mutates the individual
        /// </summary>
        void Mutate();
        /// <summary>
        /// Performs crossover of two parent individuals
        /// </summary>
        /// <param name="parentA">First parent individual</param>
        /// <param name="parentB">Second parent individual</param>
        void Crossover(Individual parentA, Individual parentB);
    }
}
