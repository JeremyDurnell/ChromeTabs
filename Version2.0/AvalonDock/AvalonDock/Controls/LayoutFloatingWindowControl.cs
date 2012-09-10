﻿//Copyright (c) 2007-2012, Adolfo Marinucci
//All rights reserved.

//Redistribution and use in source and binary forms, with or without modification, are permitted provided that the 
//following conditions are met:

//* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

//* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following 
//disclaimer in the documentation and/or other materials provided with the distribution.

//* Neither the name of Adolfo Marinucci nor the names of its contributors may be used to endorse or promote products
//derived from this software without specific prior written permission.

//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
//INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
//IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, 
//EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, 
//STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, 
//EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using AvalonDock.Layout;
using System.Diagnostics;

namespace AvalonDock.Controls
{
    public abstract class LayoutFloatingWindowControl : Window, ILayoutControl
    {
        static LayoutFloatingWindowControl()
        {
            LayoutFloatingWindowControl.ContentProperty.OverrideMetadata(typeof(LayoutFloatingWindowControl), new FrameworkPropertyMetadata(null, null, new CoerceValueCallback(CoerceContentValue)));
            AllowsTransparencyProperty.OverrideMetadata(typeof(LayoutFloatingWindowControl), new FrameworkPropertyMetadata(false));
            ShowInTaskbarProperty.OverrideMetadata(typeof(LayoutFloatingWindowControl), new FrameworkPropertyMetadata(false));
        } 

        static object CoerceContentValue(DependencyObject sender, object content)
        {
            return new FloatingWindowContentHost(sender as LayoutFloatingWindowControl) { Content = content as UIElement };
        }

        protected class FloatingWindowContentHost : HwndHost
        {
            LayoutFloatingWindowControl _owner;
            public FloatingWindowContentHost(LayoutFloatingWindowControl owner)
            {
                _owner = owner;
                var manager = _owner.Model.Root.Manager;
            }


            HwndSource _wpfContentHost = null;
            Border _rootPresenter = null;
            DockingManager _manager = null;

            protected override System.Runtime.InteropServices.HandleRef BuildWindowCore(System.Runtime.InteropServices.HandleRef hwndParent)
            {
                _wpfContentHost = new HwndSource(new HwndSourceParameters()
                {
                    ParentWindow = hwndParent.Handle,
                    WindowStyle = Win32Helper.WS_CHILD | Win32Helper.WS_VISIBLE | Win32Helper.WS_CLIPSIBLINGS | Win32Helper.WS_CLIPCHILDREN,
                    Width = 1,
                    Height = 1
                });

                _rootPresenter = new Border() { Child = Content, Focusable = true };
                _rootPresenter.GotKeyboardFocus += new KeyboardFocusChangedEventHandler(_rootPresenter_GotKeyboardFocus);
                _rootPresenter.SetBinding(Border.BackgroundProperty, new Binding("Background") { Source = _owner });
                _wpfContentHost.RootVisual = _rootPresenter;
                _wpfContentHost.SizeToContent = SizeToContent.Manual;
                _manager = _owner.Model.Root.Manager;
                ((ILogicalChildrenContainer)_manager).InternalAddLogicalChild(_rootPresenter);

                return new HandleRef(this, _wpfContentHost.Handle);
            }

            void _rootPresenter_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
            {
                Debug.WriteLine("_rootPresenter_GotKeyboardFocus");
            }

            protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
            {
                Debug.WriteLine("FloatingWindowContentHost.GotKeyboardFocus");
                base.OnGotKeyboardFocus(e);
            }
            protected override void DestroyWindowCore(HandleRef hwnd)
            {
                ((ILogicalChildrenContainer)_manager).InternalRemoveLogicalChild(_rootPresenter);
                if (_wpfContentHost != null)
                {
                    _wpfContentHost.Dispose();
                    _wpfContentHost = null;
                }
            }
            protected override IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
            {
                switch (msg)
                {
                    case Win32Helper.WM_SETFOCUS:
                        Debug.WriteLine("FloatingWindowContentHost.WM_SETFOCUS");
                        break;
                    case Win32Helper.WM_KILLFOCUS:
                        Debug.WriteLine("FloatingWindowContentHost.WM_KILLFOCUS");
                        break;
                }                
                return base.WndProc(hwnd, msg, wParam, lParam, ref handled);
            }

            public Visual RootVisual
            {
                get { return _rootPresenter; }
            }

            protected override Size MeasureOverride(Size constraint)
            {
                if (Content == null)
                    return base.MeasureOverride(constraint);

                Content.Measure(constraint);
                return Content.DesiredSize;
            }

            #region Content

            /// <summary>
            /// Content Dependency Property
            /// </summary>
            public static readonly DependencyProperty ContentProperty =
                DependencyProperty.Register("Content", typeof(UIElement), typeof(FloatingWindowContentHost),
                    new FrameworkPropertyMetadata((UIElement)null,
                        new PropertyChangedCallback(OnContentChanged)));

