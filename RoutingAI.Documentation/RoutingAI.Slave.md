# RoutingAI.Slave #

**RoutingAI.Slave.exe** can be run from command line using following syntax:

`RoutingAI.Slave.exe /option1:argument1,argument2 /option2:argument3`

It accepts following options:

- **bw** : *Buffer Width*, specifies number of columns in console window, default is 120. Must have 1 integer argument.

- **IdleTimeout** : Specifies after being idle for how many milliseconds a computation thread is considered timed-out and removed from dispatcher

- **Verbose** : Directs the program to show all messages in the console, including messages marked with a `Trivial` or `Verbose` flag. **Note**: even without this option, all log messages are available in the `debug.log` file.