using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutingAI.Threading
{
    public interface IComputationTask : IDisposable
    {
        /// <summary>
        /// Computation itself
        /// </summary>
        /// <param name="args">Arguments for computation</param>
        void Compute(params Object[] args);

        /// <summary>
        /// Called when computation is force-stopped
        /// </summary>
        void HandleAbort();

        /// <summary>
        /// Gets results of computation
        /// </summary>
        Object GetResult();
    }

    public interface IComputationTask<TResult> : IComputationTask
    {
        TResult Result { get; }
    }
}
