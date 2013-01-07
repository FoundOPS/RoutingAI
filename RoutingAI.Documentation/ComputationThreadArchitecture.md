# Computation Threads #
**ComputationThreads** are wrappers around `System.Threading.Thread` objects that facilitate task management in RoutingAI.Slave project.

## Lifetime of a ComputationThread #

A computation thread may have following states:

- **Ready** - Thread is confogured and ready to accept commands

- **Working** - Thread is in the process of computing something, all commands are rejected

- **Finished** - Thread is done computing whatever it was computing and the result is ready to be fetched; otherwise, this state is the same as *Ready*

- **Exception** - Thread encountered an error doing whatever it was doing. Check logs or use `GetExceptionInfo()` (not implemented yet) method for more details.

- **Dead** - Thread was not found. This means that the thread you are looking for was removed by calling `KillComputationThread()` method. Possibly your thread never existed in the first place. Anyway, ***if you get this one, most likely you got a bug to catch***.

So, computing a routing solution involves two steps at this time. More steps may be added in the future. But first things first, in order to use a computation thread it must be configured first. `CreateComputationThread()` method creates a new thread and initializes it, putting it into *Ready* state. Then, you can call `RunComputation(Task, Object[])` to start computing on it.

You can call `GetComputationThreadInfo()` at any time to get a `ComputationThreadInfo` object describing current thread state. You can also call `AbortComputation()` method at any time to cancel whatever the thread is doing and put it back into *Ready* state. By calling `DisposeComputationThread()` you abort whatever it is doing (unless it's already idle) and remove it from dispatcher, releasing all the resources associated with the thread.

## IComputationTask ##
**IComputationTask** is an interface that represents a task (or a portion of task) to be computed concurrently on a computation thread. I use `IComputationTask` rather than a delegate because I want to remove graceful handling of abort commands and exeptions from task implementation. `IComputationTask` interface has following methods:

- `void Compute(params Object[] args)` method: runs computation on **ONE SINGLE THREAD** or **BLOCKS UNTIL COMPUTATION IS DONE**.

- `void HandleAbort()` method: called when computation thread processing the task receives an abort command.

- `Object Result { get; }` property: when computation finishes, it contains result of computation; otherwise it is null



