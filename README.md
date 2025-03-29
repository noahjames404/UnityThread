# UnityThread Documentation

## Overview
`UnityThread` is a singleton-based threading utility for Unity that allows developers to enqueue tasks safely and execute them within the Unity engine's update loop. It ensures thread safety when executing tasks that interact with Unity’s main thread.

## Installation
Ensure that your Unity project has access to the necessary `UnityEngine` namespace before using `UnityThread`.

```csharp
using Maphatar.UnityThread;
```

## Features
- **Singleton-based execution**: Ensures a single instance of `UnityThread` exists in the scene.
- **Task queuing**: Supports enqueuing tasks as `Action` or `IEnumerator`.
- **Automatic execution**: Executes pending jobs in Unity’s `FixedUpdate()` loop.
- **Exception handling**: Captures errors and prevents the queue from breaking unexpectedly.
- **Safe coroutine stopping**: Stops coroutines safely to prevent errors.

## Usage

### Getting the Singleton Instance
To access `UnityThread`, use:
```csharp
UnityThread thread = UnityThread.Instance;
```

### Enqueuing Tasks

#### Enqueue an Action
```csharp
UnityThread.Instance.EnqueueTask(() => Debug.Log("This runs on Unity’s main thread"));
```

#### Enqueue a Coroutine
```csharp
UnityThread.Instance.EnqueueTask(MyCoroutine());
```

Example coroutine:
```csharp
IEnumerator MyCoroutine()
{
    Debug.Log("Start coroutine");
    yield return new WaitForSeconds(2);
    Debug.Log("Coroutine finished");
}
```

### Checking Pending Jobs
To check if there are any pending tasks:
```csharp
bool hasJobs = UnityThread.Instance.HasPendingJob();
```

### Logging Messages
To add a message to the internal log queue:
```csharp
UnityThread.Instance.Log("This is a log message");
```

### Stopping a Coroutine Safely
To safely stop a coroutine:
```csharp
Coroutine myCoroutine = StartCoroutine(MyCoroutine());
UnityThread.Instance.SafelyStopCoroutine(myCoroutine);
```

## Lifecycle Management
The `UnityThread` instance persists across scenes using:
```csharp
DontDestroyOnLoad(gameObject);
```
This ensures that tasks remain available throughout the application’s lifecycle.

## Example Use Case
Below is an example of how to use `UnityThread` to execute tasks on Unity’s main thread.
```csharp
void Start()
{
    UnityThread.Instance.EnqueueTask(() => Debug.Log("Task executed in UnityThread"));
    UnityThread.Instance.EnqueueTask(MyCoroutine());
}

IEnumerator MyCoroutine()
{
    Debug.Log("Coroutine started");
    yield return new WaitForSeconds(3);
    Debug.Log("Coroutine completed");
}
```

## Using UnityThread with Background Threads
`UnityThread` is particularly useful when working with background threads that need to update Unity’s main thread safely.

### Running a Background Thread
```csharp
void Start()
{
    Thread backgroundThread = new Thread(BackgroundTask);
    backgroundThread.Start();
}

void BackgroundTask()
{
    Thread.Sleep(2000); // Simulating heavy computation
    UnityThread.Instance.EnqueueTask(() => Debug.Log("Background thread completed. Now updating Unity UI."));
}
```

### Using UnityThread with API Calls
If you're fetching data from a server in a background thread, you can safely update the UI in Unity’s main thread:
```csharp
void FetchDataFromServer()
{
    Thread backgroundThread = new Thread(() =>
    {
        string data = "Sample API Data"; // Simulated API response
        Thread.Sleep(3000);
        UnityThread.Instance.EnqueueTask(() => Debug.Log($"Data received: {data}"));
    });
    
    backgroundThread.Start();
}
```

#### Tags
Unity
Unity3D
UnityThreading
Threading
Multithreading
Async
Coroutine
UnityCoroutines
BackgroundTasks
GameDevelopment
UnityTools
GameOptimization
UnityPerformance
UnityAPI
CSharp
UnityEngine
TaskQueue
Concurrency
PerformanceOptimization
UnityUtilities

