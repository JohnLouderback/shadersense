/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.IO;
using System.Collections;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.VsSDK.UnitTestLibrary;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Company.ShaderSense;
using System.Windows.Forms;
using System.Drawing;
using EnvDTE;
using EnvDTE80;

namespace UnitTestProject
{
    [TestClass()]
    public class PackageTest
    {        
        [DllImport("ole32.dll")]
        static extern int GetRunningObjectTable(uint reserved, out IRunningObjectTable pprot);

        [DllImport("ole32.dll")]

        static extern int CreateBindCtx(uint reserved, out IBindCtx pctx);
        const int SLEEP_TIME = 10;

        //look-up the DTE from Running Object Table
        private static DTE2 SeekDTE2InstanceFromROT(String moniker)
        {
            IRunningObjectTable prot = null;
            IEnumMoniker pmonkenum = null;
            IntPtr pfeteched = IntPtr.Zero;

            DTE2 ret = null;
            try
            {
                //get running object table
                if ((GetRunningObjectTable(0, out prot) != 0) || (prot == null))
                {
                    return ret;
                }

                prot.EnumRunning(out pmonkenum);
                pmonkenum.Reset();
                IMoniker[] monikers = new IMoniker[1];

                while (pmonkenum.Next(1, monikers, pfeteched) == 0)
                {
                    String insname;
                    IBindCtx pctx;

                    CreateBindCtx(0, out pctx);
                    monikers[0].GetDisplayName(pctx, null, out insname);
                    Marshal.ReleaseComObject(pctx);

                    if (string.Compare(insname, moniker) == 0) //lookup by item moniker
                    {
                        Object obj;
                        prot.GetObject(monikers[0], out obj);
                        ret = (DTE2)obj;
                    }
                }
            }

            finally
            {
                if (prot != null) Marshal.ReleaseComObject(prot);
                if (pmonkenum != null) Marshal.ReleaseComObject(pmonkenum);
            }

            return ret;
        }

        public class MessageFilter : IOleMessageFilter
        {
            //
            // Class containing the IOleMessageFilter
            // thread error-handling functions.

            // Start the filter.
            public static void Register()
            {
                IOleMessageFilter newFilter = new MessageFilter();
                IOleMessageFilter oldFilter = null;
                CoRegisterMessageFilter(newFilter, out oldFilter);
            }

            // Done with the filter, close it.
            public static void Revoke()
            {
                IOleMessageFilter oldFilter = null;
                CoRegisterMessageFilter(null, out oldFilter);
            }

            //
            // IOleMessageFilter functions.
            // Handle incoming thread requests.
            int IOleMessageFilter.HandleInComingCall(int dwCallType,
              System.IntPtr hTaskCaller, int dwTickCount, System.IntPtr
              lpInterfaceInfo)
            {
                //Return the flag SERVERCALL_ISHANDLED.
                return 0;
            }

            // Thread call was rejected, so try again.
            int IOleMessageFilter.RetryRejectedCall(System.IntPtr
              hTaskCallee, int dwTickCount, int dwRejectType)
            {
                if (dwRejectType == 2)
                // flag = SERVERCALL_RETRYLATER.
                {
                    // Retry the thread call immediately if return >=0 & 
                    // <100.
                    return 99;
                }
                // Too busy; cancel call.
                return -1;
            }

            int IOleMessageFilter.MessagePending(System.IntPtr hTaskCallee,
              int dwTickCount, int dwPendingType)
            {
                //Return the flag PENDINGMSG_WAITDEFPROCESS.
                return 2;
            }

            // Implement the IOleMessageFilter interface.
            [DllImport("Ole32.dll")]
            private static extern int
              CoRegisterMessageFilter(IOleMessageFilter newFilter, out 
          IOleMessageFilter oldFilter);
        }

