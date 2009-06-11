/**************************************************
 * 
 * Copyright 2009 Garrett Kiel, Cory Luitjohan, Feng Cao, Phil Slama, Ed Han, Michael Covert
 * 
 * This file is part of Shader Sense.
 *
 *   Shader Sense is free software: you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation, either version 3 of the License, or
 *   (at your option) any later version.
 *
 *   Shader Sense is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU General Public License for more details.
 *
 *   You should have received a copy of the GNU General Public License
 *   along with Shader Sense.  If not, see <http://www.gnu.org/licenses/>.
 *
 *************************************************/

// VsPkg.cs : Implementation of ShaderSense
//

using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;

namespace Company.ShaderSense
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the registration utility (regpkg.exe) that this class needs
    // to be registered as package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // A Visual Studio component can be registered under different regitry roots; for instance
    // when you debug your package you want to register it in the experimental hive. This
    // attribute specifies the registry root to use if no one is provided to regpkg.exe with
    // the /root switch.
    [DefaultRegistryRoot("Software\\Microsoft\\VisualStudio\\9.0")]
    // This attribute is used to register the informations needed to show the this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration(false, "#110", "#112", "1.0", IconResourceID = 400)]
    // In order be loaded inside Visual Studio in a machine that has not the VS SDK installed, 
    // package needs to have a valid load key (it can be requested at 
    // http://msdn.microsoft.com/vstudio/extend/). This attributes tells the shell that this 
    // package has a load key embedded in its resources.
    [ProvideLoadKey("Standard", "1.0", "Shader Sense", "", 1)]
    [Guid(GuidList.guidShaderSensePkgString)]
    [ProvideService(typeof(Babel.HLSLLanguageService))]
    [ProvideLanguageExtension(typeof(Babel.HLSLLanguageService), Babel.Configuration.Extension)]
    [ProvideLanguageExtension(typeof(Babel.HLSLLanguageService), ".fxc")]
    [ProvideLanguageService(typeof(Babel.HLSLLanguageService), Babel.Configuration.Name, 0,
        CodeSense=true,
        EnableCommenting=true,
        MatchBraces=true,
        ShowCompletion=true,
        ShowMatchingBrace=true,
        MatchBracesAtCaret=true
    )]
    public sealed class ShaderSensePackage : Package, IOleComponent
    {
        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public ShaderSensePackage()
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }

        private Babel.HLSLLanguageService _languageService;
        private uint componentID = 0;

        /////////////////////////////////////////////////////////////////////////////
        // Overriden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initilaization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Trace.WriteLine (string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            _languageService = new Babel.HLSLLanguageService();
            _languageService.SetSite(this);

            IServiceContainer serviceContainer = (IServiceContainer)this;
            serviceContainer.AddService(typeof(Babel.HLSLLanguageService), _languageService, true);

            _languageService.Preferences.ParameterInformation = true;

            IOleComponentManager componentManager = (IOleComponentManager)this.GetService(typeof(SOleComponentManager));
            if (componentID == 0 && componentManager != null)
            {
                OLECRINFO[] crinfo = new OLECRINFO[1];
                crinfo[0].cbSize = (uint)Marshal.SizeOf(typeof(OLECRINFO));
                crinfo[0].grfcrf = (uint)_OLECRF.olecrfNeedIdleTime | (uint)_OLECRF.olecrfNeedPeriodicIdleTime;
                crinfo[0].grfcadvf = (uint)_OLECADVF.olecadvfModal | (uint)_OLECADVF.olecadvfRedrawOff | (uint)_OLECADVF.olecadvfWarningsOff;
                crinfo[0].uIdleTimeInterval = 1000;
                componentManager.FRegisterComponent(this, crinfo, out componentID);
            }
        }
        #endregion

        #region IOleComponent methods
        /// <include file='doc\Package.uex' path='docs/doc[@for="Package.FDoIdle"]/*' />
        public int FDoIdle(uint grfidlef)
        {
            bool periodic = ((grfidlef & (uint)_OLEIDLEF.oleidlefPeriodic) != 0);
            Babel.HLSLLanguageService svc = (Babel.HLSLLanguageService)GetService(typeof(Babel.HLSLLanguageService));
            if (svc != null)
            {
                svc.OnIdle(periodic);
            }
            return 0;
        }
        /// <include file='doc\Package.uex' path='docs/doc[@for="Package.Terminate"]/*' />
        public void Terminate()
        {
        }
        /// <include file='doc\Package.uex' path='docs/doc[@for="Package.FPreTranslateMessage"]/*' />
        public int FPreTranslateMessage(MSG[] msg)
        {
            return 0;
        }
        /// <include file='doc\Package.uex' path='docs/doc[@for="Package.OnEnterState"]/*' />
        public void OnEnterState(uint uStateID, int fEnter)
        {
        }
        /// <include file='doc\Package.uex' path='docs/doc[@for="Package.OnAppActivate"]/*' />
        public void OnAppActivate(int fActive, uint dwOtherThreadID)
        {
        }
        /// <include file='doc\Package.uex' path='docs/doc[@for="Package.OnLoseActivation"]/*' />
        public void OnLoseActivation()
        {
        }
        /// <include file='doc\Package.uex' path='docs/doc[@for="Package.OnActivationChange"]/*' />
        public void OnActivationChange(Microsoft.VisualStudio.OLE.Interop.IOleComponent pic, int fSameComponent, OLECRINFO[] pcrinfo, int fHostIsActivating, OLECHOSTINFO[] pchostinfo, uint dwReserved)
        {
        }
        /// <include file='doc\Package.uex' path='docs/doc[@for="Package.FContinueMessageLoop"]/*' />
        public int FContinueMessageLoop(uint uReason, IntPtr pvLoopData, MSG[] pMsgPeeked)
        {
            return 1;
        }
        /// <include file='doc\Package.uex' path='docs/doc[@for="Package.FQueryTerminate"]/*' />
        public int FQueryTerminate(int fPromptUser)
        {
            return 1;
        }
        /// <include file='doc\Package.uex' path='docs/doc[@for="Package.HwndGetWindow"]/*' />
        public IntPtr HwndGetWindow(uint dwWhich, uint dwReserved)
        {
            return IntPtr.Zero;
        }
        /// <include file='doc\Package.uex' path='docs/doc[@for="Package.FReserved1"]/*' />
        public int FReserved1(uint reserved, uint message, IntPtr wParam, IntPtr lParam)
        {
            return 1;
        }
        #endregion

    }
}