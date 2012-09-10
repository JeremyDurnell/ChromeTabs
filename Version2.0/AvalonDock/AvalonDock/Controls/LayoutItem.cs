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
using AvalonDock.Layout;
using System.Windows.Input;
using AvalonDock.Commands;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Media;

namespace AvalonDock.Controls
{
    public abstract class LayoutItem : FrameworkElement
    {
        static LayoutItem()
        { 
            ToolTipProperty.OverrideMetadata(typeof(LayoutItem), new FrameworkPropertyMetadata(null, (s, e) => OnToolTipChanged(s,e)));
            VisibilityProperty.OverrideMetadata(typeof(LayoutItem), new FrameworkPropertyMetadata(Visibility.Visible, (s,e) => OnVisibilityChanged(s, e)));
        }


        internal LayoutItem()
        {

        }

        internal virtual void Attach(LayoutContent model)
        { 
            LayoutElement = model;
            Model = model.Content;

            InitDefaultCommands();

            LayoutElement.IsSelectedChanged+=new EventHandler(LayoutElement_IsSelectedChanged);
            LayoutElement.IsActiveChanged+=new EventHandler(LayoutElement_IsActiveChanged);
            
            DataContext = this;
        }



        void LayoutElement_IsActiveChanged(object sender, EventArgs e)
        {
            if (_isActiveReentrantFlag.CanEnter)
            {
                using (_isActiveReentrantFlag.Enter())
                {
                    var bnd = BindingOperations.GetBinding(this, IsActiveProperty);
                    IsActive = LayoutElement.IsActive;
                    var bnd2 = BindingOperations.GetBinding(this, IsActiveProperty);
                }
            }
        }

        void LayoutElement_IsSelectedChanged(object sender, EventArgs e)
        {
            if (_isSelectedReentrantFlag.CanEnter)
            {
                using (_isSelectedReentrantFlag.Enter())
                {
                    IsSelected = LayoutElement.IsSelected;
                }
            }
        }

        internal virtual void Detach()
        {
            LayoutElement.IsSelectedChanged -= new EventHandler(LayoutElement_IsSelectedChanged);
            LayoutElement.IsActiveChanged -= new EventHandler(LayoutElement_IsActiveChanged);
        }

        public LayoutContent LayoutElement
        {
            get;
            private set;
        }

        public object Model
        {
            get;
            private set;
        }

        ICommand _defaultCloseCommand;
        ICommand _defaultFloatCommand;
        ICommand _defaultDockAsDocumentCommand;
        ICommand _defaultCloseAllButThisCommand;
        ICommand _defaultActivateCommand;
        ICommand _defaultNewVerticalTabGroupCommand;
        ICommand _defaultNewHorizontalTabGroupCommand;
        ICommand _defaultMoveToNextTabGroupCommand;
        ICommand _defaultMoveToPreviousTabGroupCommand;

        protected virtual void InitDefaultCommands()
        {
            _defaultCloseCommand = new RelayCommand((p) => ExecuteCloseCommand(p), (p) => CanExecuteCloseCommand(p));
            _defaultFloatCommand = new RelayCommand((p) => ExecuteFloatCommand(p), (p) => CanExecuteFloatCommand(p));
            _defaultDockAsDocumentCommand = new RelayCommand((p) => ExecuteDockAsDocumentCommand(p), (p) => CanExecuteDockAsDocumentCommand(p));
            _defaultCloseAllButThisCommand = new RelayCommand((p) => ExecuteCloseAllButThisCommand(p), (p) => CanExecuteCloseAllButThisCommand(p));
            _defaultActivateCommand = new RelayCommand((p) => ExecuteActivateCommand(p), (p) => CanExecuteActivateCommand(p));
            _defaultNewVerticalTabGroupCommand = new RelayCommand((p) => ExecuteNewVerticalTabGroupCommand(p), (p) => CanExecuteNewVerticalTabGroupCommand(p));
            _defaultNewHorizontalTabGroupCommand = new RelayCommand((p) => ExecuteNewHorizontalTabGroupCommand(p), (p) => CanExecuteNewHorizontalTabGroupCommand(p));
            _defaultMoveToNextTabGroupCommand = new RelayCommand((p) => ExecuteMoveToNextTabGroupCommand(p), (p) => CanExecuteMoveToNextTabGroupCommand(p));
            _defaultMoveToPreviousTabGroupCommand = new RelayCommand((p) => ExecuteMoveToPreviousTabGroupCommand(p), (p) => CanExecuteMoveToPreviousTabGroupCommand(p));
        }

