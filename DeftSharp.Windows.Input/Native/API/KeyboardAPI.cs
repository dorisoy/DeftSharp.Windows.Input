﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using DeftSharp.Windows.Input.Keyboard;
using DeftSharp.Windows.Input.Shared.Exceptions;
using static DeftSharp.Windows.Input.Native.User32;
using static DeftSharp.Windows.Input.Native.Kernel32;
using static DeftSharp.Windows.Input.Native.System.InputMessages;
using static DeftSharp.Windows.Input.Native.SystemEvents;

namespace DeftSharp.Windows.Input.Native;

/// <summary>
/// Provides methods for simulating keyboard input using Windows API.
/// </summary>
internal static class KeyboardAPI
{
    /// <summary>
    /// Retrieves the type of keyboard hardware.
    /// </summary>
    /// <returns>
    /// The type of keyboard hardware as a <see cref="KeyboardType"/> object.
    /// </returns>
    internal static KeyboardType GetKeyboardType()
    {
        const int keyboardTypeFlag = 0;

        var keyboardTypeValue = User32.GetKeyboardType(keyboardTypeFlag);

        var keyboardName = keyboardTypeValue switch
        {
            1 => "IBM PC/XT or compatible (83-key) keyboard",
            2 => "Olivetti \"ICO\" (102-key) keyboard ",
            3 => "IBM PC/AT (84-key) or similar keyboard",
            4 => "IBM enhanced (101- or 102-key) keyboard",
            5 => "Nokia 1050 and similar keyboards",
            6 => "Nokia 9140 and similar keyboards",
            7 => "Japanese keyboard",
            _ => "Unknown"
        };

        return new KeyboardType(keyboardTypeValue, keyboardName);
    }

    /// <summary>
    /// Retrieves the current keyboard layout.
    /// </summary>
    /// <returns>
    /// The <see cref="KeyboardLayout"/> object representing the current keyboard layout.
    /// </returns>
    internal static KeyboardLayout GetLayout()
    {
        var layoutHandle = GetLayoutHandle();

        var lcid = layoutHandle.ToInt32() & 0xFFFF;

        var culture = new CultureInfo(lcid);

        return new KeyboardLayout(culture.KeyboardLayoutId, lcid, culture.Name, culture.DisplayName);
    }

    /// <summary>
    /// Determines whether the specified key is currently active.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns>True if the specified key is currently active; otherwise, false.</returns>
    internal static bool IsKeyActive(Key key)
    {
        var keyCode = (byte)KeyInterop.VirtualKeyFromKey(key);
        var keyState = GetKeyState(keyCode);
        return (keyState & KeyActiveFlag) != 0;
    }

    /// <summary>
    /// Determines whether the specified key is currently pressed.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns>True if the specified key is currently pressed; otherwise, false.</returns>
    internal static bool IsKeyPressed(Key key)
    {
        var keyCode = (byte)KeyInterop.VirtualKeyFromKey(key);
        var keyState = GetAsyncKeyState(keyCode);
        return (keyState & KeyPressedFlag) != 0;
    }

    /// <summary>
    /// Presses the specified key.
    /// </summary>
    /// <param name="key">The key to press.</param>
    /// <exception cref="KeyboardPressException">Thrown if the key press simulation fails.</exception>
    internal static void Press(Key key)
    {
        var keyCode = (byte)KeyInterop.VirtualKeyFromKey(key);
        var result = SimulateKeyPress(keyCode);
        if (!result)
            throw new KeyboardPressException(key);
    }

    /// <summary>
    /// Presses the specified keys synchronously.
    /// </summary>
    /// <param name="keys">The keys to press.</param>
    internal static void PressSynchronously(IEnumerable<Key> keys)
    {
        var inputs = keys
            .Distinct()
            .Select(k => (byte)KeyInterop.VirtualKeyFromKey(k))
            .Select(code => CreateInput(code))
            .ToArray();

        SendInput(inputs);

        for (var i = 0; i < inputs.Length; i++)
            inputs[i].u.ki.dwFlags = InputKeyUp;

        SendInput(inputs);
    }

    /// <summary>
    /// Simulates a keyboard input event for the specified key.
    /// </summary>
    /// <param name="key">The key to simulate.</param>
    /// <param name="keyboardEvent">The type of keyboard input event.</param>
    /// <exception cref="NotImplementedException">Thrown if the specified keyboard event is not implemented.</exception>
    internal static void Simulate(Key key, KeyboardInputEvent keyboardEvent)
    {
        var keyCode = (byte)KeyInterop.VirtualKeyFromKey(key);

        switch (keyboardEvent)
        {
            case KeyboardInputEvent.KeyDown:
                SimulateKeyDown(keyCode);
                break;
            case KeyboardInputEvent.KeyUp:
                SimulateKeyUp(keyCode);
                break;
            default:
                throw new NotImplementedException(nameof(keyboardEvent));
        }
    }

    /// <summary>
    /// Retrieves the handle to the current keyboard layout.
    /// </summary>
    internal static IntPtr GetLayoutHandle()
        => GetKeyboardLayout(GetCurrentThreadId());

    /// <summary>
    /// Simulates pressing a key with the specified virtual key code.
    /// </summary>
    /// <param name="keyCode">The virtual key code of the key to simulate pressing.</param>
    /// <returns>True if the key press simulation was successful; otherwise, false.</returns>
    private static bool SimulateKeyPress(ushort keyCode)
        => SimulateKeyDown(keyCode) && SimulateKeyUp(keyCode);

    /// <summary>
    /// Simulates a key down event for the specified virtual key code.
    /// </summary>
    /// <param name="keyCode">The virtual key code of the key to simulate.</param>
    /// <returns>
    /// <c>true</c> if the function succeeds, <c>false</c> otherwise. To get extended error information, call GetLastError.
    /// </returns>
    private static bool SimulateKeyDown(ushort keyCode)
    {
        var input = CreateInput(keyCode, InputKeyDown);
        return SendInput(input);
    }

    /// <summary>
    /// Simulates a key up event for the specified virtual key code.
    /// </summary>
    /// <param name="keyCode">The virtual key code of the key to simulate.</param>
    /// <returns>
    /// <c>true</c> if the function succeeds, <c>false</c> otherwise. To get extended error information, call GetLastError.
    /// </returns>
    private static bool SimulateKeyUp(ushort keyCode)
    {
        var input = CreateInput(keyCode, InputKeyUp);
        return SendInput(input);
    }

    /// <summary>
    /// Creates an INPUT structure representing a keyboard input event with the specified virtual key code.
    /// </summary>
    /// <param name="keyCode">The virtual key code of the key.</param>
    /// <param name="dwFlags">Flags that specify various aspects of function operation.</param>
    /// <returns>The created INPUT structure.</returns>
    private static System.Input CreateInput(ushort keyCode, uint dwFlags = InputKeyDown)
    {
        var input = new System.Input(InputKeyboard);
        input.u.ki.wVk = keyCode;
        input.u.ki.dwFlags = dwFlags;
        return input;
    }
}