        [ComImport(), Guid("00000016-0000-0000-C000-000000000046"),
        InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
        interface IOleMessageFilter
        {
            [PreserveSig]
            int HandleInComingCall(
                int dwCallType,
                IntPtr hTaskCaller,
                int dwTickCount,
                IntPtr lpInterfaceInfo);

            [PreserveSig]
            int RetryRejectedCall(
                IntPtr hTaskCallee,
                int dwTickCount,
                int dwRejectType);

            [PreserveSig]
            int MessagePending(
                IntPtr hTaskCallee,
                int dwTickCount,
                int dwPendingType);
        }

        //helper function to compare our output and the test output
        private bool CompareFiles(string File1, string File2)
        {
            FileInfo FI1 = new FileInfo(File1);
            FileInfo FI2 = new FileInfo(File2);

            if (FI1.Length != FI2.Length)
                return false;

            byte[] bytesFile1 = File.ReadAllBytes(File1);
            byte[] bytesFile2 = File.ReadAllBytes(File2);

            if (bytesFile1.Length != bytesFile2.Length)
                return false;

            for (int index = 0; index <= bytesFile2.Length - 1; index++)
            {
                if (bytesFile1[index] != bytesFile2[index])
                    return false;
            }
            return true;
        }

        //framework that opens test VS2008 instances are does commands
        public bool[] ShaderSenseColorTestFramework(String[] testFiles)
        {
            //switch the current working directory to the ShaderSense directory
            while (Directory.GetCurrentDirectory().Contains("ShaderSense"))
            {
                Directory.SetCurrentDirectory((Directory.GetParent(Directory.GetCurrentDirectory()).ToString()));
            }

            Directory.SetCurrentDirectory("ShaderSense");

            //this is the same on both Garrett's and my machine, this is where ShaderSense should be installed    
            string programPath = "\"C:\\Program Files (x86)\\Microsoft Visual Studio 9.0\\Common7\\IDE\\devenv\"";
            string shaderSenseDir = Directory.GetCurrentDirectory();      
            string args = "/ranu /rootsuffix Exp";
            string slnPath = "\""+ shaderSenseDir + "\\ShaderSense.sln\" ";
            string concatedArgs = " /deploy Debug " + slnPath + args;
            string testFileOutput = shaderSenseDir + "\\TestOutput.txt"; 

            string[] filenames = new string[testFiles.Length];
            string[] testFileCheckers = new string[testFiles.Length];
            bool[] testResults = new bool[testFiles.Length];

            //get some of the strings we need for files
            for(int index=0; index<testFiles.Length; index++)
            {                
                filenames[index] = "\"" + shaderSenseDir + "\\" + testFiles[index] + "\"";

                //strip off ".fx" and add "testOutput.fx"
                testFileCheckers[index] = shaderSenseDir + "\\" + testFiles[index].Remove(testFiles[index].LastIndexOf(".fx"), 3) + "Checker.txt";
            }

            //open the new VS2008 instances
            System.Diagnostics.Process tempInstance = System.Diagnostics.Process.Start(programPath, "/run " + slnPath);
            //wait for it to initialize
            System.Threading.Thread.Sleep(SLEEP_TIME*6*1000);
            System.Diagnostics.Process[] procArray = System.Diagnostics.Process.GetProcessesByName("devenv");
            
            if(procArray.Length == 0)
            {
                Assert.Fail();
            }

            //handles to the VS2008 processes
            System.Diagnostics.Process testInstance = procArray[0];
            System.Diagnostics.Process currentInstance = System.Diagnostics.Process.GetCurrentProcess();
            System.Diagnostics.Process helperInstance = null;

            foreach (System.Diagnostics.Process procItr in procArray)
            {
                if (procArray != null && testInstance != null && procItr != null)
                {
                    if (procItr.StartTime.CompareTo(testInstance.StartTime) > 0)
                    {
                        //this is the intermediate window that opens the test instance
                        if(procItr != currentInstance)
                        {                            
                            helperInstance = testInstance;
                        }

                        //this will be the instance that opens the file
                        testInstance = procItr;                        
                    }
                }
            }

            try
            {
                //DTE objects of open VS 2008s
                EnvDTE80.DTE2 testInstanceDTE = SeekDTE2InstanceFromROT("!VisualStudio.DTE.9.0:" + testInstance.Id.ToString());
                EnvDTE80.DTE2 helperInstanceDTE = SeekDTE2InstanceFromROT("!VisualStudio.DTE.9.0:" + helperInstance.Id.ToString());

                //this avoids waiting on VS errors, crucial
                MessageFilter.Register();

                testInstanceDTE.MainWindow.Width = Screen.PrimaryScreen.Bounds.Width;
                testInstanceDTE.MainWindow.Height = Screen.PrimaryScreen.Bounds.Height;

                //open the file to output color
                for (int index = 0; index < testFiles.Length; index++)
                {
                    if (File.Exists(testFileOutput))
                    {
                        File.Delete(testFileOutput);
                    }

                    //open the files
                    testInstanceDTE.ExecuteCommand("File.OpenFile", filenames[index]);

                    while (testInstanceDTE.ActiveDocument == null ||
                           testInstanceDTE.ActiveDocument.Name == null ||
                           testInstanceDTE.ActiveDocument.Name != testFiles[index]) { ; }

                    //wait for doc to open 
                    System.Threading.Thread.Sleep(SLEEP_TIME* 1000);

                    //stored test results
                    if (CompareFiles(testFileOutput, testFileCheckers[index]) == false)
                    {
                        testResults[index] = false;
                    }
                    else
                    {
                        testResults[index] = true;
                    }
                }

                //close the instances, in the reverse order they are opened
                testInstanceDTE.Quit();
                //wait for testIntance to close
                System.Threading.Thread.Sleep(SLEEP_TIME * 1000);
            }

            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return null;
            }

            return testResults;
        }
        
        //test to ensure syntax highlighting works
        [TestMethod()]
        public void ShaderSenseColorTests()
        {
            String[] testFiles = { "ColorTest1.fx", "ColorTest2.fx", "ColorTest3.fx", "ColorTest4.fx",
                                   "ColorTest5.fx", "ColorTest6.fx" };
            
            bool[] testResults = ShaderSenseColorTestFramework(testFiles);

            if (testResults == null)
            {
                Assert.IsTrue(false, "Error in ColorTests");
            }


            for (int index = 0; index < testFiles.Length; index++)
            {                
                Assert.IsTrue(testResults[index], "ColorTest" + (index + 1) + ".fx");
            }  
        }
        
        //test to ensure tooltips work
        [TestMethod()]
        public void ShaderSenseTooltipTest()
        {
            String[] testFiles = { "ColorTest1.fx"};

            bool[] testResults = ShaderSenseColorTestFramework(testFiles);
            
            String testFile = "tooltipsOutput.txt";
            String checkerFile = "tooltipsChecker.txt";
            Assert.IsTrue(CompareFiles(testFile, checkerFile));
        }
    }
}
