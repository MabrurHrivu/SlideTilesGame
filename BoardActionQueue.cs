using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardActionQueue : MonoBehaviour
{
    private static BoardActionQueue instance;
    public static BoardActionQueue Instance => instance;

    private readonly Queue<IEnumerator> actions = new Queue<IEnumerator>();
    private bool isRunning;

    public bool IsRunning => isRunning;

    public event Action ActionStarted;
    public event Action ActionFinished;
    public event Action QueueFinished;

    public static BoardActionQueue GetOrCreate()
    {
        if (instance != null) return instance;

        instance = FindObjectOfType<BoardActionQueue>();
        if (instance != null) return instance;

        GameObject queueObject = new GameObject(nameof(BoardActionQueue));
        instance = queueObject.AddComponent<BoardActionQueue>();
        return instance;
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void Enqueue(IEnumerator action)
    {
        if (action == null) return;

        actions.Enqueue(action);

        if (!isRunning)
        {
            isRunning = true;
            StartCoroutine(ProcessQueue());
        }
    }

    public void Enqueue(Action action)
    {
        if (action == null) return;
        Enqueue(RunAction(action));
    }

    private IEnumerator ProcessQueue()
    {
        while (actions.Count > 0)
        {
            IEnumerator action = actions.Dequeue();
            ActionStarted?.Invoke();
            yield return StartCoroutine(action);
            ActionFinished?.Invoke();
        }

        isRunning = false;
        QueueFinished?.Invoke();
    }

    private IEnumerator RunAction(Action action)
    {
        action.Invoke();
        yield break;
    }
}
