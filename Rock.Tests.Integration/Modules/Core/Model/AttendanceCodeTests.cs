﻿// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.Linq;
using Rock;
using Rock.Data;
using Rock.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.Entity;
using Rock.Tests.Shared;

namespace Rock.Tests.Integration.Model
{
    /// <summary>
    /// Used for testing anything regarding AttendanceCode.
    /// </summary>
    [TestClass]
    public class AttendanceCodeTests
    {
        #region Setup

        /// <summary>
        /// Runs after each test in this class is executed.
        /// Deletes the test data added to the database for each tests.
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
            using ( var rockContext = new RockContext() )
            {
                var acService = new AttendanceCodeService( rockContext );
                var attendanceService = new AttendanceService( rockContext );

                DateTime today = RockDateTime.Today;
                DateTime tomorrow = today.AddDays( 1 );
                var todaysCodes = acService.Queryable()
                        .Where( c => c.IssueDateTime >= today && c.IssueDateTime < tomorrow )
                        .ToList();
                if ( todaysCodes.Any() )
                {
                    var ids = todaysCodes.Select( c => c.Id ).ToList();

                    // get the corresponding attendance records and delete them first.
                    var todayTestAttendance = attendanceService.Queryable().Where( a => ids.Contains( a.AttendanceCodeId.Value ) );
                    if ( todayTestAttendance.Any() )
                    {
                        attendanceService.DeleteRange( todayTestAttendance );
                    }

                    acService.DeleteRange( todaysCodes );
                    rockContext.SaveChanges();
                }
            }

            AttendanceCodeService.FlushTodaysCodes();
        }

        #endregion

        #region Alpha-numeric codes

        /// <summary>
        /// Runs the test three times.
        /// </summary>
        [Ignore("Sometimes with caching you have to throw out the first result.")]
        [TestMethod]
        public void RunTestThreeTimes()
        {
            for ( int i = 0; i < 3; i++ )
            {
                GenerateLotsOfCodes();
            }
        }

        /// <summary>
        /// Generates lots of codes to test performance.
        /// </summary>
        [Ignore("This is only for local testing.")]
        [TestMethod]
        public void GenerateLotsOfCodes()
        {
            int interations = 6000;
            int alphaNumbericDigits = 0;
            int alphaDigits = 0;
            int numericDigits = 4;
            bool isRandom = false;

            var stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();

            for ( int i = 0; i < interations; i++ )
            {
                AttendanceCodeService.GetNew( alphaNumbericDigits, alphaDigits, numericDigits, isRandom );
            }

            stopWatch.Stop();
            System.Diagnostics.Trace.Listeners.Add(new System.Diagnostics.TextWriterTraceListener(Console.Out));
            System.Diagnostics.Trace.WriteLine( $"New GetNew Method AlphaNumericDigits: {alphaNumbericDigits}, AlphaDigits: {alphaDigits}, NumericDigits: {numericDigits}, IsRandom: {isRandom}, Number of Codes Generated: {interations}. Total Time: {stopWatch.ElapsedMilliseconds} ms." );
            ClearAttendanceService();
            AttendanceCodeService.FlushTodaysCodes();
        }

