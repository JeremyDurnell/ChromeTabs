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
using System.Collections.ObjectModel;
using System.Windows.Markup;
using System.Xml.Serialization;
using Standard;

namespace AvalonDock.Layout
{
    [ContentProperty("RootPanel")]
    [Serializable]
    public class LayoutRoot : LayoutElement, ILayoutContainer, ILayoutRoot
    {
        public LayoutRoot()
        { 
            RightSide = new LayoutAnchorSide();
            LeftSide = new LayoutAnchorSide();
            TopSide = new LayoutAnchorSide();
            BottomSide = new LayoutAnchorSide();
            RootPanel = new LayoutPanel(new LayoutDocumentPane());
        }


        #region RootPanel

        private LayoutPanel _rootPanel;
        public LayoutPanel RootPanel
        {
            get { return _rootPanel; }
            set
            {
                if (_rootPanel != value)
                {
                    RaisePropertyChanging("RootPanel");
                    if (_rootPanel != null &&
                        _rootPanel.Parent == this)
                        _rootPanel.Parent = null;
                    _rootPanel = value;

                    if (_rootPanel == null)
                        _rootPanel = new LayoutPanel(new LayoutDocumentPane());

                    if (_rootPanel != null)
                        _rootPanel.Parent = this;
                    RaisePropertyChanged("RootPanel");
                }
            }
        }

        #endregion

        #region TopSide

        private LayoutAnchorSide _topSide = null;
        public LayoutAnchorSide TopSide
        {
            get { return _topSide; }
            set
            {
                if (_topSide != value)
                {
                    RaisePropertyChanging("TopSide");
                    _topSide = value;
                    if (_topSide != null)
                        _topSide.Parent = this;
                    RaisePropertyChanged("TopSide");
                }
            }
        }

        #endregion

        #region RightSide

        private LayoutAnchorSide _rightSide;
        public LayoutAnchorSide RightSide
        {
            get { return _rightSide; }
            set
            {
                if (_rightSide != value)
                {
                    RaisePropertyChanging("RightSide");
                    _rightSide = value;
                    if (_rightSide != null)
                        _rightSide.Parent = this;
                    RaisePropertyChanged("RightSide");
                }
            }
        }

        #endregion

        #region LeftSide

        private LayoutAnchorSide _leftSide = null;
        public LayoutAnchorSide LeftSide
        {
            get { return _leftSide; }
            set
            {
                if (_leftSide != value)
                {
                    RaisePropertyChanging("LeftSide");
                    _leftSide = value;
                    if (_leftSide != null)
                        _leftSide.Parent = this;
                    RaisePropertyChanged("LeftSide");
                }
            }
        }

        #endregion

        #region BottomSide

        private LayoutAnchorSide _bottomSide = null;
        public LayoutAnchorSide BottomSide
        {
            get { return _bottomSide; }
            set
            {
                if (_bottomSide != value)
                {
                    RaisePropertyChanging("BottomSide");
                    _bottomSide = value;
                    if (_bottomSide != null)
                        _bottomSide.Parent = this;
                    RaisePropertyChanged("BottomSide");
                }
            }
        }

        #endregion

        #region FloatingWindows
        ObservableCollection<LayoutFloatingWindow> _floatingWindows = null;

        public ObservableCollection<LayoutFloatingWindow> FloatingWindows
        {
            get 
            {
                if (_floatingWindows == null)
                {
                    _floatingWindows = new ObservableCollection<LayoutFloatingWindow>();
                    _floatingWindows.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(_floatingWindows_CollectionChanged);
                }

                return _floatingWindows; 
            }
        }

        void _floatingWindows_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null && (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove ||
                e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace))
            {
                foreach (LayoutFloatingWindow element in e.OldItems)
                {
                    if (element.Parent == this)
                        element.Parent = null;
                }
            } 

