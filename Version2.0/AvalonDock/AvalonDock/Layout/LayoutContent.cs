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
using System.Windows.Markup;
using System.Xml.Serialization;
using System.Windows;
using System.Globalization;
using System.Windows.Media;

namespace AvalonDock.Layout
{
    [ContentProperty("Content")]
    [Serializable]
    public abstract class LayoutContent : LayoutElement, IXmlSerializable, ILayoutElementForFloatingWindow, IComparable<LayoutContent>, ILayoutPreviousContainer
    {
        internal LayoutContent()
        { }

        #region Title

        private string _title = null;
        public string Title
        {
            get { return _title; }
            set
            {
                if (_title != value)
                {
                    RaisePropertyChanging("Title");
                    _title = value;
                    RaisePropertyChanged("Title");
                }
            }
        }

        #endregion

        #region Content
        [NonSerialized]
        private object _content = null;
        [XmlIgnore]
        public object Content
        {
            get { return _content; }
            set
            {
                if (_content != value)
                {
                    RaisePropertyChanging("Content");
                    _content = value;
                    RaisePropertyChanged("Content");
                }
            }
        }

        #endregion

        #region ContentId

        private string _contentId = null;
        public string ContentId
        {
            get 
            {
                if (_contentId == null)
                { 
                    var contentAsControl = _content as FrameworkElement;
                    if (contentAsControl != null && !string.IsNullOrWhiteSpace(contentAsControl.Name))
                        return contentAsControl.Name;
                }
                return _contentId; 
            }
            set
            {
                if (_contentId != value)
                {
                    _contentId = value;
                    RaisePropertyChanged("ContentId");
                }
            }
        }

        #endregion

        #region IsSelected

