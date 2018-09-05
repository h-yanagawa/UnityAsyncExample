using System;
using System.Threading;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Async : MonoBehaviour
{
    private int count = 0;
    private int rx1Count = 0;
    private int rx2Count = 0;

    public Button AsyncButton;
    public Button RxAsyncButton1;
    public Button RxAsyncButton2;

    public class Result { }

    void Awake()
    {
        AsyncButton.onClick.AddListener(async () =>
        {
            try
            {
                await HeavyProcessAsync(() => count++, "async");
                AsyncButton.GetComponentInChildren<Text>().text = $"async {count}";
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
            
        });

        RxAsyncButton1.onClick.AddListener(() =>
        {
            Task<Result> t = new Task<Result>(() =>
            {
                return HeavyProcess(() => rx1Count++, "Rx1");
            });

            t.ToObservable()
                .ObserveOnMainThread()
                .Subscribe(result =>
                {
                    RxAsyncButton1.GetComponentInChildren<Text>().text = $"Rx1 async {rx1Count}";
                });
        });

        RxAsyncButton2.onClick.AddListener(() =>
        {
            Observable.Start(() =>
                {
                    HeavyProcess(() => rx2Count++, "Rx2");
                }).ObserveOnMainThread()
                .Subscribe(result =>
                {
                    RxAsyncButton2.GetComponentInChildren<Text>().text = $"Rx2 async {rx2Count}";
                });
        });
    }

    async void Update()
    {

        await HeavyProcessAsync(() => { }, "aa", 100);
        transform.Rotate(0, 5, 0);
    }


    async Task<Result> HeavyProcessAsync(Action f, string arg, int wait = 1000)
    {
        var task = Task.Run(() => HeavyProcess(f, arg, wait));
        return await task;
    }

    Result HeavyProcess(Action f, string arg, int wait = 1000)
    {
        Debug.Log($"HeavyProcess {arg} start ThreadId = {Thread.CurrentThread.ManagedThreadId}");
        Thread.Sleep(1000);
        f();
        Debug.Log($"HeavyProcess {arg} done ThreadId = {Thread.CurrentThread.ManagedThreadId}");
        return new Result();
    }
}