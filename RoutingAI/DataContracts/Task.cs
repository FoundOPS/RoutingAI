using RoutingAI.API.OSRM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RoutingAI.DataContracts
{
    /// <summary>
    /// Class representing a single task
    /// </summary>
    /// <remarks>
    /// In order to simplify the AI and avoid transmitting useless data over network,
    /// only barebone structure is kept. Data such as user id should be kept in the 
    /// database server.
    /// </remarks>
    [DataContract(Name = "task", Namespace = "http://foundops.com/services/routing_ai/1.0")]
    public class Task : IEquatable<Task>, ICloneable
    {
        #region Serialized Properties

        /// <summary>
        /// Id of the task
        /// </summary>
        [DataMember(Name = "task_id")]
        public UInt32 Id { get; set; }

        /// <summary>
        /// Latitude component of the task location
        /// </summary>
        [DataMember(Name = "lat")]
        public Decimal Latitude { get; set; }

        /// <summary>
        /// Longitude component of the task location
        /// </summary>
        [DataMember(Name = "lon")]
        public Decimal Longitude { get; set; }

        /// <summary>
        /// Time required to complete the task in seconds
        /// </summary>
        [DataMember(Name = "time_on_task")]
        public UInt32 TimeOnTask { get; set; }

        /// <summary>
        /// The dollar value of completing a task
        /// </summary>
        [DataMember(Name = "value")]
        public UInt32 Value { get; set; }

        /// <summary>
        /// TODO: FUTURE Priority (1)
        /// The restrictive date/time window the task can be completed
        /// </summary>
        [DataMember(Name = "window")]
        public Window Window { get; set; }

        /// <summary>
        /// TODO: FUTURE Priority (2)
        /// Required skills to complete a job
        /// If null there are no required skills
        /// </summary>
        [DataMember(Name = "req_skills")]
        public UInt32[] RequiredSkills { get; set; }


        #endregion

        #region Other Properties (Not Serialized)

        /// <summary>
        /// Gets coordinates of this task
        /// 
        /// </summary>
        [IgnoreDataMember]
        public Coordinate Coordinates
        {
            get { return new Coordinate((double)Latitude, (double)Longitude); }
        }

        #endregion

        /// <summary>
        /// Constructor
        /// Only to be used from within the Task class
        /// </summary>
        private Task() { }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">ID of the task</param>
        /// <param name="lon">Longitude component of the task location</param>
        /// <param name="lat">Latitude component of the task location</param>
        public Task(UInt32 id, Decimal lon, Decimal lat)
        {
            this.Id = id;
            this.Longitude = lon;
            this.Latitude = lat;
        }

        #region Overrides

        public bool Equals(Task other)
        {
            return this.Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            Task t = obj as Task;
            return t != null && t.Id == this.Id;
        }

        public override int GetHashCode()
        {
            return (int)Id;
        }

        public override string ToString()
        {
            return this.Id.ToString();
        }

        public object Clone()
        {
            Task t = new Task();
            t.Id = this.Id;
            t.Latitude = this.Latitude;
            t.Longitude = this.Longitude;
            t.TimeOnTask = this.TimeOnTask;
            t.Value = this.Value;
            t.Window = (Window)this.Window.Clone();
            t.RequiredSkills = new UInt32[this.RequiredSkills.Length];
            Array.Copy(this.RequiredSkills, t.RequiredSkills, t.RequiredSkills.Length);

            return t;
        }

        #endregion

    }
}