        protected virtual void ClearDefaultBindings()
        {
            if (CloseCommand == _defaultCloseCommand)
                BindingOperations.ClearBinding(this, CloseCommandProperty);
            if (FloatCommand == _defaultFloatCommand)
                BindingOperations.ClearBinding(this, FloatCommandProperty);
            if (DockAsDocumentCommand == _defaultDockAsDocumentCommand)
                BindingOperations.ClearBinding(this, DockAsDocumentCommandProperty);
            if (CloseAllButThisCommand == _defaultCloseAllButThisCommand)
                BindingOperations.ClearBinding(this, CloseAllButThisCommandProperty);
            if (ActivateCommand == _defaultActivateCommand)
                BindingOperations.ClearBinding(this, ActivateCommandProperty);
            if (NewVerticalTabGroupCommand == _defaultNewVerticalTabGroupCommand)
                BindingOperations.ClearBinding(this, NewVerticalTabGroupCommandProperty);
            if (NewHorizontalTabGroupCommand == _defaultNewHorizontalTabGroupCommand)
                BindingOperations.ClearBinding(this, NewHorizontalTabGroupCommandProperty);
            if (MoveToNextTabGroupCommand == _defaultMoveToNextTabGroupCommand)
                BindingOperations.ClearBinding(this, MoveToNextTabGroupCommandProperty);
            if (MoveToPreviousTabGroupCommand == _defaultMoveToPreviousTabGroupCommand)
                BindingOperations.ClearBinding(this, MoveToPreviousTabGroupCommandProperty);
        }

        protected virtual void SetDefaultBindings()
        {
            if (CloseCommand == null)
                CloseCommand = _defaultCloseCommand;
            if (FloatCommand == null)
                FloatCommand = _defaultFloatCommand;
            if (DockAsDocumentCommand == null)
                DockAsDocumentCommand = _defaultDockAsDocumentCommand;
            if (CloseAllButThisCommand == null)
                CloseAllButThisCommand = _defaultCloseAllButThisCommand;
            if (ActivateCommand == null)
                ActivateCommand = _defaultActivateCommand;
            if (NewVerticalTabGroupCommand == null)
                NewVerticalTabGroupCommand = _defaultNewVerticalTabGroupCommand;
            if (NewHorizontalTabGroupCommand == null)
                NewHorizontalTabGroupCommand = _defaultNewHorizontalTabGroupCommand;
            if (MoveToNextTabGroupCommand == null)
                MoveToNextTabGroupCommand = _defaultMoveToNextTabGroupCommand;
            if (MoveToPreviousTabGroupCommand == null)
                MoveToPreviousTabGroupCommand = _defaultMoveToPreviousTabGroupCommand;


            IsSelected = LayoutElement.IsSelected;
            IsActive = LayoutElement.IsActive;
        }



        internal void _ClearDefaultBindings()
        {
            ClearDefaultBindings();
        }

        internal void _SetDefaultBindings()
        {
            SetDefaultBindings();
        }

        #region Title

        /// <summary>
        /// Title Dependency Property
        /// </summary>
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(LayoutItem),
                new FrameworkPropertyMetadata((string)null,
                    new PropertyChangedCallback(OnTitleChanged)));