            /// <summary>
            /// Gets or sets the Content property.  This dependency property 
            /// indicates ....
            /// </summary>
            public UIElement Content
            {
                get { return (UIElement)GetValue(ContentProperty); }
                set { SetValue(ContentProperty, value); }
            }

            /// <summary>
            /// Handles changes to the Content property.
            /// </summary>
            private static void OnContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            {
                ((FloatingWindowContentHost)d).OnContentChanged(e);
            }

            /// <summary>
            /// Provides derived classes an opportunity to handle changes to the Content property.
            /// </summary>
            protected virtual void OnContentChanged(DependencyPropertyChangedEventArgs e)
            {
                if (_rootPresenter != null)
                    _rootPresenter.Child = Content;
            }

            #endregion
        }

        protected LayoutFloatingWindowControl(ILayoutElement model)
        {
            this.Loaded += new RoutedEventHandler(OnLoaded);
            this.Unloaded += new RoutedEventHandler(OnUnloaded);
        }

        protected override void OnClosed(EventArgs e)
        {
            if (Content != null)
            {
                var host = Content as FloatingWindowContentHost;
                host.Dispose();

                _hwndSrc.RemoveHook(_hwndSrcHook);
                _hwndSrc.Dispose();
            }

            base.OnClosed(e);
        }

        bool _attachDrag = false;
        internal void AttachDrag()
        {
            _attachDrag = true;
            this.Activated += new EventHandler(OnActivated);
            
        }

        HwndSource _hwndSrc;
        HwndSourceHook _hwndSrcHook;

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= new RoutedEventHandler(OnLoaded);

            _hwndSrc = HwndSource.FromDependencyObject(this) as HwndSource;
            _hwndSrcHook = new HwndSourceHook(FilterMessage);
            _hwndSrc.AddHook(_hwndSrcHook);            
        }

        void OnUnloaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded -= new RoutedEventHandler(OnUnloaded);