        private bool _isSelected = false;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    bool oldValue = _isSelected;
                    RaisePropertyChanging("IsSelected");
                    _isSelected = value;
                    var parentSelector = (Parent as ILayoutContentSelector);
                    if (parentSelector != null)
                        parentSelector.SelectedContentIndex = _isSelected ? parentSelector.IndexOf(this) : -1;
                    OnIsSelectedChanged(oldValue, value);
                    RaisePropertyChanged("IsSelected");
                }
            }
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the IsSelected property.
        /// </summary>
        protected virtual void OnIsSelectedChanged(bool oldValue, bool newValue)
        {
            if (IsSelectedChanged != null)
                IsSelectedChanged(this, EventArgs.Empty);
        }

        public event EventHandler IsSelectedChanged;

        #endregion

        #region IsActive

        [field: NonSerialized]
        private bool _isActive = false;
        [XmlIgnore]
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                if (_isActive != value)
                {
                    RaisePropertyChanging("IsActive");
                    bool oldValue = _isActive;
                    _isActive = value;

                    var root = Root;
                    if (root != null && _isActive)
                        root.ActiveContent = this;

                    if (_isActive)
                        IsSelected = true;

                    OnIsActiveChanged(oldValue, value);
                    RaisePropertyChanged("IsActive");
                }
            }
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the IsActive property.
        /// </summary>
        protected virtual void OnIsActiveChanged(bool oldValue, bool newValue)
        {
            LastActivationTimeStamp = DateTime.Now;

            if (IsActiveChanged != null)
                IsActiveChanged(this, EventArgs.Empty);
        }

        public event EventHandler IsActiveChanged;

        #endregion

        #region IsLastFocusedDocument

        private bool _isLastFocusedDocument = false;
        public bool IsLastFocusedDocument
        {
            get { return _isLastFocusedDocument; }
            internal set
            {
                if (_isLastFocusedDocument != value)
                {
                    RaisePropertyChanging("IsLastFocusedDocument");
                    _isLastFocusedDocument = value;
                    RaisePropertyChanged("IsLastFocusedDocument");
                }
            }
        }

        #endregion

        #region PreviousContainer

        [field: NonSerialized]
        private ILayoutContainer _previousContainer = null;

        [XmlIgnore]
        ILayoutContainer ILayoutPreviousContainer.PreviousContainer
        {
            get { return _previousContainer; }
            set
            {
                if (_previousContainer != value)
                {
                    _previousContainer = value;
                    RaisePropertyChanged("PreviousContainer");

                    var paneSerializable = _previousContainer as ILayoutPaneSerializable;
                    if (paneSerializable != null &&
                        paneSerializable.Id == null)
                        paneSerializable.Id = Guid.NewGuid().ToString();
                }
            }
        }

        protected ILayoutContainer PreviousContainer
        {
            get { return ((ILayoutPreviousContainer)this).PreviousContainer; }
            set { ((ILayoutPreviousContainer)this).PreviousContainer = value; }
        }

        [XmlIgnore]
        string ILayoutPreviousContainer.PreviousContainerId
        {
            get;
            set;
        }

        protected string PreviousContainerId
        {
            get { return ((ILayoutPreviousContainer)this).PreviousContainerId; }
            set { ((ILayoutPreviousContainer)this).PreviousContainerId = value; }
        }

        #endregion

        #region PreviousContainerIndex
        [field: NonSerialized]
        private int _previousContainerIndex = -1;
        [XmlIgnore]
        public int PreviousContainerIndex
        {
            get { return _previousContainerIndex; }
            set
            {
                if (_previousContainerIndex != value)
                {
                    _previousContainerIndex = value;
                    RaisePropertyChanged("PreviousContainerIndex");
                }
            }
        }

        #endregion

        #region LastActivationTimeStamp

        private DateTime? _lastActivationTimeStamp = null;
        public DateTime? LastActivationTimeStamp
        {
            get { return _lastActivationTimeStamp; }
            set
            {
                if (_lastActivationTimeStamp != value)
                {
                    _lastActivationTimeStamp = value;
                    RaisePropertyChanged("LastActivationTimeStamp");
                }
            }
        }

        #endregion

        protected override void OnParentChanging(ILayoutContainer oldValue, ILayoutContainer newValue)
        {
            var root = Root;

            if (oldValue != null)
                IsSelected = false;

            //if (root != null && _isActive && newValue == null)
            //    root.ActiveContent = null;
            
            base.OnParentChanging(oldValue, newValue);
        }

        protected override void OnParentChanged(ILayoutContainer oldValue, ILayoutContainer newValue)
        {
            if (IsSelected && Parent != null && Parent is ILayoutContentSelector)
            {
                var parentSelector = (Parent as ILayoutContentSelector);
                parentSelector.SelectedContentIndex = parentSelector.IndexOf(this);
            }

            //var root = Root;
            //if (root != null && _isActive)
            //    root.ActiveContent = this;

            base.OnParentChanged(oldValue, newValue);
        }

        /// <summary>
        /// Close the content
        /// </summary>
        public void Close()
        {
            var root = Root;
            var parentAsContainer = Parent as ILayoutContainer;
            parentAsContainer.RemoveChild(this);
            if (root != null)
                root.CollectGarbage();
        }

        /// <summary>
        /// Event fired when the content is closed (i.e. removed definitely from the layout)
        /// </summary>
        public event EventHandler Closed;

        protected virtual void OnClosed()
        {
            if (Closed != null)
                Closed(this, EventArgs.Empty);
        }

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public virtual void ReadXml(System.Xml.XmlReader reader)
        {
            if (reader.MoveToAttribute("Title"))
                Title = reader.Value;
            //if (reader.MoveToAttribute("IconSource"))
            //    IconSource = new Uri(reader.Value, UriKind.RelativeOrAbsolute);

            if (reader.MoveToAttribute("IsSelected"))
                IsSelected = bool.Parse(reader.Value);
            if (reader.MoveToAttribute("ContentId"))
                ContentId = reader.Value;
            if (reader.MoveToAttribute("IsLastFocusedDocument"))
                IsLastFocusedDocument = bool.Parse(reader.Value);
            if (reader.MoveToAttribute("PreviousContainerId"))
                PreviousContainerId = reader.Value;
            if (reader.MoveToAttribute("PreviousContainerIndex"))
                PreviousContainerIndex = int.Parse(reader.Value);

            if (reader.MoveToAttribute("FloatingLeft"))
                FloatingLeft = double.Parse(reader.Value, CultureInfo.InvariantCulture);
            if (reader.MoveToAttribute("FloatingTop"))
                FloatingTop = double.Parse(reader.Value, CultureInfo.InvariantCulture);
            if (reader.MoveToAttribute("FloatingWidth"))
                FloatingWidth = double.Parse(reader.Value, CultureInfo.InvariantCulture);
            if (reader.MoveToAttribute("FloatingHeight"))
                FloatingHeight = double.Parse(reader.Value, CultureInfo.InvariantCulture);
            if (reader.MoveToAttribute("IsMaximized"))
                IsMaximized = bool.Parse(reader.Value);
            if (reader.MoveToAttribute("CanClose"))
                CanClose = bool.Parse(reader.Value);
            if (reader.MoveToAttribute("CanFloat"))
                CanFloat = bool.Parse(reader.Value);
            if (reader.MoveToAttribute("LastActivationTimeStamp"))
                LastActivationTimeStamp = DateTime.Parse(reader.Value, CultureInfo.InvariantCulture);

            reader.Read();
        }

        public virtual void WriteXml(System.Xml.XmlWriter writer)
        {
            if (!string.IsNullOrWhiteSpace(Title))
                writer.WriteAttributeString("Title", Title);

            //if (IconSource != null)
            //    writer.WriteAttributeString("IconSource", IconSource.ToString());
            
            if (IsSelected)
                writer.WriteAttributeString("IsSelected", IsSelected.ToString());

            if (IsLastFocusedDocument)
                writer.WriteAttributeString("IsLastFocusedDocument", IsLastFocusedDocument.ToString());

            if (!string.IsNullOrWhiteSpace(ContentId))
                writer.WriteAttributeString("ContentId", ContentId);
          

            if (ToolTip != null && ToolTip is string)
                if (!string.IsNullOrWhiteSpace((string)ToolTip))
                    writer.WriteAttributeString("ToolTip", (string)ToolTip);

            if (FloatingLeft != 0.0)
                writer.WriteAttributeString("FloatingLeft", FloatingLeft.ToString(CultureInfo.InvariantCulture));
            if (FloatingTop != 0.0)
                writer.WriteAttributeString("FloatingTop", FloatingTop.ToString(CultureInfo.InvariantCulture));
            if (FloatingWidth != 0.0)
                writer.WriteAttributeString("FloatingWidth", FloatingWidth.ToString(CultureInfo.InvariantCulture));
            if (FloatingHeight != 0.0)
                writer.WriteAttributeString("FloatingHeight", FloatingHeight.ToString(CultureInfo.InvariantCulture));

            if (IsMaximized)
                writer.WriteAttributeString("IsMaximized", IsMaximized.ToString());
            if (!CanClose)
                writer.WriteAttributeString("CanClose", CanClose.ToString());
            if (!CanFloat)
                writer.WriteAttributeString("CanFloat", CanFloat.ToString());
        
            if (LastActivationTimeStamp != null)
                writer.WriteAttributeString("LastActivationTimeStamp", LastActivationTimeStamp.Value.ToString(CultureInfo.InvariantCulture));

            if (_previousContainer != null)
            {
                var paneSerializable = _previousContainer as ILayoutPaneSerializable;
                if (paneSerializable != null)
                {
                    writer.WriteAttributeString("PreviousContainerId", paneSerializable.Id);
                    writer.WriteAttributeString("PreviousContainerIndex", _previousContainerIndex.ToString());
                }
            }

        }

        #region FloatingWidth

        private double _floatingWidth = 0.0;
        public double FloatingWidth
        {
            get { return _floatingWidth; }
            set
            {
                if (_floatingWidth != value)
                {
                    RaisePropertyChanging("FloatingWidth");
                    _floatingWidth = value;
                    RaisePropertyChanged("FloatingWidth");
                }
            }
        }

        #endregion

        #region FloatingHeight

        private double _floatingHeight = 0.0;
        public double FloatingHeight
        {
            get { return _floatingHeight; }
            set
            {
                if (_floatingHeight != value)
                {
                    RaisePropertyChanging("FloatingHeight");
                    _floatingHeight = value;
                    RaisePropertyChanged("FloatingHeight");
                }
            }
        }

        #endregion

        #region FloatingLeft

        private double _floatingLeft = 0.0;
        public double FloatingLeft
        {
            get { return _floatingLeft; }
            set
            {
                if (_floatingLeft != value)
                {
                    RaisePropertyChanging("FloatingLeft");
                    _floatingLeft = value;
                    RaisePropertyChanged("FloatingLeft");
                }
            }
        }

        #endregion

        #region FloatingTop

        private double _floatingTop = 0.0;
        public double FloatingTop
        {
            get { return _floatingTop; }
            set
            {
                if (_floatingTop != value)
                {
                    RaisePropertyChanging("FloatingTop");
                    _floatingTop = value;
                    RaisePropertyChanged("FloatingTop");
                }
            }
        }

        #endregion

        #region IsMaximized

        private bool _isMaximized = false;
        public bool IsMaximized
        {
            get { return _isMaximized; }
            set
            {
                if (_isMaximized != value)
                {
                    RaisePropertyChanging("IsMaximized");
                    _isMaximized = value;
                    RaisePropertyChanged("IsMaximized");
                }
            }
        }

        #endregion

        #region ToolTip

        private object _toolTip = null;
        public object ToolTip
        {
            get { return _toolTip; }
            set
            {
                if (_toolTip != value)
                {
                    _toolTip = value;
                    RaisePropertyChanged("ToolTip");
                }
            }
        }

        #endregion

        public bool IsFloating
        {
            get { return this.FindParent<LayoutFloatingWindow>() != null; }
        }

        #region IconSource

        private ImageSource _iconSource = null;
        public ImageSource IconSource
        {
            get { return _iconSource; }
            set
            {
                if (_iconSource != value)
                {
                    _iconSource = value;
                    RaisePropertyChanged("IconSource");
                }
            }
        }

        #endregion

        public int CompareTo(LayoutContent other)
        {
            var contentAsComparable = Content as IComparable;
            if (contentAsComparable != null)
            {
                return contentAsComparable.CompareTo(other.Content);
            }

            return string.Compare(Title, other.Title);
        }

        /// <summary>
        /// Float the content in a popup window
        /// </summary>
        public void Float()
        {
            if (PreviousContainer != null &&
                PreviousContainer.FindParent<LayoutFloatingWindow>() != null)
            {

                var currentContainer = Parent as ILayoutPane;
                var currentContainerIndex = (currentContainer as ILayoutGroup).IndexOfChild(this);
                var previousContainerAsLayoutGroup = PreviousContainer as ILayoutGroup;

                if (PreviousContainerIndex < previousContainerAsLayoutGroup.ChildrenCount)
                    previousContainerAsLayoutGroup.InsertChildAt(PreviousContainerIndex, this);
                else
                    previousContainerAsLayoutGroup.InsertChildAt(previousContainerAsLayoutGroup.ChildrenCount, this);

                PreviousContainer = currentContainer;
                PreviousContainerIndex = currentContainerIndex;

                IsSelected = true;
                IsActive = true;

                Root.CollectGarbage();
            }
            else
            {
                Root.Manager.StartDraggingFloatingWindowForContent(this, false);

                IsSelected = true;
                IsActive = true;
            }

        }

        /// <summary>
        /// Dock the content as document
        /// </summary>
        public void DockAsDocument()
        {
            var root = Root as LayoutRoot;
            if (root == null)
                throw new InvalidOperationException();
            if (Parent is LayoutDocumentPane)
                return;

            if (PreviousContainer is LayoutDocumentPane)
            {
                Dock();
                return;
            }

            LayoutDocumentPane newParentPane;
            if (root.LastFocusedDocument != null)
            {
                newParentPane = root.LastFocusedDocument.Parent as LayoutDocumentPane;
            }
            else
            {
                newParentPane = root.Descendents().OfType<LayoutDocumentPane>().FirstOrDefault();
            }

            if (newParentPane != null)
            {
                newParentPane.Children.Add(this);
                root.CollectGarbage();
            }

            IsSelected = true;
            IsActive = true;
        }

        /// <summary>
        /// Re-dock the content to its previous container
        /// </summary>
        public void Dock()
        {
            if (PreviousContainer != null)
            {
                var currentContainer = Parent as ILayoutContainer;
                var currentContainerIndex = (currentContainer is ILayoutGroup) ? (currentContainer as ILayoutGroup).IndexOfChild(this) : -1;
                var previousContainerAsLayoutGroup = PreviousContainer as ILayoutGroup;
                if (PreviousContainerIndex < previousContainerAsLayoutGroup.ChildrenCount)
                    previousContainerAsLayoutGroup.InsertChildAt(PreviousContainerIndex, this);
                else
                    previousContainerAsLayoutGroup.InsertChildAt(previousContainerAsLayoutGroup.ChildrenCount, this);

                if (currentContainerIndex > -1)
                {
                    PreviousContainer = currentContainer;
                    PreviousContainerIndex = currentContainerIndex;
                }
                else
                {
                    PreviousContainer = null;
                    PreviousContainerIndex = 0;
                }

                IsSelected = true;
                IsActive = true;


                Root.CollectGarbage();
            }
        }


        #region CanClose

        private bool _canClose = true;
        public bool CanClose
        {
            get { return _canClose; }
            set
            {
                if (_canClose != value)
                {
                    _canClose = value;
                    RaisePropertyChanged("CanClose");
                }
            }
        }

        #endregion

        #region CanFloat

        private bool _canFloat = true;
        public bool CanFloat
        {
            get { return _canFloat; }
            set
            {
                if (_canFloat != value)
                {
                    _canFloat = value;
                    RaisePropertyChanged("CanFloat");
                }
            }
        }

        #endregion
    }
}
