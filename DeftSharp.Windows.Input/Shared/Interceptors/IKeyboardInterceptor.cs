﻿using System;
using System.Collections.Generic;
using System.Windows.Input;
using DeftSharp.Windows.Input.InteropServices.Keyboard;

namespace DeftSharp.Windows.Input.Shared.Interceptors;

public interface IKeyboardInterceptor : IRequestedInterceptor
{
    void Prevent(Key key);
    void Release(Key key);
    void ReleaseAll();
    
    IEnumerable<Key> LockedKeys { get;}

    event Action<Key>? KeyPrevented;
    event Action<Key>? KeyReleased;
    event EventHandler<KeyPressedArgs>? KeyPressed;
}