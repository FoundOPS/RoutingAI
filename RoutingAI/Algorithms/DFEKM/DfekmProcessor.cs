using RoutingAI.API.OSRM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RoutingAI.ServiceContracts;
using System.ServiceModel;
using RoutingAI.DataContracts;
using RoutingAI.Algorithms.Clustering;

namespace RoutingAI.Algorithms.DFEKM
{
    /// <summary>
    /// One single thread processing K-Means clustering
    /// </summary>
    public class DfekmProcessor
    {
        #region Fields

        private readonly Random rand = new Random();

        // id, ca table and networking
        Guid _id;
        Int32 _myId;
        CaTable _caTable;
        ILibrarianService _librarian;
        
        // problem data, partition and boundaries
        Task[] _problemData;
        ArraySegment<Task> _processorData;

        // distance algorithm
        IDistanceAlgorithm<Task> _distanceAlg;

        #endregion

        #region Methods

        public DfekmProcessor(Guid id, Int32 myId, Uri librarian, Int32 clusterCount, Task[] problemData, Int32 start, Int32 len, IDistanceAlgorithm<Task> alg)
        {
            // Initialize stuff
            this._id = id;
            this._myId = myId;
            this._problemData = problemData;
            this._processorData = new ArraySegment<Task>(_problemData, start, len);
            this._distanceAlg = alg;

            // Initialize WCF librarian proxy
            EndpointAddress endpoint = new EndpointAddress(librarian);
            _librarian = ChannelFactory<ILibrarianService>.CreateChannel(new BasicHttpBinding(), endpoint);
            _librarian.InitializeKMeans(id);

            // Check if this is node-zero
            if (myId == 0)
            { 
                // Grab a random sample
                Int32 sampleSize = (Int32)Math.Ceiling(Math.Sqrt(_problemData.Length));
                if (sampleSize < clusterCount) sampleSize = clusterCount;

                Task[] sample = _problemData.OrderBy((Task t) => { return rand.Next(); }).Take(sampleSize).ToArray();
                
                // Run k-medoid on the sample
                
 
                // Create the first row of CA Table

            }

        }

        #endregion

    }
}
