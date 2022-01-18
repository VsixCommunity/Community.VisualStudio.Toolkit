/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation.
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A
 * copy of the license can be found in the License.html file at the root of this distribution. If
 * you cannot locate the Apache License, Version 2.0, please send an email to
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * This source code has been modified from its original form.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// A base class for custom language services and editor factories
    /// </summary>
    public abstract class LanguageBase : LanguageService, IVsEditorFactory
    {
        private readonly Package _package;
        private readonly Guid _languageServiceId;

        #region Language Service

        private LanguagePreferences? _preferences = null;

        /// <summary>
        /// Creates a new instance of the language.
        /// </summary>
        public LanguageBase(object site)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            SetSite(site);

            _package = (Package)site;
            _languageServiceId = GetType().GUID;
        }

        /// <inheritdoc/>
        public override int GetLanguageName(out string name)
        {
            name = Name;
            return VSConstants.S_OK;
        }

        /// <inheritdoc/>
        public abstract override string Name { get; }

        /// <summary>
        /// An array of file extensions associated with this language.
        /// </summary>
        public abstract string[] FileExtensions { get; }

        /// <summary>
        /// Set the default preferences for this language.
        /// </summary>
        public abstract void SetDefaultPreferences(LanguagePreferences preferences);

        /// <inheritdoc/>
        public override LanguagePreferences GetLanguagePreferences()
        {
            if (_preferences == null)
            {
                _preferences = new LanguagePreferences(Site, _languageServiceId, Name);
                _preferences.Init();
                SetDefaultPreferences(_preferences);
            }

            return _preferences;
        }

        /// <inheritdoc/>
        public override IScanner GetScanner(IVsTextLines buffer) => null!;

        /// <inheritdoc/>
        public override AuthoringScope ParseSource(ParseRequest req) => null!;

        /// <inheritdoc/>
        public override string GetFormatFilterList()
        {
            IEnumerable<string> normalized = FileExtensions.Select(f => $"*{f}");
            string first = string.Join(", ", normalized);
            string second = string.Join(";", normalized);

            return $"{Name} File ({first})|{second}";
        }

        #endregion

        #region IVsEditorFactory

        private ServiceProvider? _serviceProvider;

        /// <summary>
        /// Creates a new instance of a language service and editor factory.
        /// </summary>
        public LanguageBase(Package package, Guid languageServiceId)
        {
            _package = package;
            _languageServiceId = languageServiceId;
        }

        /// <inheritdoc/>
        protected virtual bool PromptEncodingOnLoad => false;

        /// <inheritdoc/>
        public virtual int SetSite(IOleServiceProvider psp)
        {
            _serviceProvider = new ServiceProvider(psp);
            return VSConstants.S_OK;
        }

        // This method is called by the Environment (inside IVsUIShellOpenDocument::
        // OpenStandardEditor and OpenSpecificEditor) to map a LOGICAL view to a
        // PHYSICAL view. A LOGICAL view identifies the purpose of the view that is
        // desired (e.g. a view appropriate for Debugging [LOGVIEWID_Debugging], or a
        // view appropriate for text view manipulation as by navigating to a find
        // result [LOGVIEWID_TextView]). A PHYSICAL view identifies an actual type
        // of view implementation that an IVsEditorFactory can create.
        //
        // NOTE: Physical views are identified by a string of your choice with the
        // one constraint that the default/primary physical view for an editor
        // *MUST* use a NULL string as its physical view name (*pbstrPhysicalView = NULL).
        //
        // NOTE: It is essential that the implementation of MapLogicalView properly
        // validates that the LogicalView desired is actually supported by the editor.
        // If an unsupported LogicalView is requested then E_NOTIMPL must be returned.
        //
        // NOTE: The special Logical Views supported by an Editor Factory must also
        // be registered in the local registry hive. LOGVIEWID_Primary is implicitly
        // supported by all editor types and does not need to be registered.
        // For example, an editor that supports a ViewCode/ViewDesigner scenario
        // might register something like the following:
        //        HKLM\Software\Microsoft\VisualStudio\9.0\Editors\
        //            {...guidEditor...}\
        //                LogicalViews\
        //                    {...LOGVIEWID_TextView...} = s ''
        //                    {...LOGVIEWID_Code...} = s ''
        //                    {...LOGVIEWID_Debugging...} = s ''
        //                    {...LOGVIEWID_Designer...} = s 'Form'
        //
        /// <inheritdoc/>
        public virtual int MapLogicalView(ref Guid logicalView, out string? physicalView)
        {
            // initialize out parameter
            physicalView = null;

            bool isSupportedView = false;
            // Determine the physical view
            if (VSConstants.LOGVIEWID_Primary == logicalView ||
                VSConstants.LOGVIEWID_Debugging == logicalView ||
                VSConstants.LOGVIEWID_Code == logicalView ||
                VSConstants.LOGVIEWID_UserChooseView == logicalView ||
                VSConstants.LOGVIEWID_TextView == logicalView)
            {
                // primary view uses NULL as pbstrPhysicalView
                isSupportedView = true;
            }
            else if (VSConstants.LOGVIEWID_Designer == logicalView)
            {
                physicalView = "Design";
                isSupportedView = true;
            }

            if (isSupportedView)
            {
                return VSConstants.S_OK;
            }
            else
            {
                // E_NOTIMPL must be returned for any unrecognized rguidLogicalView values
                return VSConstants.E_NOTIMPL;
            }
        }

        /// <inheritdoc/>
        public virtual int Close()
        {
            return VSConstants.S_OK;
        }

        /// <inheritdoc/>
        public virtual int CreateEditorInstance(
                        uint createEditorFlags,
                        string documentMoniker,
                        string physicalView,
                        IVsHierarchy hierarchy,
                        uint itemid,
                        IntPtr docDataExisting,
                        out IntPtr docView,
                        out IntPtr docData,
                        out string? editorCaption,
                        out Guid commandUIGuid,
                        out int createDocumentWindowFlags)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // Initialize output parameters
            docView = IntPtr.Zero;
            docData = IntPtr.Zero;
            commandUIGuid = Guid.Empty;
            createDocumentWindowFlags = 0;
            editorCaption = null;

            // Validate inputs
            if ((createEditorFlags & (uint)(VSConstants.CEF.OpenFile | VSConstants.CEF.Silent)) == 0)
            {
                return VSConstants.E_INVALIDARG;
            }

            if (docDataExisting != IntPtr.Zero && PromptEncodingOnLoad)
            {
                return VSConstants.VS_E_INCOMPATIBLEDOCDATA;
            }

            // Get a text buffer
            IVsTextLines? textLines = GetTextBuffer(docDataExisting, documentMoniker);

            // Assign docData IntPtr to either existing docData or the new text buffer
            if (docDataExisting != IntPtr.Zero)
            {
                docData = docDataExisting;
                Marshal.AddRef(docData);
            }
            else
            {
                docData = Marshal.GetIUnknownForObject(textLines);
            }

            try
            {
                object? docViewObject = CreateDocumentView(documentMoniker, physicalView, hierarchy, itemid, textLines, docDataExisting == IntPtr.Zero, out editorCaption, out commandUIGuid);
                docView = Marshal.GetIUnknownForObject(docViewObject);
            }
            finally
            {
                if (docView == IntPtr.Zero)
                {
                    if (docDataExisting != docData && docData != IntPtr.Zero)
                    {
                        // Cleanup the instance of the docData that we have addref'ed
                        Marshal.Release(docData);
                        docData = IntPtr.Zero;
                    }
                }
            }
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Gets the text view lines from the doc data.
        /// </summary>
        protected virtual IVsTextLines? GetTextBuffer(IntPtr docDataExisting, string filename)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            IVsTextLines? textLines;
            if (docDataExisting == IntPtr.Zero)
            {
                // Create a new IVsTextLines buffer.
                Type textLinesType = typeof(IVsTextLines);
                Guid riid = textLinesType.GUID;
                Guid clsid = typeof(VsTextBufferClass).GUID;
                textLines = _package.CreateInstance(ref clsid, ref riid, textLinesType) as IVsTextLines;

                // set the buffer's site
                IObjectWithSite objectWithSite = (IObjectWithSite)textLines!;
                objectWithSite.SetSite(_serviceProvider?.GetService(typeof(IOleServiceProvider)));
            }
            else
            {
                // Use the existing text buffer
                object? dataObject = Marshal.GetObjectForIUnknown(docDataExisting);
                textLines = dataObject as IVsTextLines;
                if (textLines == null)
                {
                    // Try get the text buffer from textbuffer provider
                    if (dataObject is IVsTextBufferProvider textBufferProvider)
                    {
                        textBufferProvider.GetTextBuffer(out textLines);
                    }
                }
                if (textLines == null)
                {
                    // Unknown docData type then, so we have to force VS to close the other editor.
                    throw Marshal.GetExceptionForHR(VSConstants.VS_E_INCOMPATIBLEDOCDATA);
                }

            }
            return textLines;
        }

        /// <summary>
        /// Creates the document view
        /// </summary>
        protected virtual object CreateDocumentView(string documentMoniker, string physicalView, IVsHierarchy hierarchy, uint itemid, IVsTextLines? textLines, bool createdDocData, out string editorCaption, out Guid cmdUI)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            //Init out params
            editorCaption = string.Empty;
            cmdUI = Guid.Empty;

            if (string.IsNullOrEmpty(physicalView))
            {
                // create code window as default physical view
                return CreateCodeView(documentMoniker, textLines, createdDocData, ref editorCaption, ref cmdUI);
            }

            // We couldn't create the view
            // Return special error code so VS can try another editor factory.
            throw Marshal.GetExceptionForHR(VSConstants.VS_E_UNSUPPORTEDFORMAT);
        }

        /// <summary>
        /// Creates the code view.
        /// </summary>
        protected virtual IVsCodeWindow CreateCodeView(string documentMoniker, IVsTextLines? textLines, bool createdDocData, ref string editorCaption, ref Guid cmdUI)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (_serviceProvider == null)
            {
                throw new Exception("ServiceProvider can't be null");
            }

            IVsEditorAdaptersFactoryService adapterService = VS.GetMefService<IVsEditorAdaptersFactoryService>();

            IVsCodeWindow window = adapterService.CreateVsCodeWindowAdapter((IOleServiceProvider)_serviceProvider.GetService(typeof(IOleServiceProvider)));
            ErrorHandler.ThrowOnFailure(window.SetBuffer(textLines));
            ErrorHandler.ThrowOnFailure(window.SetBaseEditorCaption(null));
            ErrorHandler.ThrowOnFailure(window.GetEditorCaption(READONLYSTATUS.ROSTATUS_Unknown, out editorCaption));

            if (textLines is IVsUserData userData)
            {
                if (PromptEncodingOnLoad)
                {
                    Guid guid = VSConstants.VsTextBufferUserDataGuid.VsBufferEncodingPromptOnLoad_guid;
                    userData.SetData(ref guid, (uint)1);
                }
            }

            cmdUI = VSConstants.GUID_TextEditorFactory;

            if (!createdDocData && textLines != null)
            {
                // we have a pre-created buffer, go ahead and initialize now as the buffer already
                // exists and is initialized
                TextBufferEventListener? bufferEventListener = new(textLines, _languageServiceId);
                bufferEventListener.OnLoadCompleted(0);
            }

            return window;
        }

        private sealed class TextBufferEventListener : IVsTextBufferDataEvents
        {
            private readonly IVsTextLines _textLines;

            private readonly IConnectionPoint _connectionPoint;
            private readonly uint _cookie;
            private Guid _languageServiceId;

            public TextBufferEventListener(IVsTextLines textLines, Guid languageServiceId)
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                _textLines = textLines;
                _languageServiceId = languageServiceId;

                IConnectionPointContainer? connectionPointContainer = textLines as IConnectionPointContainer;
                Guid bufferEventsGuid = typeof(IVsTextBufferDataEvents).GUID;
                connectionPointContainer?.FindConnectionPoint(ref bufferEventsGuid, out _connectionPoint);
                _connectionPoint!.Advise(this, out _cookie);
            }

            public void OnFileChanged(uint grfChange, uint dwFileAttrs)
            {
            }

            public int OnLoadCompleted(int fReload)
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                _connectionPoint.Unadvise(_cookie);
                _textLines.SetLanguageServiceID(ref _languageServiceId);

                return VSConstants.S_OK;
            }
        }

        #endregion
    }
}