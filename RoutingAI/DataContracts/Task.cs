using RoutingAI.API.OSRM;
using RoutingAI.Utilities;
using System;
using System.Runtime.Serialization;

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
        /// The estimated time required to complete the task in minutes
        /// </summary>
        [DataMember(Name = "est_time")]
        public UInt32 EstimatedTime { get; set; }

        /// <summary>
        /// How accurate the estimated time is. Should affect the buffer between tasks
        /// Scale of 1-100
        /// </summary>
        [DataMember(Name = "time_confidence")]
        public UInt32 TimeConfidence { get; set; }

        /// <summary>
        /// The monetary value of completing a task in cents
        /// </summary>
        [DataMember(Name = "value")]
        public UInt32 Value { get; set; }

        /// <summary>
        /// Optional. Ensures jobs happen on a user defined recurrence.
        /// If this is set, target range must be set.
        /// </summary>
        [DataMember(Name = "target_date")]
        public DateTime? TargetDate { get; set; }

        /// <summary>
        /// Affects how much a task should be penalized for being off of the target date.
        /// </summary>
        [DataMember(Name = "target_range")]
        public TimeSpan? TargetRange { get; set; }

        /// <summary>
        /// The restrictive date & time windows when the task can be completed
        /// </summary>
        [DataMember(Name = "windows")]
        public Window[] Windows { get; set; }

        /// <summary>
        /// Required skill to complete a job
        /// If null there is no required skill
        /// </summary>
        [DataMember(Name = "req_skill")]
        public UInt32 RequiredSkillType { get; set; }

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

        /// <summary>
        /// Index of the task, used internally for clustering/optimizing
        /// </summary>
        [IgnoreDataMember]
        public Int32 Index
        { get; set; }

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
            t.EstimatedTime = this.EstimatedTime;
            t.Value = this.Value;
            t.Windows = this.Windows.Clone<Window>();
            t.RequiredSkillType = this.RequiredSkillType;

            return t;
        }

        #endregion
    }
}