            if (e.NewItems != null && (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add ||
                e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace))
            {
                foreach (LayoutFloatingWindow element in e.NewItems)
                    element.Parent = this;
            }
        }
        #endregion

        #region HiddenAnchorables

        ObservableCollection<LayoutAnchorable> _hiddenAnchorables = null;

        public ObservableCollection<LayoutAnchorable> Hidden
        {
            get
            {
                if (_hiddenAnchorables == null)
                {
                    _hiddenAnchorables = new ObservableCollection<LayoutAnchorable>();
                    _hiddenAnchorables.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(_hiddenAnchorables_CollectionChanged);
                }

                return _hiddenAnchorables;
            }
        }

        void _hiddenAnchorables_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove ||
                e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
            {
                if (e.OldItems != null)
                {
                    foreach (LayoutAnchorable element in e.OldItems)
                    {
                        if (element.Parent == this)
                            element.Parent = null;
                    }
                }
            }

            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add ||
                e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
            {
                if (e.NewItems != null)
                {
                    foreach (LayoutAnchorable element in e.NewItems)
                    {
                        if (element.Parent != this)
                        {
                            if (element.Parent != null)
                                element.Parent.RemoveChild(element);
                            element.Parent = this;
                        }

                    }
                }
            }



        }

        #endregion

        #region Children
        public IEnumerable<ILayoutElement> Children
        {
            get
            {
                if (RootPanel != null)
                    yield return RootPanel;
                if (_floatingWindows != null)
                {
                    foreach (var floatingWindow in _floatingWindows)
                        yield return floatingWindow;
                }
                if (TopSide != null)
                    yield return TopSide;
                if (RightSide != null)
                    yield return RightSide;
                if (BottomSide != null)
                    yield return BottomSide;
                if (LeftSide != null)
                    yield return LeftSide;
                if (_hiddenAnchorables != null)
                {
                    foreach (var hiddenAnchorable in _hiddenAnchorables)
                        yield return hiddenAnchorable;
                }
            }
        }
        public void RemoveChild(ILayoutElement element)
        {
            if (element == RootPanel)
                RootPanel = null;
            else if (_floatingWindows != null && _floatingWindows.Contains(element))
                _floatingWindows.Remove(element as LayoutFloatingWindow);
            else if (_hiddenAnchorables != null && _hiddenAnchorables.Contains(element))
                _hiddenAnchorables.Remove(element as LayoutAnchorable);
            else if (element == TopSide)
                TopSide = null;
            else if (element == RightSide)
                RightSide = null;
            else if (element == BottomSide)
                BottomSide = null;
            else if (element == LeftSide)
                LeftSide = null;

        }

        public void ReplaceChild(ILayoutElement oldElement, ILayoutElement newElement)
        {
            if (oldElement == RootPanel)
                RootPanel = (LayoutPanel)newElement;
            else if (_floatingWindows != null && _floatingWindows.Contains(oldElement))
            {
                int index = _floatingWindows.IndexOf(oldElement as LayoutFloatingWindow);
                _floatingWindows.Remove(oldElement as LayoutFloatingWindow);
                _floatingWindows.Insert(index, newElement as LayoutFloatingWindow);
            }
            else if (_hiddenAnchorables != null && _hiddenAnchorables.Contains(oldElement))
            {
                int index = _hiddenAnchorables.IndexOf(oldElement as LayoutAnchorable);
                _hiddenAnchorables.Remove(oldElement as LayoutAnchorable);
                _hiddenAnchorables.Insert(index, newElement as LayoutAnchorable);
            }
            else if (oldElement == TopSide)
                TopSide = (LayoutAnchorSide)newElement;
            else if (oldElement == RightSide)
                RightSide = (LayoutAnchorSide)newElement;
            else if (oldElement == BottomSide)
                BottomSide = (LayoutAnchorSide)newElement;
            else if (oldElement == LeftSide)
                LeftSide = (LayoutAnchorSide)newElement;
        }

        public int ChildrenCount
        {
            get
            {
                return 5 +
                    (_floatingWindows != null ? _floatingWindows.Count : 0) + 
                    (_hiddenAnchorables != null ? _hiddenAnchorables.Count : 0); 
            }
        }
        #endregion

        #region ActiveContent

        [field:NonSerialized]
        private WeakReference _activeContent = null;
        
        [XmlIgnore]
        public LayoutContent ActiveContent
        {
            get { return _activeContent.GetValueOrDefault<LayoutContent>(); }
            set
            {
                var currentValue = ActiveContent;
                if (currentValue != value)
                {
                    RaisePropertyChanging("ActiveContent");
                    if (currentValue != null)
                        currentValue.IsActive = false;
                    _activeContent = new WeakReference(value);
                    currentValue = ActiveContent;
                    if (currentValue != null)
                        currentValue.IsActive = true;
                    RaisePropertyChanged("ActiveContent");

                    if (currentValue != null)
                    {
                        if (currentValue.Parent is LayoutDocumentPane || currentValue is LayoutDocument)
                            LastFocusedDocument = currentValue;
                    }
                    else
                        LastFocusedDocument = null;
                }
            }
        }

        #endregion

        #region LastFocusedDocument

        [field: NonSerialized]
        private WeakReference _lastFocusedDocument = null;
        
        [XmlIgnore]
        public LayoutContent LastFocusedDocument
        {
            get { return _lastFocusedDocument.GetValueOrDefault<LayoutContent>(); }
            private set
            {
                var currentValue = LastFocusedDocument;
                if (currentValue != value)
                {
                    RaisePropertyChanging("LastFocusedDocument");
                    if (currentValue != null)
                        currentValue.IsLastFocusedDocument = false;
                    _lastFocusedDocument = new WeakReference(value);
                    currentValue = LastFocusedDocument;
                    if (currentValue != null)
                        currentValue.IsLastFocusedDocument = true;
                    RaisePropertyChanged("LastFocusedDocument");
                }
            }
        }

        #endregion

        #region Manager

        
        [NonSerialized]
        private DockingManager _manager = null;

        [XmlIgnore]
        public DockingManager Manager
        {
            get { return _manager; }
            internal set
            {
                if (_manager != value)
                {
                    RaisePropertyChanging("Manager");
                    _manager = value;
                    RaisePropertyChanged("Manager");
                }
            }
        }

        #endregion

        #region CollectGarbage

        /// <summary>
        /// Removes any empty container not directly referenced by other layout items
        /// </summary>
        public void CollectGarbage()
        {
            bool exitFlag = true;

            #region collect empty panes
            do
            {
                exitFlag = true;
                //for each pane that is empty
                foreach (var emptyPane in this.Descendents().OfType<ILayoutPane>().Where(p => p.ChildrenCount == 0))
                {
                    //...set null any reference coming from contents not yet hosted in a floating window
                    foreach (var contentReferencingEmptyPane in this.Descendents().OfType<LayoutContent>()
                        .Where(c => ((ILayoutPreviousContainer)c).PreviousContainer == emptyPane && !c.IsFloating))
                    {
                        if (contentReferencingEmptyPane is LayoutAnchorable &&
                            !((LayoutAnchorable)contentReferencingEmptyPane).IsVisible)
                            continue;

                        ((ILayoutPreviousContainer)contentReferencingEmptyPane).PreviousContainer = null;
                        contentReferencingEmptyPane.PreviousContainerIndex = -1;
                    }

                    //...if this pane is the only documentpane present in the layout than skip it
                    if (emptyPane is LayoutDocumentPane &&
                        this.Descendents().OfType<LayoutDocumentPane>().Count(c => c != emptyPane) == 0)
                        continue;

                    //...if this empty panes is not referenced by anyone, than removes it from its parent container
                    if (!this.Descendents().OfType<ILayoutPreviousContainer>().Any(c => c.PreviousContainer == emptyPane))
                    {
                        var parentGroup = emptyPane.Parent as ILayoutContainer;
                        parentGroup.RemoveChild(emptyPane);
                        exitFlag = false;
                        break;
                    }
                }

                if (!exitFlag)
                {
                    //removes any empty anchorable pane group
                    foreach (var emptyPaneGroup in this.Descendents().OfType<LayoutAnchorablePaneGroup>().Where(p => p.ChildrenCount == 0))
                    {
                        var parentGroup = emptyPaneGroup.Parent as ILayoutContainer;
                        parentGroup.RemoveChild(emptyPaneGroup);
                        exitFlag = false;
                        break;
                    }
                }

                if (!exitFlag)
                {
                    //removes any empty layout panel
                    foreach (var emptyPaneGroup in this.Descendents().OfType<LayoutPanel>().Where(p => p.ChildrenCount == 0))
                    {
                        var parentGroup = emptyPaneGroup.Parent as ILayoutContainer;
                        parentGroup.RemoveChild(emptyPaneGroup);
                        exitFlag = false;
                        break;
                    }
                }

                if (!exitFlag)
                {
                    //removes any empty floating window
                    foreach (var emptyPaneGroup in this.Descendents().OfType<LayoutFloatingWindow>().Where(p => p.ChildrenCount == 0))
                    {
                        var parentGroup = emptyPaneGroup.Parent as ILayoutContainer;
                        parentGroup.RemoveChild(emptyPaneGroup);
                        exitFlag = false;
                        break;
                    }
                }

                if (!exitFlag)
                {
                    //removes any empty anchor group
                    foreach (var emptyPaneGroup in this.Descendents().OfType<LayoutAnchorGroup>().Where(p => p.ChildrenCount == 0))
                    {
                        var parentGroup = emptyPaneGroup.Parent as ILayoutContainer;
                        parentGroup.RemoveChild(emptyPaneGroup);
                        exitFlag = false;
                        break;
                    }
                }

            }
            while (!exitFlag);
            #endregion

            #region collapse single child anchorable pane groups
            do
            {
                exitFlag = true;
                //for each pane that is empty
                foreach (var paneGroupToCollapse in this.Descendents().OfType<LayoutAnchorablePaneGroup>().Where(p => p.ChildrenCount == 1 && p.Children[0] is LayoutAnchorablePaneGroup).ToArray())
                {
                    var singleChild = paneGroupToCollapse.Children[0] as LayoutAnchorablePaneGroup;
                    paneGroupToCollapse.Orientation = singleChild.Orientation;
                    paneGroupToCollapse.RemoveChild(singleChild);
                    while (singleChild.ChildrenCount > 0)
                    {
                        paneGroupToCollapse.InsertChildAt(
                            paneGroupToCollapse.ChildrenCount, singleChild.Children[0]);
                    }

                    exitFlag = false;
                    break;
                }

            }
            while (!exitFlag);



            #endregion

            #region collapse single child document pane groups
            do
            {
                exitFlag = true;
                //for each pane that is empty
                foreach (var paneGroupToCollapse in this.Descendents().OfType<LayoutDocumentPaneGroup>().Where(p => p.ChildrenCount == 1 && p.Children[0] is LayoutDocumentPaneGroup).ToArray())
                {
                    var singleChild = paneGroupToCollapse.Children[0] as LayoutDocumentPaneGroup;
                    paneGroupToCollapse.Orientation = singleChild.Orientation;
                    paneGroupToCollapse.RemoveChild(singleChild);
                    while (singleChild.ChildrenCount > 0)
                    {
                        paneGroupToCollapse.InsertChildAt(
                            paneGroupToCollapse.ChildrenCount, singleChild.Children[0]);
                    }

                    exitFlag = false;
                    break;
                }

            }
            while (!exitFlag);



            #endregion


#if DEBUG
            System.Diagnostics.Debug.Assert(
                !this.Descendents().OfType<LayoutAnchorablePane>().Any(a => a.ChildrenCount == 0 && a.IsVisible));
#endif
        }

        #endregion

        public event EventHandler Updated;

        internal void FireLayoutUpdated()
        {
            if (Updated != null)
                Updated(this, EventArgs.Empty);
        }

        #region LayoutElement Added/Removed events

        internal void OnLayoutElementAdded(LayoutElement element)
        {
            if (ElementAdded != null)
                ElementAdded(this, new LayoutElementEventArgs(element));
        }

        public event EventHandler<LayoutElementEventArgs> ElementAdded;

        internal void OnLayoutElementRemoved(LayoutElement element)
        {
            if (element.Descendents().OfType<LayoutContent>().Any(c => c == LastFocusedDocument))
                LastFocusedDocument = null;
            if (element.Descendents().OfType<LayoutContent>().Any(c => c == ActiveContent))
                ActiveContent = null;
            if (ElementRemoved != null)
                ElementRemoved(this, new LayoutElementEventArgs(element));
        }

        public event EventHandler<LayoutElementEventArgs> ElementRemoved;

        #endregion
    }
}
