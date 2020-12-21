using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace BusinessCentralTNTConnector.TNTConnector.PrintHTML
{
    /// <summary>
    /// Send one HTML doc to the windows default printer
    /// Holds the doc in a WebBrowser control, for long enough time, while "events" are done
    /// </summary>
    public class PrintSender : IDisposable
    {
        private string[] errortitlerecogniser;
        private int waitTime;
        private bool printingStarted;
        private WebBrowser brws1;
        private DateTime loadingStartTime;

        /// <summary>
        /// Did the loading fail? e.g. HTML errors (available when loading is ready)
        /// The loading takes some time. After the load time (< waitTime) this value may turn to true
        /// </summary>
        public bool loadingFailed { get; private set; }

        /// <summary>
        /// Can we dispose this object without risking to stop the printing?
        /// </summary>
        public bool disposable
        {
            get
            {
                return loadingFailed || (DateTime.Now - loadingStartTime > new TimeSpan(0, 0, waitTime));
            }
        }

        /// <summary>
        /// Send one HTML doc to the windows default printer
        /// </summary>
        /// <param name="html">html source string of the document to print</param>
        /// <param name="errortitlerecogniser">2 strings that signal an error when occuring in the page title</param>
        /// <param name="waitTime">Time to wait until printing can be assumed ready (seconds)</param>
        public void Send(string html, string[] errortitlerecogniser, int waitTime)
        {
            this.errortitlerecogniser = errortitlerecogniser;
            this.waitTime = waitTime;
            this.printingStarted = false;
            this.loadingFailed = false;   //no problem yet
            this.brws1 = new WebBrowser();
            brws1.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(LoadingReady);
            brws1.DocumentText = html;
            this.loadingStartTime = DateTime.Now;
        }

        private void LoadingReady(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            //multiple ready events will occur if the doc contains images or scripts - but we take care to do it only once
            if (!printingStarted && brws1.ReadyState == WebBrowserReadyState.Complete)  
            {
                if (
                     
                    !brws1.DocumentTitle.Contains(errortitlerecogniser[0])
                    && !brws1.DocumentTitle.Contains(errortitlerecogniser[1])
                )
                {
                    brws1.Print();
                    printingStarted = true;
                }
                else
                {
                    loadingFailed = true;
                }
            }
        }

        public void Dispose()
        {
            brws1.Dispose();
        }
    }
}