        /// <summary>
        /// Gets or sets the Title property.  This dependency property 
        /// indicates the title of the element.
        /// </summary>
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        /// <summary>
        /// Handles changes to the Title property.
        /// </summary>
        private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LayoutItem)d).OnTitleChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the Title property.
        /// </summary>
        protected virtual void OnTitleChanged(DependencyPropertyChangedEventArgs e)
        {
            LayoutElement.Title = (string)e.NewValue;
        }

        #endregion

        #region ToolTip
        private static void OnToolTipChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
        {
            ((LayoutItem)s).OnToolTipChanged();
        }
        
        private void OnToolTipChanged()
        {
            LayoutElement.ToolTip = ToolTip;
        }
        #endregion

        #region IconSource

        /// <summary>
        /// IconSource Dependency Property
        /// </summary>
        public static readonly DependencyProperty IconSourceProperty =
            DependencyProperty.Register("IconSource", typeof(ImageSource), typeof(LayoutItem),
                new FrameworkPropertyMetadata((ImageSource)null,
                    new PropertyChangedCallback(OnIconSourceChanged)));

        /// <summary>
        /// Gets or sets the IconSource property.  This dependency property 
        /// indicates icon associated with the item.
        /// </summary>
        public ImageSource IconSource
        {
            get { return (ImageSource)GetValue(IconSourceProperty); }
            set { SetValue(IconSourceProperty, value); }
        }

        /// <summary>
        /// Handles changes to the IconSource property.
        /// </summary>
        private static void OnIconSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LayoutItem)d).OnIconSourceChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the IconSource property.
        /// </summary>
        protected virtual void OnIconSourceChanged(DependencyPropertyChangedEventArgs e)
        {
            LayoutElement.IconSource = IconSource;
        }

        #endregion
        
        #region Visibility

        private static void OnVisibilityChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
        {
            ((LayoutItem)s).OnVisibilityChanged();
        }

        protected virtual void OnVisibilityChanged()
        {
            if (Visibility == System.Windows.Visibility.Collapsed)
                LayoutElement.Close();
        }

        #endregion

        #region ContentId

        /// <summary>
        /// ContentId Dependency Property
        /// </summary>
        public static readonly DependencyProperty ContentIdProperty =
            DependencyProperty.Register("ContentId", typeof(string), typeof(LayoutItem),
                new FrameworkPropertyMetadata((string)null,
                    new PropertyChangedCallback(OnContentIdChanged)));

        /// <summary>
        /// Gets or sets the ContentId property.  This dependency property 
        /// indicates the content id used to retrive content when deserializing layouts.
        /// </summary>
        public string ContentId
        {
            get { return (string)GetValue(ContentIdProperty); }
            set { SetValue(ContentIdProperty, value); }
        }

        /// <summary>
        /// Handles changes to the ContentId property.
        /// </summary>
        private static void OnContentIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LayoutItem)d).OnContentIdChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the ContentId property.
        /// </summary>
        protected virtual void OnContentIdChanged(DependencyPropertyChangedEventArgs e)
        {
            LayoutElement.ContentId = (string)e.NewValue;
        }

        #endregion

        #region IsSelected

        ReentrantFlag _isSelectedReentrantFlag = new ReentrantFlag();

        /// <summary>
        /// IsSelected Dependency Property
        /// </summary>
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(LayoutItem),
                new FrameworkPropertyMetadata((bool)false,
                    new PropertyChangedCallback(OnIsSelectedChanged)));

        /// <summary>
        /// Gets or sets the IsSelected property.  This dependency property 
        /// indicates if the item is selected inside its container.
        /// </summary>
        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        /// <summary>
        /// Handles changes to the IsSelected property.
        /// </summary>
        private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LayoutItem)d).OnIsSelectedChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the IsSelected property.
        /// </summary>
        protected virtual void OnIsSelectedChanged(DependencyPropertyChangedEventArgs e)
        {
            if (_isSelectedReentrantFlag.CanEnter)
            {
                using (_isSelectedReentrantFlag.Enter())
                {
                    LayoutElement.IsSelected = (bool)e.NewValue;
                }
            }
        }

        #endregion

        #region IsActive

        ReentrantFlag _isActiveReentrantFlag = new ReentrantFlag();

        /// <summary>
        /// IsActive Dependency Property
        /// </summary>
        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register("IsActive", typeof(bool), typeof(LayoutItem),
                new FrameworkPropertyMetadata((bool)false,
                    new PropertyChangedCallback(OnIsActiveChanged)));

        /// <summary>
        /// Gets or sets the IsActive property.  This dependency property 
        /// indicates if the item is active in the UI.
        /// </summary>
        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }

        /// <summary>
        /// Handles changes to the IsActive property.
        /// </summary>
        private static void OnIsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LayoutItem)d).OnIsActiveChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the IsActive property.
        /// </summary>
        protected virtual void OnIsActiveChanged(DependencyPropertyChangedEventArgs e)
        {
            if (_isActiveReentrantFlag.CanEnter)
            {
                using (_isActiveReentrantFlag.Enter())
                {
                    LayoutElement.IsActive = (bool)e.NewValue;
                }
            }
        }

        #endregion

        #region CanClose

        /// <summary>
        /// CanClose Dependency Property
        /// </summary>
        public static readonly DependencyProperty CanCloseProperty =
            DependencyProperty.Register("CanClose", typeof(bool), typeof(LayoutItem),
                new FrameworkPropertyMetadata((bool)true,
                    new PropertyChangedCallback(OnCanCloseChanged)));

        /// <summary>
        /// Gets or sets the CanClose property.  This dependency property 
        /// indicates if the item can be closed.
        /// </summary>
        public bool CanClose
        {
            get { return (bool)GetValue(CanCloseProperty); }
            set { SetValue(CanCloseProperty, value); }
        }

        /// <summary>
        /// Handles changes to the CanClose property.
        /// </summary>
        private static void OnCanCloseChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LayoutItem)d).OnCanCloseChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the CanClose property.
        /// </summary>
        protected virtual void OnCanCloseChanged(DependencyPropertyChangedEventArgs e)
        {
            LayoutElement.CanClose = (bool)e.NewValue;
        }

        #endregion

        #region CanFloat

        /// <summary>
        /// CanFloat Dependency Property
        /// </summary>
        public static readonly DependencyProperty CanFloatProperty =
            DependencyProperty.Register("CanFloat", typeof(bool), typeof(LayoutItem),
                new FrameworkPropertyMetadata((bool)true,
                    new PropertyChangedCallback(OnCanFloatChanged)));

        /// <summary>
        /// Gets or sets the CanFloat property.  This dependency property 
        /// indicates if user can move the layout element dragging it to another position.
        /// </summary>
        public bool CanFloat
        {
            get { return (bool)GetValue(CanFloatProperty); }
            set { SetValue(CanFloatProperty, value); }
        }

        /// <summary>
        /// Handles changes to the CanFloat property.
        /// </summary>
        private static void OnCanFloatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LayoutItem)d).OnCanFloatChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the CanFloat property.
        /// </summary>
        protected virtual void OnCanFloatChanged(DependencyPropertyChangedEventArgs e)
        {
            LayoutElement.CanFloat = (bool)e.NewValue;
        }

        #endregion

        #region CloseCommand

        /// <summary>
        /// CloseCommand Dependency Property
        /// </summary>
        public static readonly DependencyProperty CloseCommandProperty =
            DependencyProperty.Register("CloseCommand", typeof(ICommand), typeof(LayoutItem),
                new FrameworkPropertyMetadata(null,
                    new PropertyChangedCallback(OnCloseCommandChanged),
                    new CoerceValueCallback(CoerceCloseCommandValue)));

        /// <summary>
        /// Gets or sets the CloseCommand property.  This dependency property 
        /// indicates the command to execute when user click the document close button.
        /// </summary>
        public ICommand CloseCommand
        {
            get { return (ICommand)GetValue(CloseCommandProperty); }
            set { SetValue(CloseCommandProperty, value); }
        }

        /// <summary>
        /// Handles changes to the CloseCommand property.
        /// </summary>
        private static void OnCloseCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LayoutItem)d).OnCloseCommandChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the CloseCommand property.
        /// </summary>
        protected virtual void OnCloseCommandChanged(DependencyPropertyChangedEventArgs e)
        {
        }

        /// <summary>
        /// Coerces the CloseCommand value.
        /// </summary>
        private static object CoerceCloseCommandValue(DependencyObject d, object value)
        {
            return value;
        }

        private bool CanExecuteCloseCommand(object parameter)
        {
            return LayoutElement.CanClose;
        }

        private void ExecuteCloseCommand(object parameter)
        {
            Close();
        }

        protected abstract void Close();


        #endregion

        #region FloatCommand
        /// <summary>
        /// FloatCommand Dependency Property
        /// </summary>
        public static readonly DependencyProperty FloatCommandProperty =
            DependencyProperty.Register("FloatCommand", typeof(ICommand), typeof(LayoutItem),
                new FrameworkPropertyMetadata(null,
                    new PropertyChangedCallback(OnFloatCommandChanged),
                    new CoerceValueCallback(CoerceFloatCommandValue)));

        /// <summary>
        /// Gets or sets the FloatCommand property.  This dependency property 
        /// indicates the command to execute when user click the float button.
        /// </summary>
        /// <remarks>By default this command move the anchorable inside new floating window.</remarks>
        public ICommand FloatCommand
        {
            get { return (ICommand)GetValue(FloatCommandProperty); }
            set { SetValue(FloatCommandProperty, value); }
        }

        /// <summary>
        /// Handles changes to the FloatCommand property.
        /// </summary>
        private static void OnFloatCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LayoutItem)d).OnFloatCommandChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the FloatCommand property.
        /// </summary>
        protected virtual void OnFloatCommandChanged(DependencyPropertyChangedEventArgs e)
        {
        }

        /// <summary>
        /// Coerces the FloatCommand value.
        /// </summary>
        private static object CoerceFloatCommandValue(DependencyObject d, object value)
        {
            return value;
        }

        private bool CanExecuteFloatCommand(object anchorable)
        {
            return LayoutElement.CanFloat && LayoutElement.FindParent<LayoutFloatingWindow>() == null;
        }

        private void ExecuteFloatCommand(object parameter)
        {
            LayoutElement.Root.Manager._ExecuteFloatCommand(LayoutElement);
        }

        protected virtual void Float()
        { 
            
        }

        #endregion

        #region DockAsDocumentCommand

        /// <summary>
        /// DockAsDocumentCommand Dependency Property
        /// </summary>
        public static readonly DependencyProperty DockAsDocumentCommandProperty =
            DependencyProperty.Register("DockAsDocumentCommand", typeof(ICommand), typeof(LayoutItem),
                new FrameworkPropertyMetadata(null,
                    new PropertyChangedCallback(OnDockAsDocumentCommandChanged),
                    new CoerceValueCallback(CoerceDockAsDocumentCommandValue)));

        /// <summary>
        /// Gets or sets the DockAsDocumentCommand property.  This dependency property 
        /// indicates the command to execute when user click the DockAsDocument button.
        /// </summary>
        /// <remarks>By default this command move the anchorable inside the last focused document pane.</remarks>
        public ICommand DockAsDocumentCommand
        {
            get { return (ICommand)GetValue(DockAsDocumentCommandProperty); }
            set { SetValue(DockAsDocumentCommandProperty, value); }
        }

        /// <summary>
        /// Handles changes to the DockAsDocumentCommand property.
        /// </summary>
        private static void OnDockAsDocumentCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LayoutItem)d).OnDockAsDocumentCommandChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the DockAsDocumentCommand property.
        /// </summary>
        protected virtual void OnDockAsDocumentCommandChanged(DependencyPropertyChangedEventArgs e)
        {
        }

        /// <summary>
        /// Coerces the DockAsDocumentCommand value.
        /// </summary>
        private static object CoerceDockAsDocumentCommandValue(DependencyObject d, object value)
        {
            return value;
        }

        private bool CanExecuteDockAsDocumentCommand(object parameter)
        {
            return LayoutElement.FindParent<LayoutDocumentPane>() == null;
        }

        private void ExecuteDockAsDocumentCommand(object parameter)
        {
            LayoutElement.Root.Manager._ExecuteDockAsDocumentCommand(LayoutElement);
        }

        #endregion

        #region CloseAllButThisCommand

        /// <summary>
        /// CloseAllButThisCommand Dependency Property
        /// </summary>
        public static readonly DependencyProperty CloseAllButThisCommandProperty =
            DependencyProperty.Register("CloseAllButThisCommand", typeof(ICommand), typeof(LayoutItem),
                new FrameworkPropertyMetadata(null,
                    new PropertyChangedCallback(OnCloseAllButThisCommandChanged),
                    new CoerceValueCallback(CoerceCloseAllButThisCommandValue)));

        /// <summary>
        /// Gets or sets the CloseAllButThisCommand property.  This dependency property 
        /// indicates the 'Close All But This' command.
        /// </summary>
        public ICommand CloseAllButThisCommand
        {
            get { return (ICommand)GetValue(CloseAllButThisCommandProperty); }
            set { SetValue(CloseAllButThisCommandProperty, value); }
        }

        /// <summary>
        /// Handles changes to the CloseAllButThisCommand property.
        /// </summary>
        private static void OnCloseAllButThisCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LayoutItem)d).OnCloseAllButThisCommandChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the CloseAllButThisCommand property.
        /// </summary>
        protected virtual void OnCloseAllButThisCommandChanged(DependencyPropertyChangedEventArgs e)
        {
        }

        /// <summary>
        /// Coerces the CloseAllButThisCommand value.
        /// </summary>
        private static object CoerceCloseAllButThisCommandValue(DependencyObject d, object value)
        {
            return value;
        }

        private bool CanExecuteCloseAllButThisCommand(object parameter)
        {
            var root = LayoutElement.Root;
            if (root == null)
                return false;

            return LayoutElement.Root.Manager.Layout.
                Descendents().OfType<LayoutContent>().Where(d => d != LayoutElement && (d.Parent is LayoutDocumentPane || d.Parent is LayoutDocumentFloatingWindow)).Any();
        }

        private void ExecuteCloseAllButThisCommand(object parameter)
        {
            LayoutElement.Root.Manager._ExecuteCloseAllButThisCommand(LayoutElement);
        }

        #endregion

        #region ActivateCommand

        /// <summary>
        /// ActivateCommand Dependency Property
        /// </summary>
        public static readonly DependencyProperty ActivateCommandProperty =
            DependencyProperty.Register("ActivateCommand", typeof(ICommand), typeof(LayoutItem),
                new FrameworkPropertyMetadata(null,
                    new PropertyChangedCallback(OnActivateCommandChanged),
                    new CoerceValueCallback(CoerceActivateCommandValue)));

        /// <summary>
        /// Gets or sets the ActivateCommand property.  This dependency property 
        /// indicates the command to execute when user wants to activate a content (either a Document or an Anchorable).
        /// </summary>
        public ICommand ActivateCommand
        {
            get { return (ICommand)GetValue(ActivateCommandProperty); }
            set { SetValue(ActivateCommandProperty, value); }
        }

        /// <summary>
        /// Handles changes to the ActivateCommand property.
        /// </summary>
        private static void OnActivateCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LayoutItem)d).OnActivateCommandChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the ActivateCommand property.
        /// </summary>
        protected virtual void OnActivateCommandChanged(DependencyPropertyChangedEventArgs e)
        {
        }

        /// <summary>
        /// Coerces the ActivateCommand value.
        /// </summary>
        private static object CoerceActivateCommandValue(DependencyObject d, object value)
        {
            return value;
        }

        private bool CanExecuteActivateCommand(object parameter)
        {
            return true;
        }

        private void ExecuteActivateCommand(object parameter)
        {
            LayoutElement.Root.Manager._ExecuteContentActivateCommand(LayoutElement);
        }

        #endregion

        #region NewVerticalTabGroupCommand

        /// <summary>
        /// NewVerticalTabGroupCommand Dependency Property
        /// </summary>
        public static readonly DependencyProperty NewVerticalTabGroupCommandProperty =
            DependencyProperty.Register("NewVerticalTabGroupCommand", typeof(ICommand), typeof(LayoutItem),
                new FrameworkPropertyMetadata((ICommand)null,
                    new PropertyChangedCallback(OnNewVerticalTabGroupCommandChanged)));

        /// <summary>
        /// Gets or sets the NewVerticalTabGroupCommand property.  This dependency property 
        /// indicates the new vertical tab group command.
        /// </summary>
        public ICommand NewVerticalTabGroupCommand
        {
            get { return (ICommand)GetValue(NewVerticalTabGroupCommandProperty); }
            set { SetValue(NewVerticalTabGroupCommandProperty, value); }
        }

        /// <summary>
        /// Handles changes to the NewVerticalTabGroupCommand property.
        /// </summary>
        private static void OnNewVerticalTabGroupCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LayoutItem)d).OnNewVerticalTabGroupCommandChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the NewVerticalTabGroupCommand property.
        /// </summary>
        protected virtual void OnNewVerticalTabGroupCommandChanged(DependencyPropertyChangedEventArgs e)
        {
        }

        private bool CanExecuteNewVerticalTabGroupCommand(object parameter)
        {
            var parentDocumentGroup = LayoutElement.FindParent<LayoutDocumentPaneGroup>();
            var parentDocumentPane = LayoutElement.Parent as LayoutDocumentPane;
            return ((parentDocumentGroup == null ||
                parentDocumentGroup.ChildrenCount == 1 ||
                parentDocumentGroup.Root.Manager.AllowMixedOrientation || 
                parentDocumentGroup.Orientation == System.Windows.Controls.Orientation.Horizontal) &&
                parentDocumentPane != null &&
                parentDocumentPane.ChildrenCount > 1);
        }

        private void ExecuteNewVerticalTabGroupCommand(object parameter)
        {
            var parentDocumentGroup = LayoutElement.FindParent<LayoutDocumentPaneGroup>();
            var parentDocumentPane = LayoutElement.Parent as LayoutDocumentPane;

            if (parentDocumentGroup == null)
            {
                var grandParent = parentDocumentPane.Parent as ILayoutContainer;
                parentDocumentGroup = new LayoutDocumentPaneGroup() { Orientation = System.Windows.Controls.Orientation.Horizontal };
                grandParent.ReplaceChild(parentDocumentPane, parentDocumentGroup);
                parentDocumentGroup.Children.Add(parentDocumentPane);
            }
            parentDocumentGroup.Orientation = System.Windows.Controls.Orientation.Horizontal; 
            int indexOfParentPane = parentDocumentGroup.IndexOfChild(parentDocumentPane);
            parentDocumentGroup.InsertChildAt(indexOfParentPane + 1, new LayoutDocumentPane(LayoutElement));
            LayoutElement.IsActive = true;
            LayoutElement.Root.CollectGarbage();
        }
        #endregion

        #region NewHorizontalTabGroupCommand

        /// <summary>
        /// NewHorizontalTabGroupCommand Dependency Property
        /// </summary>
        public static readonly DependencyProperty NewHorizontalTabGroupCommandProperty =
            DependencyProperty.Register("NewHorizontalTabGroupCommand", typeof(ICommand), typeof(LayoutItem),
                new FrameworkPropertyMetadata((ICommand)null,
                    new PropertyChangedCallback(OnNewHorizontalTabGroupCommandChanged)));

        /// <summary>
        /// Gets or sets the NewHorizontalTabGroupCommand property.  This dependency property 
        /// indicates the new horizontal tab group command.
        /// </summary>
        public ICommand NewHorizontalTabGroupCommand
        {
            get { return (ICommand)GetValue(NewHorizontalTabGroupCommandProperty); }
            set { SetValue(NewHorizontalTabGroupCommandProperty, value); }
        }

        /// <summary>
        /// Handles changes to the NewHorizontalTabGroupCommand property.
        /// </summary>
        private static void OnNewHorizontalTabGroupCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LayoutItem)d).OnNewHorizontalTabGroupCommandChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the NewHorizontalTabGroupCommand property.
        /// </summary>
        protected virtual void OnNewHorizontalTabGroupCommandChanged(DependencyPropertyChangedEventArgs e)
        {
        }


        private bool CanExecuteNewHorizontalTabGroupCommand(object parameter)
        {
            var parentDocumentGroup = LayoutElement.FindParent<LayoutDocumentPaneGroup>();
            var parentDocumentPane = LayoutElement.Parent as LayoutDocumentPane;
            return ((parentDocumentGroup == null ||
                parentDocumentGroup.ChildrenCount == 1 ||
                parentDocumentGroup.Root.Manager.AllowMixedOrientation ||
                parentDocumentGroup.Orientation == System.Windows.Controls.Orientation.Vertical) &&
                parentDocumentPane != null &&
                parentDocumentPane.ChildrenCount > 1);
        }

        private void ExecuteNewHorizontalTabGroupCommand(object parameter)
        {
            var parentDocumentGroup = LayoutElement.FindParent<LayoutDocumentPaneGroup>();
            var parentDocumentPane = LayoutElement.Parent as LayoutDocumentPane;

            if (parentDocumentGroup == null)
            {
                var grandParent = parentDocumentPane.Parent as ILayoutContainer;
                parentDocumentGroup = new LayoutDocumentPaneGroup() { Orientation = System.Windows.Controls.Orientation.Vertical };
                grandParent.ReplaceChild(parentDocumentPane, parentDocumentGroup);
                parentDocumentGroup.Children.Add(parentDocumentPane);
            }
            parentDocumentGroup.Orientation = System.Windows.Controls.Orientation.Vertical;
            int indexOfParentPane = parentDocumentGroup.IndexOfChild(parentDocumentPane);
            parentDocumentGroup.InsertChildAt(indexOfParentPane + 1, new LayoutDocumentPane(LayoutElement));
            LayoutElement.IsActive = true;
            LayoutElement.Root.CollectGarbage();
        }       
        #endregion

        #region MoveToNextTabGroupCommand

        /// <summary>
        /// MoveToNextTabGroupCommand Dependency Property
        /// </summary>
        public static readonly DependencyProperty MoveToNextTabGroupCommandProperty =
            DependencyProperty.Register("MoveToNextTabGroupCommand", typeof(ICommand), typeof(LayoutItem),
                new FrameworkPropertyMetadata((ICommand)null,
                    new PropertyChangedCallback(OnMoveToNextTabGroupCommandChanged)));

        /// <summary>
        /// Gets or sets the MoveToNextTabGroupCommand property.  This dependency property 
        /// indicates move to next tab group command.
        /// </summary>
        public ICommand MoveToNextTabGroupCommand
        {
            get { return (ICommand)GetValue(MoveToNextTabGroupCommandProperty); }
            set { SetValue(MoveToNextTabGroupCommandProperty, value); }
        }

        /// <summary>
        /// Handles changes to the MoveToNextTabGroupCommand property.
        /// </summary>
        private static void OnMoveToNextTabGroupCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LayoutItem)d).OnMoveToNextTabGroupCommandChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the MoveToNextTabGroupCommand property.
        /// </summary>
        protected virtual void OnMoveToNextTabGroupCommandChanged(DependencyPropertyChangedEventArgs e)
        {
        }

        private bool CanExecuteMoveToNextTabGroupCommand(object parameter)
        {
            var parentDocumentGroup = LayoutElement.FindParent<LayoutDocumentPaneGroup>();
            var parentDocumentPane = LayoutElement.Parent as LayoutDocumentPane;
            return (parentDocumentGroup != null &&
                parentDocumentPane != null &&
                parentDocumentGroup.ChildrenCount > 1 &&
                parentDocumentGroup.IndexOfChild(parentDocumentPane) < parentDocumentGroup.ChildrenCount - 1 &&
                parentDocumentGroup.Children[parentDocumentGroup.IndexOfChild(parentDocumentPane) + 1] is LayoutDocumentPane);
        }

        private void ExecuteMoveToNextTabGroupCommand(object parameter)
        {
            var parentDocumentGroup = LayoutElement.FindParent<LayoutDocumentPaneGroup>();
            var parentDocumentPane = LayoutElement.Parent as LayoutDocumentPane;
            int indexOfParentPane = parentDocumentGroup.IndexOfChild(parentDocumentPane);
            var nextDocumentPane = parentDocumentGroup.Children[indexOfParentPane + 1] as LayoutDocumentPane;
            nextDocumentPane.InsertChildAt(0, LayoutElement);
            LayoutElement.IsActive = true;
            LayoutElement.Root.CollectGarbage();
        }

        #endregion

        #region MoveToPreviousTabGroupCommand

        /// <summary>
        /// MoveToPreviousTabGroupCommand Dependency Property
        /// </summary>
        public static readonly DependencyProperty MoveToPreviousTabGroupCommandProperty =
            DependencyProperty.Register("MoveToPreviousTabGroupCommand", typeof(ICommand), typeof(LayoutItem),
                new FrameworkPropertyMetadata((ICommand)null,
                    new PropertyChangedCallback(OnMoveToPreviousTabGroupCommandChanged)));

        /// <summary>
        /// Gets or sets the MoveToPreviousTabGroupCommand property.  This dependency property 
        /// indicates move to rpevious tab group command.
        /// </summary>
        public ICommand MoveToPreviousTabGroupCommand
        {
            get { return (ICommand)GetValue(MoveToPreviousTabGroupCommandProperty); }
            set { SetValue(MoveToPreviousTabGroupCommandProperty, value); }
        }

        /// <summary>
        /// Handles changes to the MoveToPreviousTabGroupCommand property.
        /// </summary>
        private static void OnMoveToPreviousTabGroupCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LayoutItem)d).OnMoveToPreviousTabGroupCommandChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the MoveToPreviousTabGroupCommand property.
        /// </summary>
        protected virtual void OnMoveToPreviousTabGroupCommandChanged(DependencyPropertyChangedEventArgs e)
        {
        }

        private bool CanExecuteMoveToPreviousTabGroupCommand(object parameter)
        {
            var parentDocumentGroup = LayoutElement.FindParent<LayoutDocumentPaneGroup>();
            var parentDocumentPane = LayoutElement.Parent as LayoutDocumentPane;
            return (parentDocumentGroup != null &&
                parentDocumentPane != null &&
                parentDocumentGroup.ChildrenCount > 1 &&
                parentDocumentGroup.IndexOfChild(parentDocumentPane) > 0 &&
                parentDocumentGroup.Children[parentDocumentGroup.IndexOfChild(parentDocumentPane) - 1] is LayoutDocumentPane);
        }

        private void ExecuteMoveToPreviousTabGroupCommand(object parameter)
        {
            var parentDocumentGroup = LayoutElement.FindParent<LayoutDocumentPaneGroup>();
            var parentDocumentPane = LayoutElement.Parent as LayoutDocumentPane;
            int indexOfParentPane = parentDocumentGroup.IndexOfChild(parentDocumentPane);
            var nextDocumentPane = parentDocumentGroup.Children[indexOfParentPane - 1] as LayoutDocumentPane;
            nextDocumentPane.InsertChildAt(0, LayoutElement);
            LayoutElement.IsActive = true;
            LayoutElement.Root.CollectGarbage();
        }
        #endregion
       
    }
}
