using System;
using System.Collections.Concurrent;
using System.Threading;

public class TaskProcessor
{
    private readonly Thread _workerThread;
    private readonly BlockingCollection<Action> _taskQueue = new();
    private bool _isRunning = true;

    public TaskProcessor()
    {
        _workerThread = new Thread(ProcessTasks)
        {
            IsBackground = true
        };

        _workerThread.Start();
    }

    public void EnqueueTask(Action task)
    {
        _taskQueue.Add(task);
    }

    public void Stop()
    {
        _isRunning = false;
        _taskQueue.CompleteAdding();
    }

    private void ProcessTasks()
    {
        while (_isRunning || _taskQueue.Count > 0)
        {
            try
            {
                var task = _taskQueue.Take(); // Blocks until a task is available
                task.Invoke();
            }
            catch (InvalidOperationException)
            {
                // Handle case where queue is empty and CompleteAdding is called
                break;
            }
        }
    }
}
