using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libWyvernzora;
using System.Runtime.Serialization;
using RoutingAI.API.OSRM;

namespace RoutingAI.Algorithms.DFEKM
{
    /// <summary>
    /// Cluster Abstraction Table used by K-Means Clustering Algorithm
    /// </summary>
    [DataContract(Name = "ca_table", Namespace = "http://foundops.com/services/routing_ai/1.0")]
    public class CaTable
    {
        #region Fields 

        [DataMember]
        private Guid _id;
        [DataMember]
        private Int32 _k;

        [DataMember]
        private List<Pair<Coordinate, Double>[]> _store;

        #endregion

        #region Properties

        /// <summary>
        /// Unique identifier that represents K-Means problem instance
        /// that owns this CA Table
        /// </summary>
        [IgnoreDataMember]
        public Guid ID
        { get { return _id; } }
        /// <summary>
        /// Number of clusters that K-Means Clustering Algorithm
        /// is trying to produce
        /// </summary>
        [IgnoreDataMember]
        public Int32 ClusterCount
        { get { return _k; } }
        /// <summary>
        /// Number of iterations elapsed
        /// </summary>
        [IgnoreDataMember]
        public Int32 IterationCount
        { get { return _store.Count; } }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">Unique identifier of the K-Means problem instance</param>
        /// <param name="initialCenters">K-Means centroids based on random sampling of data</param>
        public CaTable(Guid id, Pair<Coordinate, Double>[] initialCenters)
        {
            this._id = id;
            this._store = new List<Pair<Coordinate, Double>[]>();
            _store.Add(initialCenters);
            this._k = initialCenters.Length;
        }

        #region Methods

        /// <summary>
        /// Returns data of all centroids of the specified iteration
        /// </summary>
        /// <param name="i">)-based iteration index</param>
        /// <returns>array of Pairs of centroid coordinate and its confidence radius</returns>
        public Pair<Coordinate, Double>[] GetIteration(Int32 i)
        {
            if (i < 0 || i >= IterationCount)
                throw new ArgumentOutOfRangeException();

            return _store[i];
        }
        /// <summary>
        /// Returns data about a specific centroid at the specified iteration
        /// </summary>
        /// <param name="i">0-based iteration index</param>
        /// <param name="k">0-based centroid index</param>
        /// <returns>Pair of centroid coordinate and its confidence radius</returns>
        public Pair<Coordinate, Double> GetCentroidTuple(Int32 i, Int32 k)
        {
            if (i < 0 || i >= IterationCount || k < 0 || k >= _k)
                throw new ArgumentOutOfRangeException();

            return _store[i][k];
        }
        /// <summary>
        /// Adds centroids prodiced by an iteration to the table
        /// </summary>
        /// <param name="iter">Centroids produced by an iteration</param>
        public void AddIteration(Pair<Coordinate, Double>[] iter)
        {
            if (iter.Length != _k)
                throw new ArgumentException();

            _store.Add(iter);
        }

        #endregion
    }
}