        private void ClearAttendanceService()
        {
            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.ExecuteSqlCommand( "delete from AttendanceCode" );
            }
        }

        /// <summary>
        /// Verify that three character alpha-numeric codes are all good codes.
        /// </summary>
        [TestMethod]
        public void AlphaNumericCodesShouldSkipBadCodes()
        {
            var stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();

            var codeList = new List<string>();
            AttendanceCode code = null;
            for ( int i = 0; i < 6000; i++ )
            {
                code = AttendanceCodeService.GetNew( 3, 0, 0, false );
                codeList.Add( code.Code );
            }

            bool hasMatchIsBad = codeList.Where( c => AttendanceCodeService.NoGood.Any( ng => c.Contains( ng ) ) ).Any();
            Assert.That.IsFalse( hasMatchIsBad );

            stopWatch.Stop();
            System.Diagnostics.Trace.Listeners.Add(new System.Diagnostics.TextWriterTraceListener(Console.Out));
            System.Diagnostics.Trace.WriteLine( string.Format( "Test AlphaNumericCodesShouldSkipBadCodes took {0} ms.", stopWatch.ElapsedMilliseconds ) );
        }

        #endregion

        #region Numeric only codes

        /// <summary>
        /// Checks the three char "002" code.
        /// </summary>
        [TestMethod]
        public void CheckThreeChar002Code()
        {
            var stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();

            AttendanceCode code = null;
            for ( int i = 0; i < 2; i++ )
            {
                code = AttendanceCodeService.GetNew( 0, 0, 3, false );
            }

            Assert.That.AreEqual( "002", code.Code );

            stopWatch.Stop();
            System.Diagnostics.Trace.Listeners.Add(new System.Diagnostics.TextWriterTraceListener(Console.Out));
            System.Diagnostics.Trace.WriteLine( string.Format( "Test CheckThreeChar002Code took {0} ms.", stopWatch.ElapsedMilliseconds ) );
        }

        /// <summary>
        /// NOTE: This test currently fails in Rock v8 and earlier.
        /// </summary>
        [TestMethod]
        public void NumericCodesShouldNotContain911And666()
        {
            var stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();

            var codeList = new List<string>();
            AttendanceCode code = null;
            for ( int i = 0; i < 2000; i++ )
            {
                code = AttendanceCodeService.GetNew( 0, 0, 4, false );
                codeList.Add( code.Code );
            }

            Assert.That.IsFalse( codeList.Any( s => s.Contains( "911" ) ) );
            Assert.That.IsFalse( codeList.Any( s => s.Contains( "666" ) ) );

            stopWatch.Stop();
            System.Diagnostics.Trace.Listeners.Add(new System.Diagnostics.TextWriterTraceListener(Console.Out));
            System.Diagnostics.Trace.WriteLine( string.Format( "Test NumericCodesShouldNotContain911And666 took {0} ms.", stopWatch.ElapsedMilliseconds ) );
        }

        /// <summary>
        /// Numeric only code with length of 2 should not go beyond 99.
        /// Attempting to create one should not be allowed so throwing a timeout
        /// exception is acceptable to let the administrator know there is a
        /// configuration problem.
        /// </summary>
        [TestMethod]
        public void NumericCodeWithLengthOf2ShouldNotGoBeyond99()
        {
            var stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();

            try
            {
                var codeList = new List<string>();
                AttendanceCode code = null;
                for ( int i = 0; i < 101; i++ )
                {
                    code = AttendanceCodeService.GetNew( 0, 0, 2, false );
                    codeList.Add( code.Code );
                }

                // should not be longer than 2 characters
                // This is a known bug in v7.4 and earlier, and possibly fixed via PR #3071
                Assert.That.IsTrue( codeList.Last().Length == 2, "last code was " + codeList.Last().Length + " characters long." );
            }
            catch ( Exception )
            {
                // An exception in this case is considered better than hanging (since there is no actual solution).
                Assert.That.IsTrue( true );
            }
            finally
            {
                stopWatch.Stop();
                System.Diagnostics.Trace.Listeners.Add(new System.Diagnostics.TextWriterTraceListener(Console.Out));
                System.Diagnostics.Trace.WriteLine( string.Format( "Test NumericCodeWithLengthOf2ShouldNotGoBeyond99 took {0} ms.", stopWatch.ElapsedMilliseconds ) );
            }
        }

        /// <summary>
        /// Numerics codes should not repeat. There are 998 possible good numeric three character codes.
        /// </summary>
        [TestMethod]
        public void NumericCodesShouldNotRepeat()
        {
            var stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();

            var codeList = new List<string>();
            AttendanceCode code = null;
            for ( int i = 0; i < 997; i++ )
            {
                code = AttendanceCodeService.GetNew( 0, 0, 3, false );
                codeList.Add( code.Code );
            }

            var duplicates = codeList.GroupBy( x => x ).Where( group => group.Count() > 1 ).Select( group => group.Key );
            Assert.That.IsTrue( duplicates.Count() == 0, "repeated codes: " + string.Join( ", ", duplicates ) );

            stopWatch.Stop();
            System.Diagnostics.Trace.Listeners.Add(new System.Diagnostics.TextWriterTraceListener(Console.Out));
            System.Diagnostics.Trace.WriteLine( string.Format( "Test NumericCodesShouldNotRepeat took {0} ms.", stopWatch.ElapsedMilliseconds ) );
        }

        /// <summary>
        /// Random numeric codes should not repeat. There are 998 possible good numeric three character codes.
        /// </summary>
        [TestMethod]
        public void RandomNumericCodesShouldNotRepeat()
        {
            var stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();

            var codeList = new List<string>();
            AttendanceCode code = null;
            for ( int i = 0; i < 998; i++ )
            {
                code = AttendanceCodeService.GetNew( 0, 0, 3, true );
                codeList.Add( code.Code );
            }

            var duplicates = codeList.GroupBy( x => x )
                                    .Where( group => group.Count() > 1 )
                                    .Select( group => group.Key );

            Assert.That.IsTrue( duplicates.Count() == 0, "repeated codes: " + string.Join(", ", duplicates ) );

            stopWatch.Stop();
            System.Diagnostics.Trace.Listeners.Add(new System.Diagnostics.TextWriterTraceListener(Console.Out));
            System.Diagnostics.Trace.WriteLine( string.Format( "Test RandomNumericCodesShouldNotRepeat took {0} ms.", stopWatch.ElapsedMilliseconds ) );
        }

        /// <summary>
        /// Requestings the more codes than are possible should throw exception...
        /// because there's really nothing else we could do in that situation, right?
        /// 
        /// NOTE: This test has a special setup using an async task so that we can break
        /// out if the underlying Rock service call is hung in an infinite loop.
        /// </summary>
        [TestMethod]
        public void RequestingMoreCodesThanPossibleShouldThrowException()
        {
            var stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();

            var codeList = new List<string>();
            AttendanceCode code = null;

            // Generate 99 codes (the maximum number of valid codes).
            for ( int i = 0; i < 100; i++ )
            {
                code = AttendanceCodeService.GetNew( 0, 0, 2, true );
                codeList.Add( code.Code );
            }

            // Now try to generate one more... which should NOT hang but instead, may
            // throw one of two exceptions.
            try
            {
                code = AttendanceCodeService.GetNew( 0, 0, 2, true );
            }
            catch ( InvalidOperationException )
            {
                Assert.That.IsTrue( true );
            }
            catch ( TimeoutException )
            {
                // An exception in this case is considered better than hanging (since there is 
                // no actual solution).
                Assert.That.IsTrue( true );
            }
            finally
            {
                stopWatch.Stop();
                System.Diagnostics.Trace.Listeners.Add(new System.Diagnostics.TextWriterTraceListener(Console.Out));
                System.Diagnostics.Trace.WriteLine( string.Format( "Test RequestingMoreCodesThanPossibleShouldThrowException took {0} ms.", stopWatch.ElapsedMilliseconds ) );
            }
        }

        /// <summary>
        /// Sequentially increment three-character numeric codes to 100 and verify "100".
        /// </summary>
        [TestMethod]
        public void Increment100SequentialNumericCodes()
        {
            var stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();

            AttendanceCode code = null;
            for ( int i = 0; i < 100; i++ )
            {
                code = AttendanceCodeService.GetNew( 0, 0, 3, false );
            }

            Assert.That.AreEqual( "100", code.Code );

            stopWatch.Stop();
            System.Diagnostics.Trace.Listeners.Add(new System.Diagnostics.TextWriterTraceListener(Console.Out));
            System.Diagnostics.Trace.WriteLine( string.Format( "Test Increment100SequentialNumericCodes took {0} ms.", stopWatch.ElapsedMilliseconds ) );
        }

        #endregion

        #region Alpha only codes

        /// <summary>
        /// Three character alpha only codes should not 'contain' any bad codes.
        /// </summary>
        [TestMethod]
        public void AlphaOnlyCodesShouldSkipBadCodes()
        {
            var codeList = new List<string>();
            AttendanceCode code = null;
            for ( int i = 0; i < 1000; i++ )
            {
                code = AttendanceCodeService.GetNew( 0, 3, 0, true );
                codeList.Add( code.Code );
            }

            bool hasMatchIsBad = codeList.Where( c => AttendanceCodeService.NoGood.Any( ng => c.Contains( ng ) ) ).Any();

            Assert.That.IsFalse( hasMatchIsBad );
        }

        /// <summary>
        /// Alpha codes should not repeat.  For three character codes there are approximately
        ///  4847 (17*17*17 minus ~50 bad codes) possible combinations of the 17 letters
        /// </summary>
        [TestMethod]
        public void AlphaOnlyCodesShouldNotRepeat()
        {
            var stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();

            var codeList = new List<string>();
            AttendanceCode code = null;

            // 4847 (17*17*17 minus ~50 bad codes) possible combinations of 17 letters
            for ( int i = 0; i < 4860; i++ )
            {
                ////System.Diagnostics.Debug.WriteIf( i > 4700, "code number " + i + " took... " );
                code = AttendanceCodeService.GetNew( 0, 3, 0, false );
                codeList.Add( code.Code );
                ////System.Diagnostics.Debug.WriteLineIf( i > 4700, "" );
            }

            var duplicates = codeList.GroupBy( x => x )
                                    .Where( group => group.Count() > 1 )
                                    .Select( group => group.Key );

            Assert.That.IsTrue( duplicates.Count() == 0 );

            stopWatch.Stop();
            System.Diagnostics.Trace.Listeners.Add(new System.Diagnostics.TextWriterTraceListener(Console.Out));
            System.Diagnostics.Trace.WriteLine( string.Format( "Test AlphaOnlyCodesShouldNotRepeat took {0} ms.", stopWatch.ElapsedMilliseconds ) );
        }

        #endregion

        #region Alpha-numeric + numeric only codes

        /// <summary>
        /// Two character alpha numeric codes (AttendanceCodeService.codeCharacters) has possible
        /// 24*24 (576) combinations plus two character numeric codes has a possible 10*10 (100)
        /// for a total set of 676 combinations.  Removing the noGood (~60) codes leaves us with
        /// a valid set of about 616 codes.
        /// 
        /// NOTE: This appears to be a possible bug in v8.0 and earlier. The AttendanceCodeService
        /// service will only generate 100 codes when trying to combine the numeric parameter of "2" with
        /// the other parameters.
        ///
        /// Even when run with 2 alpha numeric and 3 numeric, this test should verify that codes
        /// such as X6662, 99119, 66600 do not occur.
        /// 
        /// There should be no bad codes in the generated codeList -- even though
        /// individually each part has no bad codes.  For example, "A6" + "66" should
        /// not appear since combined it would be "A666".
        /// </summary>
        [TestMethod]
        public void AlphaNumericWithNumericCodesShouldSkipBadCodes()
        {
            int attemptCombination = 0;
            var stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();

            try
            {
                var codeList = new List<string>();
                AttendanceCode code = null;
                for ( int i = 0; i < 600; i++ )
                {
                    attemptCombination = i;
                    code = AttendanceCodeService.GetNew( 2, 0, 3, true );
                    codeList.Add( code.Code );
                }

                var matches = codeList.Where( c => AttendanceCodeService.NoGood.Any( ng => c.Contains( ng ) ) );
                bool hasMatchIsBad = matches.Any();

                Assert.That.IsFalse( hasMatchIsBad, "bad codes were: " + string.Join( ", ", matches ) );
            }
            catch ( TimeoutException )
            {
                // If an infinite loop was detected, but we tried at least 600 codes then
                // we'll consider this a pass.
                Assert.That.IsTrue( attemptCombination >= 600 );
            }
            finally
            {
                stopWatch.Stop();
                System.Diagnostics.Trace.Listeners.Add(new System.Diagnostics.TextWriterTraceListener(Console.Out));
                System.Diagnostics.Trace.WriteLine( string.Format( "Test AlphaOnlyWithNumericOnlyCodesShouldSkipBadCodes took {0} ms.", stopWatch.ElapsedMilliseconds ) );
            }
        }

        #endregion

        #region Alpha only + numeric only codes

        /// <summary>
        /// This is the configuration that churches like Central Christian Church use for their
        /// Children's check-in.
        /// </summary>
        [TestMethod]
        public void TwoAlphaWithFourRandomNumericCodesShouldSkipBadCodes()
        {
            var stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();

            int attemptCombination = 0;

            var codeList = new List<string>();
            AttendanceCode code = null;
            for ( int i = 0; i < 2500; i++ )
            {
                attemptCombination = i;
                code = AttendanceCodeService.GetNew( 0, 2, 4, true );
                codeList.Add( code.Code );
            }

            var matches = codeList.Where( c => AttendanceCodeService.NoGood.Any( ng => c.Contains( ng ) ) );
            bool hasMatchIsBad = matches.Any();
            Assert.That.IsFalse( hasMatchIsBad, "bad codes were: " + string.Join( ", ", matches ) );

            stopWatch.Stop();
            System.Diagnostics.Trace.Listeners.Add(new System.Diagnostics.TextWriterTraceListener(Console.Out));
            System.Diagnostics.Trace.WriteLine( string.Format( "Test AlphaOnlyWithNumericOnlyCodesShouldSkipBadCodes took {0} ms.", stopWatch.ElapsedMilliseconds ) );
        }

        /// <summary>
        /// Codes containing parts combined into noGood codes, such as "P" + "55", should not occur.
        /// Ensure sequential numbers are also validated at the correct time so the next number is generated.
        /// </summary>
        [TestMethod]
        public void TwoAlphaWithFourSequentialNumericCodesShouldSkipBadCodes()
        {
            var stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();

            var codeList = new List<string>();
            AttendanceCode code = null;
            for ( int i = 0; i < 6000; i++ )
            {
                code = AttendanceCodeService.GetNew( 0, 2, 4, false );
                codeList.Add( code.Code );
            }

            var matches = codeList.Where( c => AttendanceCodeService.NoGood.Any( ng => c.Contains( ng ) ) );
            bool hasMatchIsBad = matches.Any();
            Assert.That.IsFalse( hasMatchIsBad, "bad codes were: " + string.Join(", ", matches ) );

            stopWatch.Stop();
            System.Diagnostics.Trace.Listeners.Add(new System.Diagnostics.TextWriterTraceListener(Console.Out));
            System.Diagnostics.Trace.WriteLine( string.Format( "Test AlphaOnlyWithNumericOnlyCodesShouldSkipBadCodes took {0} ms.", stopWatch.ElapsedMilliseconds ) );
        }

        /// <summary>
        /// Codes containing parts combined into noGood codes, such as "P" + "55",
        /// should not occur.
        /// </summary>
        [TestMethod]
        public void AlphaOnlyWithNumericOnlyCodesShouldSkipBadCodes()
        {
            var stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();

            var codeList = new List<string>();
            AttendanceCode code = null;
            for ( int i = 0; i < 6000; i++ )
            {
                code = AttendanceCodeService.GetNew( 0, 1, 4, true );
                codeList.Add( code.Code );
            }

            var matches = codeList.Where( c => AttendanceCodeService.NoGood.Any( ng => c.Contains( ng ) ) );
            bool hasMatchIsBad = matches.Any();
            Assert.That.IsFalse( hasMatchIsBad, "bad codes were: " + string.Join(", ", matches ) );

            stopWatch.Stop();
            System.Diagnostics.Trace.Listeners.Add(new System.Diagnostics.TextWriterTraceListener(Console.Out));
            System.Diagnostics.Trace.WriteLine( string.Format( "Test AlphaOnlyWithNumericOnlyCodesShouldSkipBadCodes took {0} ms.", stopWatch.ElapsedMilliseconds ) );
        }
        
        #endregion
    }
}
