using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ApprovalTests.Core;
using ApprovalTests.Reporters;

namespace UnitTestSupport
{
    public class CustomReporter : FirstWorkingReporter
    {
        public static readonly CustomReporter INSTANCE = new CustomReporter();

        public CustomReporter()
            : base(
            //ApproveEverythingReporter.INSTANCE //to be used ONLY to re-approve after a global change such as moving test classes.
                WinMergeReporter.INSTANCE,
                QuietReporter.INSTANCE
            )
        {
        }
    }

    /// <summary>
    /// DANGEROUS! USE THIS ON PURPOSE - NEVER BY ACCIDENT-->
    /// </summary>
    public class ApproveEverythingReporter : IEnvironmentAwareReporter
    {
        public static readonly ApproveEverythingReporter INSTANCE = new ApproveEverythingReporter();
        public void Report(string approved, string received)
        {
            File.Copy(received, approved, true);
        }

        public bool IsWorkingInThisEnvironment(string forFile)
        {
            return true;
        }
    }
}
