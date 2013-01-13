using System.IO;
using RoutingAI.DataContracts;
using System;
using System.Collections.Generic;

namespace RoutingAI.Sandbox
{
    public enum OptimizationPeriod
    {
        Day,
        Week,
        Month,
        Year
    }

    public class GenerationConfig
    {
        /// <summary>
        /// The number of resources
        /// </summary>
        public int NumberResources { get; set; }
        /// <summary>
        /// Number of tasks per resource's work day
        /// (affects how many tasks are generated)
        /// </summary>
        public int TasksPerResourceDay { get; set; }
        /// <summary>
        /// The period the request is optimizing for
        /// (affects how many tasks are generated)
        /// </summary>
        public OptimizationPeriod Period { get; set; }
        /// <summary>
        /// The cost per hour of a resource traveling in cents
        /// </summary>
        public UInt32 ResourceCostPerHour { get; set; }
        /// <summary>
        /// The cost per mile of a resource traveling in cents
        /// </summary>
        public UInt32 ResourceCostPerMile { get; set; }
        /// <summary>
        /// The number of tasks
        /// </summary>
        public int NumberTasks { get; set; }
        /// <summary>
        /// Likelihood to complete all tasks 1-100
        /// </summary>
        public int LikelihoodCompleteAllTasks { get; set; }
        /// <summary>
        /// A task's average price in cents
        /// </summary>
        public UInt32 TaskAveragePrice { get; set; }
    }

    public class OptimizationRequestGenerator
    {
        private readonly List<decimal[]> _sampleCoordinates = new List<decimal[]>();

        public OptimizationRequestGenerator()
        {
            String[] lines = File.ReadAllLines("Lafayette.csv");
            foreach (String str in lines)
            {
                String[] points = str.Split(new[] { ',' }, 2);
                points[1] = points[1].Replace(",", "");
                var c = new decimal[] { decimal.Parse(points[0]), decimal.Parse(points[1]) };
                if (!_sampleCoordinates.Contains(c)) _sampleCoordinates.Add(c);
            }
        }

        /// <summary>
        /// Generate an optimization request based on the configuration
        /// TODO add multiple / mixed skill requests, add dynamic availability
        /// </summary>
        /// <param name="config">The configuration used to generate the request</param>
        /// <returns>A generated optimization request</returns>
        public OptimizationRequest Generate(GenerationConfig config)
        {
            var resources = new List<Resource>();

            var window = OptimizationWindow(config.Period);

            for (var r = 0; r < config.NumberResources; r++)
            {
                resources.Add(new Resource
                {
                    Id = (uint)r,
                    CostPerHour = config.ResourceCostPerHour,
                    CostPerMile = config.ResourceCostPerMile,
                    OriginLatitude = _sampleCoordinates[0][0],
                    OriginLongitude = _sampleCoordinates[0][1],
                    //TODO make these more dynamic
                    Availability = window,
                    Skills = new uint[] { 1 }
                });
            }

            var averageTime = AverageTaskTime(config);

            var tasks = new List<Task>();
            var taskCount = TaskCount(config);
            var coordinateIndex = 0;

            for (var t = 0; t < taskCount; t++)
            {
                coordinateIndex++;
                if (coordinateIndex > _sampleCoordinates.Count)
                    coordinateIndex = 1;

                var task = new Task((uint)t, 0, 0)
                    {
                        Time = (uint)averageTime,
                        Value = config.TaskAveragePrice,
                        Latitude = _sampleCoordinates[coordinateIndex][0],
                        Longitude = _sampleCoordinates[coordinateIndex][1],
                        //TODO make these more dynamic
                        Windows = new[] { window },
                        RequiredSkill = 1
                    };

                tasks.Add(task);
            }

            return new OptimizationRequest
            {
                Id = Guid.NewGuid(),
                ClientId = Guid.NewGuid(),
                RegionCode = "debug",
                Resources = resources.ToArray(),
                Tasks = tasks.ToArray()
            };
        }

        /// <summary>
        /// Calculate the # tasks based on resource information and the period to optimize for
        /// </summary>
        /// <param name="config">The configuration used to generate the request</param>
        /// <returns>The number of tasks</returns>
        public static int TaskCount(GenerationConfig config)
        {
            var window = OptimizationWindow(config.Period);
            var tasksCount = config.NumberResources * config.TasksPerResourceDay * window.End.Subtract(window.Start).TotalDays;
            return (int)tasksCount;
        }

        /// <summary>
        /// Returns the window of Tasks / Resources to optimize for
        /// </summary>
        /// <param name="period">The length of the period</param>
        /// <returns>Inclusive start and end date</returns>
        public static Window OptimizationWindow(OptimizationPeriod period)
        {
            //inclusive
            var start = DateTime.UtcNow.Date.AddHours(8); //8 am
            var end = DateTime.UtcNow.Date.AddHours(20); //8 pm
            switch (period)
            {
                case OptimizationPeriod.Week:
                    end = start.AddDays(7);
                    break;
                case OptimizationPeriod.Month:
                    end = start.AddMonths(1);
                    break;
                case OptimizationPeriod.Year:
                    end = start.AddYears(1);
                    break;
            }

            return new Window { Start = start, End = end };
        }

        /// <summary>
        /// Calculate the average task time
        /// </summary>
        /// <param name="config">The configuration used to generate the request</param>
        /// <returns>Time in minutes</returns>
        public static int AverageTaskTime(GenerationConfig config)
        {
            //12 hours (workday) divided by number tasks per resource day
            var taskTime = (12 * 60) / config.TasksPerResourceDay;

            //30 minutes is a guess for the average time. it can be improved as we know more info
            var travelTime = 30 * (2 * config.LikelihoodCompleteAllTasks / 100);

            //if 100% likelihood to complete all tasks, account for 100 minutes in between tasks
            //if 0% likelihood to complete all tasks, account for 0 minutes in between tasks
            taskTime = Math.Max(taskTime - travelTime, 1);
            return taskTime;
        }
    }
}
