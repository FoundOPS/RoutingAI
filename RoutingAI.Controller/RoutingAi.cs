using RoutingAI.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using RoutingAI.Threading;
using RoutingAI.DataContracts;
using libWyvernzora.Logging;

namespace RoutingAI.Controller
{
    public class RoutingAi : IRoutingAiService
    {
        const string TAG = "SlaveServer";
        ComputationThreadDispatcher _dispatcher = ComputationThreadDispatcher.Instance;
        
        public CallResponse Post(OptimizationRequest request)
        {
            try
            {
                using (PCRoutingTask task = new PCRoutingTask(request))
                {
                    _dispatcher.NewThread(request.Id);
                    _dispatcher.RunComputation(request.Id, task);
                }
                return new CallResponse() { Success = true, Details = "" };
            }
            catch (Exception ex)
            {
                return new CallResponse() { Success = false, Details = String.Format("{0}{{Message = {1}; Stack = {2}}}", ex.GetType().FullName, ex.Message, ex.StackTrace) };
            }
        }

        public OptimizationResponse<Solution> Get(Guid id)
        {
            ComputationThreadInfo info = _dispatcher.GetThreadInfo(id);

            switch (info.State)
            {
                case ComputationThreadState.Working:
                    return new OptimizationResponse<Solution>()
                        {
                            Progress = 0,
                            Solution = null,
                            Stage = Stage.Processing
                        };
                case ComputationThreadState.Finished:
                    return new OptimizationResponse<Solution>()
                        {
                            Progress = 100,
                            Solution = (Solution) _dispatcher.GetComputationResult(id),
                            Stage = Stage.Completed
                        };
                case ComputationThreadState.Exception:
                    return new OptimizationResponse<Solution>()
                        {
                            Progress = 0,
                            Solution = null,
                            Stage = Stage.Error
                        };
                case ComputationThreadState.Dead:
                    GlobalLogger.SendLogMessage(TAG, MessageFlags.Warning | MessageFlags.Expected, "Get: ID not found: {{{0}}}", id);
                    return new OptimizationResponse<Solution>()
                        {
                            Progress = 0,
                            Solution = null,
                            Stage = Stage.Error
                        };
                default:
                    return new OptimizationResponse<Solution>() {Progress = 0, Solution = null, Stage = Stage.Queued};
            }
        }

        public CallResponse Delete(Guid id)
        {
            return _dispatcher.AbortThreadAction(id);
        }
    }
}
