using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;

namespace NvidiaDisplayController.Global;

public class HotkeyManager : IDisposable
{
    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private const int WM_HOTKEY = 0x0312;
    
    private readonly IntPtr _windowHandle;
    private readonly Dictionary<int, Action> _hotkeyActions = new();
    private int _currentId = 1;

    public HotkeyManager(IntPtr windowHandle)
    {
        _windowHandle = windowHandle;
        ComponentDispatcher.ThreadFilterMessage += OnThreadFilterMessage;
    }

    public int RegisterHotkey(ModifierKeys modifiers, Key key, Action action)
    {
        var id = _currentId++;
        var virtualKey = KeyInterop.VirtualKeyFromKey(key);
        var modifierFlags = ConvertModifiers(modifiers);

        if (RegisterHotKey(_windowHandle, id, modifierFlags, (uint)virtualKey))
        {
            _hotkeyActions[id] = action;
            return id;
        }

        return -1;
    }

    public void UnregisterHotkey(int id)
    {
        if (_hotkeyActions.ContainsKey(id))
        {
            UnregisterHotKey(_windowHandle, id);
            _hotkeyActions.Remove(id);
        }
    }

    private void OnThreadFilterMessage(ref MSG msg, ref bool handled)
    {
        if (msg.message == WM_HOTKEY)
        {
            var id = msg.wParam.ToInt32();
            if (_hotkeyActions.TryGetValue(id, out var action))
            {
                action?.Invoke();
                handled = true;
            }
        }
    }

    private static uint ConvertModifiers(ModifierKeys modifiers)
    {
        uint modifierFlags = 0;
        if ((modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
            modifierFlags |= 0x0001; // MOD_ALT
        if ((modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            modifierFlags |= 0x0002; // MOD_CONTROL
        if ((modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
            modifierFlags |= 0x0004; // MOD_SHIFT
        if ((modifiers & ModifierKeys.Windows) == ModifierKeys.Windows)
            modifierFlags |= 0x0008; // MOD_WIN
        return modifierFlags;
    }

    public void Dispose()
    {
        ComponentDispatcher.ThreadFilterMessage -= OnThreadFilterMessage;
        foreach (var id in _hotkeyActions.Keys)
        {
            UnregisterHotKey(_windowHandle, id);
        }
        _hotkeyActions.Clear();
    }
}
