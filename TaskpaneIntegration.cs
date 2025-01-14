﻿using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swpublished;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace WebView2.SolidWorks.AddIn
{
    [Guid("060E2203-CCC3-4FDD-A9AC-C7C7F079DEA4"), ComVisible(true)]
    /// <summary>
    /// Our SolidWorks taskpane add-in
    /// </summary>

    public class TaskpaneIntegration : ISwAddin
    {
        #region Private Members

        /// <summary>
        /// The cookie to the current instance of SolidWorks we are running inside of
        /// </summary>
        private int mSwCookie;

        /// <summary>
        /// The taskpane view for our add-in
        /// </summary>
        private TaskpaneView mTaskpaneView;

        /// <summary>
        /// The UI control that is going to be inside the SolidWorks taskpane view
        /// </summary>
        private TaskpaneHostUI mTaskpaneHost;

        /// <summary>
        /// The current instance of the SolidWorks application
        /// </summary>
        private SldWorks mSolidWorksApplication;

        #endregion

        #region Public Members

        /// <summary>
        /// The unique Id to the taskpane used for registration in COM
        /// </summary>
        public const string SWTASKPANE_PROGID = "WebView.Taskpane";

        #endregion

        #region SolidWorks Add-in Callbacks

        /// <summary>
        /// Called when SolidWorks has loaded our add-in and wants us to do our connection logic
        /// </summary>
        /// <param name="ThisSW">The current SolidWorks instance</param>
        /// <param name="Cookie">The current SolidWorks cookie Id</param>
        /// <returns></returns>
        public bool ConnectToSW(object ThisSW, int Cookie)
        {
            // Store a reference to the current SolidWorks instance
            mSolidWorksApplication = (SldWorks)ThisSW;

            // Store cookie Id
            mSwCookie = Cookie;

            // Setup callback info
            var ok = mSolidWorksApplication.SetAddinCallbackInfo2(0, this, mSwCookie);


            // load cef sharp 
            loadCefRuntime();

            // Create our UI
            LoadUI();

            // Return ok
            return true;
        }

        private void loadCefRuntime()
        {
            var thisAssemblyFile = new FileInfo(this.GetType().Assembly.Location);
            var thisAssemblyDir = thisAssemblyFile.Directory;

            //System.Reflection.Assembly.LoadFrom(System.IO.Path.Combine(thisAssemblyDir.FullName, "libcef.dll"));

           // System.Reflection.Assembly.LoadFrom(System.IO.Path.Combine(thisAssemblyDir.FullName, "libEGL.dll"));

            System.Reflection.Assembly.LoadFrom(System.IO.Path.Combine(thisAssemblyDir.FullName, "cefsharp.dll"));
            System.Reflection.Assembly.LoadFrom(System.IO.Path.Combine(thisAssemblyDir.FullName, "cefsharp.core.dll"));
            System.Reflection.Assembly.LoadFrom(System.IO.Path.Combine(thisAssemblyDir.FullName, "cefsharp.core.runtime.dll"));
            System.Reflection.Assembly.LoadFrom(System.IO.Path.Combine(thisAssemblyDir.FullName, "cefsharp.wpf.dll"));
        }

        /// <summary>
        /// Called when SolidWorks is about to unload our add-in and wants us to do our disconnection logic
        /// </summary>
        /// <returns></returns>
        public bool DisconnectFromSW()
        {
            // Clean up our UI
            UnloadUI();

            // Return ok
            return true;
        }

        #endregion

        #region Create UI

        /// <summary>
        /// Create our Taskpane and inject our host UI
        /// </summary>
        private void LoadUI()
        {
            // Find location to our taskpane icon
            var imagePath = Path.Combine(Path.GetDirectoryName(typeof(TaskpaneIntegration).Assembly.CodeBase).Replace(@"file:\", string.Empty), "logo-small.bmp");

            // Create our Taskpane
            mTaskpaneView = mSolidWorksApplication.CreateTaskpaneView2(imagePath, "Web View 2 task pane");

            // Load our UI into the taskpane
            mTaskpaneHost = (TaskpaneHostUI)mTaskpaneView.AddControl(TaskpaneIntegration.SWTASKPANE_PROGID, string.Empty);
        }

        /// <summary>
        /// Cleanup the taskpane when we disconnect/unload
        /// </summary>
        private void UnloadUI()
        {
            mTaskpaneHost = null;

            // Remove taskpane view
            mTaskpaneView.DeleteView();

            // Release COM reference and cleanup memory
            Marshal.ReleaseComObject(mTaskpaneView);

            mTaskpaneView = null;
        }

        #endregion

        #region COM Registration

        /// <summary>
        /// The COM registration call to add our registry entries to the SolidWorks add-in registry
        /// </summary>
        /// <param name="t"></param>
        [ComRegisterFunction()]
        private static void ComRegister(Type t)
        {
            var keyPath = string.Format(@"SOFTWARE\SolidWorks\AddIns\{0:b}", t.GUID);

            // Create our registry folder for the add-in
            using (var rk = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(keyPath))
            {
                // Load add-in when SolidWorks opens
                rk.SetValue(null, 1);

                // Set SolidWorks add-in title and description
                rk.SetValue("Title", "Web View 2");
                rk.SetValue("Description", "Show web page in SW");
            }
        }

        /// <summary>
        /// The COM unregister call to remove our custom entries we added in the COM register function
        /// </summary>
        /// <param name="t"></param>
        [ComUnregisterFunction()]
        private static void ComUnregister(Type t)
        {
            var keyPath = string.Format(@"SOFTWARE\SolidWorks\AddIns\{0:b}", t.GUID);

            // Remove our registry entry
            Microsoft.Win32.Registry.LocalMachine.DeleteSubKeyTree(keyPath);

        }

        #endregion
    }
}
