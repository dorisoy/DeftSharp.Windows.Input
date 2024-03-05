﻿using System;
using System.Linq;
using DeftSharp.Windows.Input.InteropServices.Mouse;
using DeftSharp.Windows.Input.Shared.Interceptors;
using DeftSharp.Windows.Input.Shared.Listeners;
using DeftSharp.Windows.Input.Shared.Subscriptions;

namespace DeftSharp.Windows.Input.Mouse;

public sealed class MouseListener : InputListener<MouseSubscription>, IDisposable
{
    private readonly IMouseInterceptor _mouseInterceptor;

    public MouseListener()
    {
        _mouseInterceptor = WindowsMouseInterceptor.Instance;
        _mouseInterceptor.MouseInput += OnMouseInput;
        _mouseInterceptor.UnhookRequested += OnInterceptorUnhookRequested;
    }

    private bool OnInterceptorUnhookRequested() => !_subscriptions.Any();

    public Coordinates GetPosition() => _mouseInterceptor.GetPosition();

    public void Subscribe(MouseEvent mouseEvent, Action onAction, TimeSpan? intervalOfClick = null)
    {
        var subscription = new MouseSubscription(mouseEvent, onAction, intervalOfClick ?? TimeSpan.Zero);
        _subscriptions.Add(subscription);
    }

    public void SubscribeOnce(MouseEvent mouseEvent, Action onAction)
    {
        var subscription = new MouseSubscription(mouseEvent, onAction, true);
        _subscriptions.Add(subscription);
    }

    public void Unsubscribe(MouseEvent mouseEvent)
    {
        var subscriptions = _subscriptions.Where(e => e.Event.Equals(mouseEvent)).ToArray();

        foreach (var mouseSubscription in subscriptions)
            _subscriptions.Remove(mouseSubscription);
    }

    public void Unsubscribe(Guid id)
    {
        var mouseEvent = _subscriptions.FirstOrDefault(s => s.Id == id);

        if (mouseEvent is null)
            return;

        _subscriptions.Remove(mouseEvent);
    }

    public void Dispose()
    {
        Unregister();
        _mouseInterceptor.MouseInput -= OnMouseInput;
        _mouseInterceptor.UnhookRequested -= OnInterceptorUnhookRequested;
    }

    protected override void Register()
    {
        if (IsListening)
            return;

        _mouseInterceptor.Hook();
        base.Register();
    }

    protected override void Unregister()
    {
        if (!IsListening)
            return;

        UnsubscribeAll();
        _mouseInterceptor.Unhook();
        base.Unregister();
    }

    private void OnMouseInput(object? sender, MouseInputArgs e)
    {
        var mouseEvents =
            _subscriptions.Where(s => s.Event.Equals(e.Event)).ToArray();

        foreach (var mouseEvent in mouseEvents)
        {
            if (mouseEvent.SingleUse)
                Unsubscribe(mouseEvent.Id);

            mouseEvent.Invoke();
        }
    }
}