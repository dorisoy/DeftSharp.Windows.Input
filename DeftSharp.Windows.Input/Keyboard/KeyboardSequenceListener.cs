﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using DeftSharp.Windows.Input.Keyboard.Interceptors;
using DeftSharp.Windows.Input.Shared.Abstraction.Keyboard;
using DeftSharp.Windows.Input.Shared.Subscriptions;

namespace DeftSharp.Windows.Input.Keyboard;

public sealed class KeyboardSequenceListener: IDisposable
{
    private readonly IKeyboardSequenceListener _sequenceListener = new KeyboardSequenceListenerInterceptor();
    
    public bool IsListening => _sequenceListener.Subscriptions.Any();
    public IEnumerable<KeyboardSequenceSubscription> Subscriptions => _sequenceListener.Subscriptions;
    
    ~KeyboardSequenceListener() => Dispose();

    public KeyboardSequenceSubscription Subscribe(IEnumerable<Key> sequence, Action onClick, TimeSpan? intervalOfClick = null) 
        => _sequenceListener.Subscribe(sequence, onClick, intervalOfClick ?? TimeSpan.Zero);
    
    public KeyboardSequenceSubscription SubscribeOnce(IEnumerable<Key> sequence, Action onClick) 
        => _sequenceListener.SubscribeOnce(sequence, onClick);
    
    public void UnsubscribeAll() => _sequenceListener.UnsubscribeAll();

    public void Unsubscribe(Guid id) => _sequenceListener.Unsubscribe(id);

    public void Dispose() => _sequenceListener.Dispose();
}