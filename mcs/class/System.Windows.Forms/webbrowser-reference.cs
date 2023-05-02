// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Windows.Forms
{
    [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.LinkDemand, Name="FullTrust")]
    public sealed partial class HtmlDocument
    {
        internal HtmlDocument() { }
        public System.Windows.Forms.HtmlElement ActiveElement { get { throw null; } }
        public System.Drawing.Color ActiveLinkColor { get { throw null; } set { } }
        public System.Windows.Forms.HtmlElementCollection All { get { throw null; } }
        public System.Drawing.Color BackColor { get { throw null; } set { } }
        public System.Windows.Forms.HtmlElement Body { get { throw null; } }
        public string Cookie { get { throw null; } set { } }
        public string DefaultEncoding { get { throw null; } }
        public string Domain { get { throw null; } set { } }
        public object DomDocument { get { throw null; } }
        public string Encoding { get { throw null; } set { } }
        public bool Focused { get { throw null; } }
        public System.Drawing.Color ForeColor { get { throw null; } set { } }
        public System.Windows.Forms.HtmlElementCollection Forms { get { throw null; } }
        public System.Windows.Forms.HtmlElementCollection Images { get { throw null; } }
        public System.Drawing.Color LinkColor { get { throw null; } set { } }
        public System.Windows.Forms.HtmlElementCollection Links { get { throw null; } }
        public bool RightToLeft { get { throw null; } set { } }
        public string Title { get { throw null; } set { } }
        public System.Uri Url { get { throw null; } }
        public System.Drawing.Color VisitedLinkColor { get { throw null; } set { } }
        public System.Windows.Forms.HtmlWindow Window { get { throw null; } }
        public event System.Windows.Forms.HtmlElementEventHandler Click { add { } remove { } }
        public event System.Windows.Forms.HtmlElementEventHandler ContextMenuShowing { add { } remove { } }
        public event System.Windows.Forms.HtmlElementEventHandler Focusing { add { } remove { } }
        public event System.Windows.Forms.HtmlElementEventHandler LosingFocus { add { } remove { } }
        public event System.Windows.Forms.HtmlElementEventHandler MouseDown { add { } remove { } }
        public event System.Windows.Forms.HtmlElementEventHandler MouseLeave { add { } remove { } }
        public event System.Windows.Forms.HtmlElementEventHandler MouseMove { add { } remove { } }
        public event System.Windows.Forms.HtmlElementEventHandler MouseOver { add { } remove { } }
        public event System.Windows.Forms.HtmlElementEventHandler MouseUp { add { } remove { } }
        public event System.Windows.Forms.HtmlElementEventHandler Stop { add { } remove { } }
        public void AttachEventHandler(string eventName, System.EventHandler eventHandler) { }
        public System.Windows.Forms.HtmlElement CreateElement(string elementTag) { throw null; }
        public void DetachEventHandler(string eventName, System.EventHandler eventHandler) { }
        public override bool Equals(object obj) { throw null; }
        public void ExecCommand(string command, bool showUI, object value) { }
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        public void Focus() { }
        public System.Windows.Forms.HtmlElement GetElementById(string id) { throw null; }
        public System.Windows.Forms.HtmlElement GetElementFromPoint(System.Drawing.Point point) { throw null; }
        public System.Windows.Forms.HtmlElementCollection GetElementsByTagName(string tagName) { throw null; }
        public override int GetHashCode() { throw null; }
        public object InvokeScript(string scriptName) { throw null; }
        public object InvokeScript(string scriptName, object[] args) { throw null; }
        public System.Windows.Forms.HtmlDocument OpenNew(bool replaceInHistory) { throw null; }
        public static bool operator ==(System.Windows.Forms.HtmlDocument left, System.Windows.Forms.HtmlDocument right) { throw null; }
        public static bool operator !=(System.Windows.Forms.HtmlDocument left, System.Windows.Forms.HtmlDocument right) { throw null; }
        public void Write(string text) { }
    }
    [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.LinkDemand, Name="FullTrust")]
    public sealed partial class HtmlElement
    {
        internal HtmlElement() { }
        public System.Windows.Forms.HtmlElementCollection All { get { throw null; } }
        public bool CanHaveChildren { get { throw null; } }
        public System.Windows.Forms.HtmlElementCollection Children { get { throw null; } }
        public System.Drawing.Rectangle ClientRectangle { get { throw null; } }
        public System.Windows.Forms.HtmlDocument Document { get { throw null; } }
        public object DomElement { get { throw null; } }
        public bool Enabled { get { throw null; } set { } }
        public System.Windows.Forms.HtmlElement FirstChild { get { throw null; } }
        public string Id { get { throw null; } set { } }
        public string InnerHtml { get { throw null; } set { } }
        public string InnerText { get { throw null; } set { } }
        public string Name { get { throw null; } set { } }
        public System.Windows.Forms.HtmlElement NextSibling { get { throw null; } }
        public System.Windows.Forms.HtmlElement OffsetParent { get { throw null; } }
        public System.Drawing.Rectangle OffsetRectangle { get { throw null; } }
        public string OuterHtml { get { throw null; } set { } }
        public string OuterText { get { throw null; } set { } }
        public System.Windows.Forms.HtmlElement Parent { get { throw null; } }
        public int ScrollLeft { get { throw null; } set { } }
        public System.Drawing.Rectangle ScrollRectangle { get { throw null; } }
        public int ScrollTop { get { throw null; } set { } }
        public string Style { get { throw null; } set { } }
        public short TabIndex { get { throw null; } set { } }
        public string TagName { get { throw null; } }
        public event System.Windows.Forms.HtmlElementEventHandler Click { add { } remove { } }
        public event System.Windows.Forms.HtmlElementEventHandler DoubleClick { add { } remove { } }
        public event System.Windows.Forms.HtmlElementEventHandler Drag { add { } remove { } }
        public event System.Windows.Forms.HtmlElementEventHandler DragEnd { add { } remove { } }
        public event System.Windows.Forms.HtmlElementEventHandler DragLeave { add { } remove { } }
        public event System.Windows.Forms.HtmlElementEventHandler DragOver { add { } remove { } }
        public event System.Windows.Forms.HtmlElementEventHandler Focusing { add { } remove { } }
        public event System.Windows.Forms.HtmlElementEventHandler GotFocus { add { } remove { } }
        public event System.Windows.Forms.HtmlElementEventHandler KeyDown { add { } remove { } }
        public event System.Windows.Forms.HtmlElementEventHandler KeyPress { add { } remove { } }
        public event System.Windows.Forms.HtmlElementEventHandler KeyUp { add { } remove { } }
        public event System.Windows.Forms.HtmlElementEventHandler LosingFocus { add { } remove { } }
        public event System.Windows.Forms.HtmlElementEventHandler LostFocus { add { } remove { } }
        public event System.Windows.Forms.HtmlElementEventHandler MouseDown { add { } remove { } }
        public event System.Windows.Forms.HtmlElementEventHandler MouseEnter { add { } remove { } }
        public event System.Windows.Forms.HtmlElementEventHandler MouseLeave { add { } remove { } }
        public event System.Windows.Forms.HtmlElementEventHandler MouseMove { add { } remove { } }
        public event System.Windows.Forms.HtmlElementEventHandler MouseOver { add { } remove { } }
        public event System.Windows.Forms.HtmlElementEventHandler MouseUp { add { } remove { } }
        public System.Windows.Forms.HtmlElement AppendChild(System.Windows.Forms.HtmlElement newElement) { throw null; }
        public void AttachEventHandler(string eventName, System.EventHandler eventHandler) { }
        public void DetachEventHandler(string eventName, System.EventHandler eventHandler) { }
        public override bool Equals(object obj) { throw null; }
        public void Focus() { }
        public string GetAttribute(string attributeName) { throw null; }
        public System.Windows.Forms.HtmlElementCollection GetElementsByTagName(string tagName) { throw null; }
        public override int GetHashCode() { throw null; }
        public System.Windows.Forms.HtmlElement InsertAdjacentElement(System.Windows.Forms.HtmlElementInsertionOrientation orient, System.Windows.Forms.HtmlElement newElement) { throw null; }
        public object InvokeMember(string methodName) { throw null; }
        public object InvokeMember(string methodName, params object[] parameter) { throw null; }
        public static bool operator ==(System.Windows.Forms.HtmlElement left, System.Windows.Forms.HtmlElement right) { throw null; }
        public static bool operator !=(System.Windows.Forms.HtmlElement left, System.Windows.Forms.HtmlElement right) { throw null; }
        public void RaiseEvent(string eventName) { }
        public void RemoveFocus() { }
        public void ScrollIntoView(bool alignWithTop) { }
        public void SetAttribute(string attributeName, string value) { }
    }
    public sealed partial class HtmlElementCollection : System.Collections.ICollection, System.Collections.IEnumerable
    {
        internal HtmlElementCollection() { }
        public int Count { get { throw null; } }
        public System.Windows.Forms.HtmlElement this[int index] { get { throw null; } }
        public System.Windows.Forms.HtmlElement this[string elementId] { get { throw null; } }
        bool System.Collections.ICollection.IsSynchronized { get { throw null; } }
        object System.Collections.ICollection.SyncRoot { get { throw null; } }
        public System.Windows.Forms.HtmlElementCollection GetElementsByName(string name) { throw null; }
        public System.Collections.IEnumerator GetEnumerator() { throw null; }
        void System.Collections.ICollection.CopyTo(System.Array dest, int index) { }
    }
    public sealed partial class HtmlElementErrorEventArgs : System.EventArgs
    {
        internal HtmlElementErrorEventArgs() { }
        public string Description { get { throw null; } }
        public bool Handled { get { throw null; } set { } }
        public int LineNumber { get { throw null; } }
        public System.Uri Url { get { throw null; } }
    }
    public delegate void HtmlElementErrorEventHandler(object sender, System.Windows.Forms.HtmlElementErrorEventArgs e);
    public sealed partial class HtmlElementEventArgs : System.EventArgs
    {
        internal HtmlElementEventArgs() { }
        public bool AltKeyPressed { get { throw null; } }
        public bool BubbleEvent { get { throw null; } set { } }
        public System.Drawing.Point ClientMousePosition { get { throw null; } }
        public bool CtrlKeyPressed { get { throw null; } }
        public string EventType { get { throw null; } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        public System.Windows.Forms.HtmlElement FromElement { get { throw null; } }
        public int KeyPressedCode { get { throw null; } }
        public System.Windows.Forms.MouseButtons MouseButtonsPressed { get { throw null; } }
        public System.Drawing.Point MousePosition { get { throw null; } }
        public System.Drawing.Point OffsetMousePosition { get { throw null; } }
        public bool ReturnValue { get { throw null; } set { } }
        public bool ShiftKeyPressed { get { throw null; } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        public System.Windows.Forms.HtmlElement ToElement { get { throw null; } }
    }
    public delegate void HtmlElementEventHandler(object sender, System.Windows.Forms.HtmlElementEventArgs e);
    public enum HtmlElementInsertionOrientation
    {
        AfterBegin = 1,
        AfterEnd = 3,
        BeforeBegin = 0,
        BeforeEnd = 2,
    }
    [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.LinkDemand, Name="FullTrust")]
    public sealed partial class HtmlHistory : System.IDisposable
    {
        internal HtmlHistory() { }
        public object DomHistory { get { throw null; } }
        public int Length { get { throw null; } }
        public void Back(int numberBack) { }
        public void Dispose() { }
        public void Forward(int numberForward) { }
        public void Go(int relativePosition) { }
        public void Go(string urlString) { }
        public void Go(System.Uri url) { }
    }
    [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.LinkDemand, Name="FullTrust")]
    public sealed partial class HtmlWindow
    {
        internal HtmlWindow() { }
        public System.Windows.Forms.HtmlDocument Document { get { throw null; } }
        public object DomWindow { get { throw null; } }
        public System.Windows.Forms.HtmlWindowCollection Frames { get { throw null; } }
        public System.Windows.Forms.HtmlHistory History { get { throw null; } }
        public bool IsClosed { get { throw null; } }
        public string Name { get { throw null; } set { } }
        public System.Windows.Forms.HtmlWindow Opener { get { throw null; } }
        public System.Windows.Forms.HtmlWindow Parent { get { throw null; } }
        public System.Drawing.Point Position { get { throw null; } }
        public System.Drawing.Size Size { get { throw null; } set { } }
        public string StatusBarText { get { throw null; } set { } }
        public System.Uri Url { get { throw null; } }
        public System.Windows.Forms.HtmlElement WindowFrameElement { get { throw null; } }
        public event System.Windows.Forms.HtmlElementErrorEventHandler Error { add { } remove { } }
        public event System.Windows.Forms.HtmlElementEventHandler GotFocus { add { } remove { } }
        public event System.Windows.Forms.HtmlElementEventHandler Load { add { } remove { } }
        public event System.Windows.Forms.HtmlElementEventHandler LostFocus { add { } remove { } }
        public event System.Windows.Forms.HtmlElementEventHandler Resize { add { } remove { } }
        public event System.Windows.Forms.HtmlElementEventHandler Scroll { add { } remove { } }
        public event System.Windows.Forms.HtmlElementEventHandler Unload { add { } remove { } }
        public void Alert(string message) { }
        public void AttachEventHandler(string eventName, System.EventHandler eventHandler) { }
        public void Close() { }
        public bool Confirm(string message) { throw null; }
        public void DetachEventHandler(string eventName, System.EventHandler eventHandler) { }
        public override bool Equals(object obj) { throw null; }
        public void Focus() { }
        public override int GetHashCode() { throw null; }
        public void MoveTo(System.Drawing.Point point) { }
        public void MoveTo(int x, int y) { }
        public void Navigate(string urlString) { }
        public void Navigate(System.Uri url) { }
        public System.Windows.Forms.HtmlWindow Open(string urlString, string target, string windowOptions, bool replaceEntry) { throw null; }
        public System.Windows.Forms.HtmlWindow Open(System.Uri url, string target, string windowOptions, bool replaceEntry) { throw null; }
        public System.Windows.Forms.HtmlWindow OpenNew(string urlString, string windowOptions) { throw null; }
        public System.Windows.Forms.HtmlWindow OpenNew(System.Uri url, string windowOptions) { throw null; }
        public static bool operator ==(System.Windows.Forms.HtmlWindow left, System.Windows.Forms.HtmlWindow right) { throw null; }
        public static bool operator !=(System.Windows.Forms.HtmlWindow left, System.Windows.Forms.HtmlWindow right) { throw null; }
        public string Prompt(string message, string defaultInputValue) { throw null; }
        public void RemoveFocus() { }
        public void ResizeTo(System.Drawing.Size size) { }
        public void ResizeTo(int width, int height) { }
        public void ScrollTo(System.Drawing.Point point) { }
        public void ScrollTo(int x, int y) { }
    }
    public partial class HtmlWindowCollection : System.Collections.ICollection, System.Collections.IEnumerable
    {
        internal HtmlWindowCollection() { }
        public int Count { get { throw null; } }
        public System.Windows.Forms.HtmlWindow this[int index] { get { throw null; } }
        public System.Windows.Forms.HtmlWindow this[string windowId] { get { throw null; } }
        bool System.Collections.ICollection.IsSynchronized { get { throw null; } }
        object System.Collections.ICollection.SyncRoot { get { throw null; } }
        public System.Collections.IEnumerator GetEnumerator() { throw null; }
        void System.Collections.ICollection.CopyTo(System.Array dest, int index) { }
    }
    [System.ComponentModel.DefaultEventAttribute("DocumentCompleted")]
    [System.ComponentModel.DefaultPropertyAttribute("Url")]
    [System.ComponentModel.DesignerAttribute("System.Windows.Forms.Design.WebBrowserDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    [System.Runtime.InteropServices.ClassInterfaceAttribute(System.Runtime.InteropServices.ClassInterfaceType.AutoDispatch)]
    [System.Runtime.InteropServices.ComVisibleAttribute(true)]
    [System.Windows.Forms.DockingAttribute(System.Windows.Forms.DockingBehavior.AutoDock)]
    [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.InheritanceDemand, Name="FullTrust")]
    [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.LinkDemand, Name="FullTrust")]
    public partial class WebBrowser : System.Windows.Forms.WebBrowserBase
    {
        [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.Demand, Name="FullTrust")]
        public WebBrowser() { }
        [System.ComponentModel.DefaultValueAttribute(true)]
        public bool AllowNavigation { get { throw null; } set { } }
        [System.ComponentModel.DefaultValueAttribute(true)]
        public bool AllowWebBrowserDrop { get { throw null; } set { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public bool CanGoBack { get { throw null; } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public bool CanGoForward { get { throw null; } }
        protected override System.Drawing.Size DefaultSize { get { throw null; } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public System.Windows.Forms.HtmlDocument Document { get { throw null; } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public System.IO.Stream DocumentStream { get { throw null; } set { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public string DocumentText { get { throw null; } set { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public string DocumentTitle { get { throw null; } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public string DocumentType { get { throw null; } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public System.Windows.Forms.WebBrowserEncryptionLevel EncryptionLevel { get { throw null; } }
        public override bool Focused { get { throw null; } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public bool IsBusy { get { throw null; } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public bool IsOffline { get { throw null; } }
        [System.ComponentModel.DefaultValueAttribute(true)]
        public bool IsWebBrowserContextMenuEnabled { get { throw null; } set { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public object ObjectForScripting { get { throw null; } set { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        public new System.Windows.Forms.Padding Padding { get { throw null; } set { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public System.Windows.Forms.WebBrowserReadyState ReadyState { get { throw null; } }
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool ScriptErrorsSuppressed { get { throw null; } set { } }
        [System.ComponentModel.DefaultValueAttribute(true)]
        public bool ScrollBarsEnabled { get { throw null; } set { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public virtual string StatusText { get { throw null; } }
        [System.ComponentModel.BindableAttribute(true)]
        [System.ComponentModel.DefaultValueAttribute(null)]
        [System.ComponentModel.TypeConverterAttribute("System.Windows.Forms.WebBrowserUriTypeConverter")]
        public System.Uri Url { get { throw null; } set { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public System.Version Version { get { throw null; } }
        [System.ComponentModel.DefaultValueAttribute(true)]
        public bool WebBrowserShortcutsEnabled { get { throw null; } set { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        public event System.EventHandler CanGoBackChanged { add { } remove { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        public event System.EventHandler CanGoForwardChanged { add { } remove { } }
        public event System.Windows.Forms.WebBrowserDocumentCompletedEventHandler DocumentCompleted { add { } remove { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        public event System.EventHandler DocumentTitleChanged { add { } remove { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        public event System.EventHandler EncryptionLevelChanged { add { } remove { } }
        public event System.EventHandler FileDownload { add { } remove { } }
        public event System.Windows.Forms.WebBrowserNavigatedEventHandler Navigated { add { } remove { } }
        public event System.Windows.Forms.WebBrowserNavigatingEventHandler Navigating { add { } remove { } }
        public event System.ComponentModel.CancelEventHandler NewWindow { add { } remove { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        public new event System.EventHandler PaddingChanged { add { } remove { } }
        public event System.Windows.Forms.WebBrowserProgressChangedEventHandler ProgressChanged { add { } remove { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        public event System.EventHandler StatusTextChanged { add { } remove { } }
        protected override void AttachInterfaces(object nativeActiveXObject) { }
        protected override void CreateSink() { }
        protected override System.Windows.Forms.WebBrowserSiteBase CreateWebBrowserSiteBase() { throw null; }
        protected override void DetachInterfaces() { }
        protected override void DetachSink() { }
        protected override void Dispose(bool disposing) { }
        public bool GoBack() { throw null; }
        public bool GoForward() { throw null; }
        public void GoHome() { }
        public void GoSearch() { }
        public void Navigate(string urlString) { }
        public void Navigate(string urlString, bool newWindow) { }
        public void Navigate(string urlString, string targetFrameName) { }
        public void Navigate(string urlString, string targetFrameName, byte[] postData, string additionalHeaders) { }
        public void Navigate(System.Uri url) { }
        public void Navigate(System.Uri url, bool newWindow) { }
        public void Navigate(System.Uri url, string targetFrameName) { }
        public void Navigate(System.Uri url, string targetFrameName, byte[] postData, string additionalHeaders) { }
        protected virtual void OnCanGoBackChanged(System.EventArgs e) { }
        protected virtual void OnCanGoForwardChanged(System.EventArgs e) { }
        protected virtual void OnDocumentCompleted(System.Windows.Forms.WebBrowserDocumentCompletedEventArgs e) { }
        protected virtual void OnDocumentTitleChanged(System.EventArgs e) { }
        protected virtual void OnEncryptionLevelChanged(System.EventArgs e) { }
        protected virtual void OnFileDownload(System.EventArgs e) { }
        protected virtual void OnNavigated(System.Windows.Forms.WebBrowserNavigatedEventArgs e) { }
        protected virtual void OnNavigating(System.Windows.Forms.WebBrowserNavigatingEventArgs e) { }
        protected virtual void OnNewWindow(System.ComponentModel.CancelEventArgs e) { }
        protected virtual void OnProgressChanged(System.Windows.Forms.WebBrowserProgressChangedEventArgs e) { }
        protected virtual void OnStatusTextChanged(System.EventArgs e) { }
        public void Print() { }
        public override void Refresh() { }
        public void Refresh(System.Windows.Forms.WebBrowserRefreshOption opt) { }
        public void ShowPageSetupDialog() { }
        public void ShowPrintDialog() { }
        public void ShowPrintPreviewDialog() { }
        public void ShowPropertiesDialog() { }
        public void ShowSaveAsDialog() { }
        public void Stop() { }
        protected override void WndProc(ref System.Windows.Forms.Message m) { }
        [System.Runtime.InteropServices.ComVisibleAttribute(false)]
        [System.Security.Permissions.SecurityPermissionAttribute(System.Security.Permissions.SecurityAction.InheritanceDemand, Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)]
        protected partial class WebBrowserSite : System.Windows.Forms.WebBrowserSiteBase
        {
            [System.Security.Permissions.SecurityPermissionAttribute(System.Security.Permissions.SecurityAction.LinkDemand, Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)]
            public WebBrowserSite(System.Windows.Forms.WebBrowser host) { }
        }
    }
    [System.ComponentModel.DefaultEventAttribute("Enter")]
    [System.ComponentModel.DefaultPropertyAttribute("Name")]
    [System.ComponentModel.DesignerAttribute("System.Windows.Forms.Design.AxDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    [System.Runtime.InteropServices.ClassInterfaceAttribute(System.Runtime.InteropServices.ClassInterfaceType.AutoDispatch)]
    [System.Runtime.InteropServices.ComVisibleAttribute(true)]
    [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.InheritanceDemand, Name="FullTrust")]
    [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.LinkDemand, Name="FullTrust")]
    public partial class WebBrowserBase : System.Windows.Forms.Control
    {
        internal WebBrowserBase() { }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public object ActiveXInstance { get { throw null; } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        public override bool AllowDrop { get { throw null; } set { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        public override System.Drawing.Color BackColor { get { throw null; } set { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        public override System.Drawing.Image BackgroundImage { get { throw null; } set { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        public override System.Windows.Forms.ImageLayout BackgroundImageLayout { get { throw null; } set { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        public override System.Windows.Forms.Cursor Cursor { get { throw null; } set { } }
        protected override System.Drawing.Size DefaultSize { get { throw null; } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        public new bool Enabled { get { throw null; } set { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        public override System.Drawing.Font Font { get { throw null; } set { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        public override System.Drawing.Color ForeColor { get { throw null; } set { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        public new System.Windows.Forms.ImeMode ImeMode { get { throw null; } set { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.ComponentModel.LocalizableAttribute(false)]
        public override System.Windows.Forms.RightToLeft RightToLeft { get { throw null; } set { } }
        public override System.ComponentModel.ISite Site { set { } }
        [System.ComponentModel.BindableAttribute(false)]
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        public override string Text { get { throw null; } set { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.DesignerSerializationVisibilityAttribute(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        public new bool UseWaitCursor { get { throw null; } set { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        public new event System.EventHandler BackColorChanged { add { } remove { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        public new event System.EventHandler BackgroundImageChanged { add { } remove { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        public new event System.EventHandler BackgroundImageLayoutChanged { add { } remove { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        public new event System.EventHandler BindingContextChanged { add { } remove { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        public new event System.Windows.Forms.UICuesEventHandler ChangeUICues { add { } remove { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        public new event System.EventHandler Click { add { } remove { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        public new event System.EventHandler CursorChanged { add { } remove { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        public new event System.EventHandler DoubleClick { add { } remove { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        public new event System.Windows.Forms.DragEventHandler DragDrop { add { } remove { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        public new event System.Windows.Forms.DragEventHandler DragEnter { add { } remove { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        public new event System.EventHandler DragLeave { add { } remove { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        public new event System.Windows.Forms.DragEventHandler DragOver { add { } remove { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        public new event System.EventHandler EnabledChanged { add { } remove { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        public new event System.EventHandler Enter { add { } remove { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        public new event System.EventHandler FontChanged { add { } remove { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        public new event System.EventHandler ForeColorChanged { add { } remove { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        public new event System.Windows.Forms.GiveFeedbackEventHandler GiveFeedback { add { } remove { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        public new event System.Windows.Forms.HelpEventHandler HelpRequested { add { } remove { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        public new event System.EventHandler ImeModeChanged { add { } remove { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        public new event System.Windows.Forms.KeyEventHandler KeyDown { add { } remove { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        public new event System.Windows.Forms.KeyPressEventHandler KeyPress { add { } remove { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        public new event System.Windows.Forms.KeyEventHandler KeyUp { add { } remove { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        public new event System.Windows.Forms.LayoutEventHandler Layout { add { } remove { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        public new event System.EventHandler Leave { add { } remove { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        public new event System.EventHandler MouseCaptureChanged { add { } remove { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        public new event System.Windows.Forms.MouseEventHandler MouseClick { add { } remove { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        public new event System.Windows.Forms.MouseEventHandler MouseDoubleClick { add { } remove { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        public new event System.Windows.Forms.MouseEventHandler MouseDown { add { } remove { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        public new event System.EventHandler MouseEnter { add { } remove { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        public new event System.EventHandler MouseHover { add { } remove { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        public new event System.EventHandler MouseLeave { add { } remove { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        public new event System.Windows.Forms.MouseEventHandler MouseMove { add { } remove { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        public new event System.Windows.Forms.MouseEventHandler MouseUp { add { } remove { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        public new event System.Windows.Forms.MouseEventHandler MouseWheel { add { } remove { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        public new event System.Windows.Forms.PaintEventHandler Paint { add { } remove { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        public new event System.Windows.Forms.QueryAccessibilityHelpEventHandler QueryAccessibilityHelp { add { } remove { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        public new event System.Windows.Forms.QueryContinueDragEventHandler QueryContinueDrag { add { } remove { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        public new event System.EventHandler RightToLeftChanged { add { } remove { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        public new event System.EventHandler StyleChanged { add { } remove { } }
        [System.ComponentModel.BrowsableAttribute(false)]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        public new event System.EventHandler TextChanged { add { } remove { } }
        protected virtual void AttachInterfaces(object nativeActiveXObject) { }
        protected virtual void CreateSink() { }
        protected virtual System.Windows.Forms.WebBrowserSiteBase CreateWebBrowserSiteBase() { throw null; }
        protected virtual void DetachInterfaces() { }
        protected virtual void DetachSink() { }
        protected override void Dispose(bool disposing) { }
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        public new void DrawToBitmap(System.Drawing.Bitmap bitmap, System.Drawing.Rectangle targetBounds) { }
        protected override bool IsInputChar(char charCode) { throw null; }
        protected override void OnBackColorChanged(System.EventArgs e) { }
        protected override void OnFontChanged(System.EventArgs e) { }
        protected override void OnForeColorChanged(System.EventArgs e) { }
        protected override void OnGotFocus(System.EventArgs e) { }
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        protected override void OnHandleCreated(System.EventArgs e) { }
        protected override void OnLostFocus(System.EventArgs e) { }
        protected override void OnParentChanged(System.EventArgs e) { }
        protected override void OnRightToLeftChanged(System.EventArgs e) { }
        protected override void OnVisibleChanged(System.EventArgs e) { }
        public override bool PreProcessMessage(ref System.Windows.Forms.Message msg) { throw null; }
        [System.Security.Permissions.UIPermissionAttribute(System.Security.Permissions.SecurityAction.LinkDemand, Window=System.Security.Permissions.UIPermissionWindow.AllWindows)]
        protected override bool ProcessDialogKey(System.Windows.Forms.Keys keyData) { throw null; }
        [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.InheritanceDemand, Name="FullTrust")]
        [System.Security.Permissions.SecurityPermissionAttribute(System.Security.Permissions.SecurityAction.LinkDemand, Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)]
        [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.LinkDemand, Name="FullTrust")]
        protected override void WndProc(ref System.Windows.Forms.Message m) { }
    }
    public partial class WebBrowserDocumentCompletedEventArgs : System.EventArgs
    {
        public WebBrowserDocumentCompletedEventArgs(System.Uri url) { }
        public System.Uri Url { get { throw null; } }
    }
    public delegate void WebBrowserDocumentCompletedEventHandler(object sender, System.Windows.Forms.WebBrowserDocumentCompletedEventArgs e);
    public enum WebBrowserEncryptionLevel
    {
        Bit128 = 6,
        Bit40 = 3,
        Bit56 = 4,
        Fortezza = 5,
        Insecure = 0,
        Mixed = 1,
        Unknown = 2,
    }
    public partial class WebBrowserNavigatedEventArgs : System.EventArgs
    {
        public WebBrowserNavigatedEventArgs(System.Uri url) { }
        public System.Uri Url { get { throw null; } }
    }
    public delegate void WebBrowserNavigatedEventHandler(object sender, System.Windows.Forms.WebBrowserNavigatedEventArgs e);
    public partial class WebBrowserNavigatingEventArgs : System.ComponentModel.CancelEventArgs
    {
        public WebBrowserNavigatingEventArgs(System.Uri url, string targetFrameName) { }
        public string TargetFrameName { get { throw null; } }
        public System.Uri Url { get { throw null; } }
    }
    public delegate void WebBrowserNavigatingEventHandler(object sender, System.Windows.Forms.WebBrowserNavigatingEventArgs e);
    public partial class WebBrowserProgressChangedEventArgs : System.EventArgs
    {
        public WebBrowserProgressChangedEventArgs(long currentProgress, long maximumProgress) { }
        public long CurrentProgress { get { throw null; } }
        public long MaximumProgress { get { throw null; } }
    }
    public delegate void WebBrowserProgressChangedEventHandler(object sender, System.Windows.Forms.WebBrowserProgressChangedEventArgs e);
    public enum WebBrowserReadyState
    {
        Complete = 4,
        Interactive = 3,
        Loaded = 2,
        Loading = 1,
        Uninitialized = 0,
    }
    public enum WebBrowserRefreshOption
    {
        Completely = 3,
        Continue = 2,
        IfExpired = 1,
        Normal = 0,
    }
    public partial class WebBrowserSiteBase : System.IDisposable
    {
        internal WebBrowserSiteBase() { }
        public void Dispose() { }
        protected virtual void Dispose(bool disposing) { }
    }
}
