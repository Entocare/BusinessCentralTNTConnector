using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessCentralTNTConnector.TNTConnector.PrintHTML
{
    /// <summary>
    /// To print a HTML document, one first needs to load it into a browser for layout.
    /// After that, a print command can be given. Both processes are asynchronous.
    /// After the first (loading) we can know if the loading succeeded.
    /// The second process, printing, has no feedback mechanism. We need to hold the document in the browser for
    /// a certain amount of time and after that we may assume that the printing either succeeded or failed.
    /// 
    /// This object holds a queue of browser objects, each with a document busy loading or printing.
    /// The object takes care that the queue is kept as short as possible, and that loading problems are
    /// reported back to the caller (but not in a friendly way, so take care that it's not necessary that often!)
    /// </summary>
    public class PrintHTMLQueue : IDisposable
    {
        /// <summary>
        /// How long to wait till we can asume a print task to be ready or failed (seconds)
        /// </summary>
         public static readonly int WaitForPrint = 60;

        /// <summary>
        /// Recognisers for: "Can't reach this page", "Page not reachable", or their Dutch equivalents
        /// Exactly two entries needed
        /// </summary>
        public static readonly string[] errortitlerecogniser = { " bereik", " reach" };

        //The queue
        private Queue<PrintSender> tasks = new Queue<PrintSender>();

        /// <summary>
        /// Add a new printjob and check whether there were loading problems for earlier queued jobs
        /// </summary>
        /// <param name="html">The document we want to print as html source string</param>
        /// <returns>Were there any loading problems detected with earlier queued jobs? Note: loading takes a bit of time! Success or fail is not clear immediately.</returns>
        public bool AddPrintJob(string html)
        {
            bool loadingProblems = CleanUp();

            //add the new job
            PrintSender ps = new PrintSender();
            ps.Send(html, errortitlerecogniser, WaitForPrint);
            tasks.Enqueue(ps);

            return loadingProblems;
        }

        /// <summary>
        /// Get the number of unfinished jobs, plus: whether there were new loading problems detected (not reported earlier).
        /// Note: Ask for the Status e.g. just before closing the application, in order to cancel the shutdown when unfinished jobs exist.
        /// </summary>
        public PrintQueueStatus Status
        {
            get
            {
                bool loadingProblems = CleanUp();
                return new PrintQueueStatus()
                {
                    Count = tasks.Count,
                    LoadingProblems = loadingProblems,
                };
            }
        }

        /// <summary>
        /// Clean up the queue and check for loading problems
        /// </summary>
        /// <returns>Were there any loading problems in documents returned from the queue?</returns>
        private bool CleanUp()
        {
            bool loadingproblems = false; //none detected yet
            while (tasks.Any() && tasks.Peek().disposable)
            {
                PrintSender job = tasks.Dequeue();
                if (job.loadingFailed)
                    loadingproblems = true;
                job.Dispose();
            }
            return loadingproblems;
        }

        /// <summary>
        /// Forced shutdown! We dispose all queue members, ready or not
        /// </summary>
        public void Dispose()
        {
            while (tasks.Any())
            {
                tasks.Dequeue().Dispose();
            }
        }

        public class PrintQueueStatus
        {
            /// <summary>
            /// Size of the print queue
            /// </summary>
            public int Count;

            /// <summary>
            /// Any loading problems in just now dequeued print jobs? (they are not included in the Count)
            /// </summary>
            public bool LoadingProblems;
        }
    }
}
