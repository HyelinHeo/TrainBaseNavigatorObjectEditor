using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;


public class TaskReceiver<T> : Singleton<T> where T : Singleton<T>
{
    public delegate void Task();

    private Queue<Task> TaskQueue = new Queue<Task>();
    private object _queueLock = new object();


    void Update()
    {
        lock (_queueLock)
        {
            if (TaskQueue.Count > 0)
                TaskQueue.Dequeue()();
        }
    }

    public void ScheduleTask(Task newTask)
    {
        lock (_queueLock)
        {
            if (TaskQueue.Count < 100)
                TaskQueue.Enqueue(newTask);
        }
    }
}