            _hwndSrc.RemoveHook(_hwndSrcHook);
            _hwndSrc.Dispose();
            _hwndSrc = null;
        }

        void OnActivated(object sender, EventArgs e)
        {
            this.Activated -= new EventHandler(OnActivated);

            if (_attachDrag && Mouse.LeftButton == MouseButtonState.Pressed)
            {
                IntPtr windowHandle = new WindowInteropHelper(this).Handle;
                var mousePosition = this.PointToScreenDPI(Mouse.GetPosition(this));
                var clientArea = Win32Helper.GetClientRect(windowHandle);
                var windowArea = Win32Helper.GetWindowRect(windowHandle);

                Left = mousePosition.X - windowArea.Width / 2.0;
                Top = mousePosition.Y - (windowArea.Height - clientArea.Height) / 2.0;
                _attachDrag = false;

                int lParam = ((int)mousePosition.X & (int)0xFFFF) | (((int)mousePosition.Y) << 16);
                Win32Helper.SendMessage(windowHandle, Win32Helper.WM_NCLBUTTONDOWN, Win32Helper.HT_CAPTION, lParam);
            }
        }


        protected override void OnInitialized(EventArgs e)
        {
            CommandBindings.Add(new CommandBinding(Microsoft.Windows.Shell.SystemCommands.CloseWindowCommand,
                new ExecutedRoutedEventHandler((s, args) => Microsoft.Windows.Shell.SystemCommands.CloseWindow((Window)args.Parameter))));
            CommandBindings.Add(new CommandBinding(Microsoft.Windows.Shell.SystemCommands.MaximizeWindowCommand,
                new ExecutedRoutedEventHandler((s, args) => Microsoft.Windows.Shell.SystemCommands.MaximizeWindow((Window)args.Parameter))));
            CommandBindings.Add(new CommandBinding(Microsoft.Windows.Shell.SystemCommands.MinimizeWindowCommand,
                new ExecutedRoutedEventHandler((s, args) => Microsoft.Windows.Shell.SystemCommands.MinimizeWindow((Window)args.Parameter))));
            CommandBindings.Add(new CommandBinding(Microsoft.Windows.Shell.SystemCommands.RestoreWindowCommand,
                new ExecutedRoutedEventHandler((s, args) => Microsoft.Windows.Shell.SystemCommands.RestoreWindow((Window)args.Parameter))));
            Debug.Assert(this.Owner != null);
            base.OnInitialized(e);
        }

        public abstract ILayoutElement Model { get; }


        #region IsDragging

        /// <summary>
        /// IsDragging Read-Only Dependency Property
        /// </summary>
        private static readonly DependencyPropertyKey IsDraggingPropertyKey
            = DependencyProperty.RegisterReadOnly("IsDragging", typeof(bool), typeof(LayoutFloatingWindowControl),
                new FrameworkPropertyMetadata((bool)false,
                    new PropertyChangedCallback(OnIsDraggingChanged)));

        public static readonly DependencyProperty IsDraggingProperty
            = IsDraggingPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the IsDragging property.  This dependency property 
        /// indicates that this floating window is being dragged.
        /// </summary>
        public bool IsDragging
        {
            get { return (bool)GetValue(IsDraggingProperty); }
        }

        /// <summary>
        /// Provides a secure method for setting the IsDragging property.  
        /// This dependency property indicates that this floating window is being dragged.
        /// </summary>
        /// <param name="value">The new value for the property.</param>
        protected void SetIsDragging(bool value)
        {
            SetValue(IsDraggingPropertyKey, value);
        }

        /// <summary>
        /// Handles changes to the IsDragging property.
        /// </summary>
        private static void OnIsDraggingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LayoutFloatingWindowControl)d).OnIsDraggingChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the IsDragging property.
        /// </summary>
        protected virtual void OnIsDraggingChanged(DependencyPropertyChangedEventArgs e)
        {
            //Debug.WriteLine("IsDragging={0}", e.NewValue);
        }

        #endregion



        DragService _dragService = null;

        void UpdatePositionAndSizeOfPanes()
        {
            foreach (var posElement in Model.Descendents().OfType<ILayoutElementForFloatingWindow>())
            {
                posElement.FloatingLeft = Left;
                posElement.FloatingTop = Top;
                posElement.FloatingWidth = Width;
                posElement.FloatingHeight = Height;
                posElement.IsMaximized = this.WindowState == System.Windows.WindowState.Maximized;
            }
        }

        protected virtual IntPtr FilterMessage(
            IntPtr hwnd,
            int msg,
            IntPtr wParam,
            IntPtr lParam,
            ref bool handled
            )
        {
            handled = false;

            switch (msg)
            {
                case Win32Helper.WM_ACTIVATE:
                    if (((int)wParam & 0xFFFF) == Win32Helper.WA_INACTIVE)
                    {
                        if (lParam == new WindowInteropHelper(this.Owner).Handle)
                        {
                            Win32Helper.SetActiveWindow(_hwndSrc.Handle);
                            handled = true;
                        }
                    }
                    break;
                case Win32Helper.WM_EXITSIZEMOVE:
                    UpdatePositionAndSizeOfPanes();

                    if (_dragService != null)
                    {
                        bool dropFlag;
                        var mousePosition = this.TransformToDeviceDPI(Win32Helper.GetMousePosition());
                        _dragService.Drop(mousePosition, out dropFlag);
                        _dragService = null;
                        SetIsDragging(false);

                        if (dropFlag)
                            InternalClose();
                    }
                    
                    break;
                case Win32Helper.WM_MOVING:
                    {
                        if (_dragService == null)
                        {
                            _dragService = new DragService(this);
                            SetIsDragging(true);
                        }

                        var mousePosition = this.TransformToDeviceDPI(Win32Helper.GetMousePosition());
                        _dragService.UpdateMouseLocation(mousePosition);
                    }
                    break;
                case Win32Helper.WM_LBUTTONUP: //set as handled right button click on title area (after showing context menu)
                    if (_dragService != null && Mouse.LeftButton == MouseButtonState.Released)
                    {
                        _dragService.Abort();
                        _dragService = null;
                        SetIsDragging(false);
                    }
                    break;
            }

             

            return IntPtr.Zero;
        }

        bool _internalCloseFlag = false;

        internal void InternalClose()
        {
            _internalCloseFlag = true;
            Close();
        }

        protected bool CloseInitiatedByUser
        {
            get { return !_internalCloseFlag; }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
        }

        #region IsMaximized

        /// <summary>
        /// IsMaximized Read-Only Dependency Property
        /// </summary>
        private static readonly DependencyPropertyKey IsMaximizedPropertyKey
            = DependencyProperty.RegisterReadOnly("IsMaximized", typeof(bool), typeof(LayoutFloatingWindowControl),
                new FrameworkPropertyMetadata((bool)false));

        public static readonly DependencyProperty IsMaximizedProperty
            = IsMaximizedPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the IsMaximized property.  This dependency property 
        /// indicates if the window is maximized.
        /// </summary>
        public bool IsMaximized
        {
            get { return (bool)GetValue(IsMaximizedProperty); }
        }

        /// <summary>
        /// Provides a secure method for setting the IsMaximized property.  
        /// This dependency property indicates if the window is maximized.
        /// </summary>
        /// <param name="value">The new value for the property.</param>
        protected void SetIsMaximized(bool value)
        {
            SetValue(IsMaximizedPropertyKey, value);
        }

        protected override void OnStateChanged(EventArgs e)
        {
            SetIsMaximized(WindowState == System.Windows.WindowState.Maximized);
            base.OnStateChanged(e);
        }

        #endregion





        
        
    }
}
