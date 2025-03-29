using System;
using System.Collections.Generic; 
using UnityEngine;
using static UnityEngine.Debug;
using System.Collections; 


namespace Maphatar.UnityThread
{
    public class UnityThread : MonoBehaviour
    {
        static UnityThread instance;

        Queue<object> jobs = new Queue<object>();
        Queue<string> stringJobs = new Queue<string>();

        Coroutine workRoutine = null;

        public void SafelyStopCoroutine(Coroutine routine)
        {
            if (routine != null)
            {
                StopCoroutine(routine);
            }
        }

        public static UnityThread Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject(nameof(UnityThread));
                    instance = go.AddComponent<UnityThread>();

                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        public bool HasPendingJob()
        {
            return workRoutine != null || jobs.Count > 0;
        }

        private void OnDestroy()
        {
            instance = null;
        }

        public void Log(string log)
        {
            stringJobs.Enqueue(log);
        }

        public void EnqueueTask(Action action)
        {
            jobs.Enqueue(action);
            Debug.Log($"{nameof(UnityThread)}: Queuing job. Has no active work? {workRoutine == null}. Number of Jobs {jobs.Count}");
        }

        public void EnqueueTask(IEnumerator enumerator)
        {
            jobs.Enqueue(enumerator);
            Debug.Log($"{nameof(UnityThread)}: Queuing job. Has no active work? {workRoutine == null}. Number of Jobs {jobs.Count}");
        }

        private void FixedUpdate()
        {
            if (workRoutine == null && jobs.Count > 0)
            {
                Debug.Log($"{nameof(UnityThread)}: No active work, creating work.. {jobs.Count}");
                try
                {

                    workRoutine = StartCoroutine(WorkRoutine());
                }
                catch (Exception e)
                {
                    Debug.LogError($"{nameof(UnityThread)}: something went wrong in coroutine, cancelling active task");
                    workRoutine = null;
                }
            }
        }

        private IEnumerator WorkRoutine()
        {
            Debug.Log($"{nameof(UnityThread)}: Doing god's work!");

            var res = jobs.Dequeue();
            yield return RunThrowingIterator(DoJob(res), ex =>
            {
                Debug.LogError($"{nameof(UnityThread)}: Something went wrong on when enqueuing coroutine task. Stopping current task. " +
                    $"\nThis error should not be ignored, check the recent enqueued task and identify the cause of the issue.  " +
                    $"\nIn some cases, this might be related to network or API related calls. See the Exception for details - {ex}, {ex.StackTrace}");
                workRoutine = null;
            });

            if (!jobs.TryPeek(out _))
            {
                workRoutine = null;
                Debug.Log($"{nameof(UnityThread)}: No active work routine. Clearing out.");
                yield break;
            }

            yield return WorkRoutine();
        }

        private IEnumerator RunThrowingIterator(IEnumerator enumerator, Action<Exception> exceptionCallback)
        {
            while (true)
            {
                object current;
                try
                {
                    Debug.Log($"{nameof(UnityThread)}: looping through objects");
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    current = enumerator.Current;
                }
                catch (Exception ex)
                {
                    exceptionCallback?.Invoke(ex);
                    yield break;
                }

                if (current is IEnumerator e)
                {
                    Exception error = null;
                    yield return RunThrowingIterator(e, ex =>
                    {
                        error = ex;
                    });

                    if (error != null)
                    {
                        exceptionCallback?.Invoke(error);
                        yield break;
                    }
                }
                else
                {
                    yield return current;
                }
            }
        }



        private IEnumerator DoJob(object res)
        {
            switch (res)
            {
                case Action action:
                    action?.Invoke();
                    yield return null;
                    break;
                case IEnumerator enumerator:
                    yield return enumerator;
                    break;
                default:
                    Debug.LogError($"{nameof(UnityThread)}: Undefined job");
                    yield return null;
                    break;
            }
        }
    }
}
