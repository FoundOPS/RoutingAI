# Distributed Clustering Algorithm #

## Design and API ##

Current idea behind fast clustering is sacrificing precision for speed and scalability. Instead of running PAM Algorithm on the entire set of nodes, it is only run on a random sample (CLARA Algorithm). It is possible to distribute sampling since each slave server has a complete set of data to be processed. 

Future thoughts: KMeans|| might end up being better, it's also distributable. Clustering on multiple dimensions (location and job times).

**RoutingAI.Librarian**

RoutingAI.Librarian can be considered a server for exchanging data between peers. There is need for such a server because RoutingAI.Slave instances are unaware of their peers, therefore communication between them is not possible by design.


```InitClusteringCache(Guid jobId)``` is called before starting clustering algorithm to create an entry in centroid table.

```SubmitSamplingResult(Guid jobId, Int32 score, Int32[] centroids)``` is called after each sampling iteration to transmit and maintain current best sampling result.

**RoutingAI**

RoutingAI assembly contains all generic algorithm code. All clustering-related classes are located in RoutingAI.Algorithms.Clustering namespace.

```IClusteringAlgorithm``` interface represents a clustering algorithm.

```PAMClusteringAlgorithm``` is a non-parallelized, single-threaded implementation of K-Medoid algorithm. It's fast for small number of nodes with many clusters. It is more precise than CLARA.

```CLARAClusteringAlgorithm``` is a parallelized and distributable implementation of CLARA algorithm. It is fast for large numbers of nodes with less than ```sqrt(N)``` clusters, where ```N``` is number of nodes. One key parameter in setting up CLARA is ```SamplingRuns```. A large ```SamplingRuns``` parameter will cause CLARA to produce more accurate results at the cost of dramatically increased runtime. Meanwhile, a small ```SamplingRuns``` value will drastically improve runtime at the cost of precision. Maybe next step in developing CLARA would be distributing it across multiple slave servers and introducing some kind of heruistic for sampling.