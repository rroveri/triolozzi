using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace WindowsGame2
{
    /// <summary>
    /// <para>This class wrappes the system thread class.</para>
    /// <para>It provides the function to start a loop which calls the 'threadFunction'.</para>
    /// <para>It also provides a stop function to quit this loop.</para>
    /// <para>Override the 'threadFunction' to execute iteratively own code.</para>
    /// </summary>
    public abstract class Thread
    {
        /// <summary>
        /// <para>The system thread class.</para>
        /// </summary>
        private System.Threading.Thread _thread;


        /// <summary>
        /// <para>This bool indicates whether the thread is currently running or not.
        /// </summary>
        private bool _isRunning;


        /// <summary>
        /// <para>This bool indicates whether the thread has finished its execution or not.</para>
        /// <para>It is used by the 'stop' function to determine when to return.</para>
        /// </summary>
        private bool _isFinished;


        /// <summary>
        /// <para>This integer indicates on which hardware thread slot the thread should run.</para>
        /// <para>Valid values are 1,3,4 and 5.</para>
        /// </summary>
        private int _processorAffinity;


        /// <summary>
        /// <para>This field indicates, whether the thread is running or not.</para>
        /// </summary>
        public bool isRunning
        {
            //return whether the thread is running or not
            get
            {
                //return the value
                return _isRunning;
            }
        }


        /// <summary>
        /// <para>Property access to the variable which determines on which hardware thread slot the thread is executed.</para>
        /// <para>Possible results are 1,3,4 or 5.</para>
        /// </summary>
        public int hardwareThreadSlot
        {
            //get the value
            get
            {
                //return the result
                return _processorAffinity;
            }
        }


        /// <summary>
        /// <para>This function iterates as long as this thread is not stopped.</para>
        /// <para>It calls 'threadFunction' once in each iteration.</para>
        /// </summary>
        private void threadLoop()
        {
            //set the hardware thread slot on which the thread should run
            #if XBOX || XBOX360
                System.Threading.Thread.CurrentThread.SetProcessorAffinity(_processorAffinity);
            #endif

            //loop as long as running
            while (_isRunning)
            {
                //call the code of the descendant
                threadFunction();
            }

            //set is finished to true
            _isFinished = true;
        }


        /// <summary>
        /// <para>This function is called once in each iteration of the thread.</para>
        /// <para>Overide it to execute own code.</para>
        /// </summary>
        protected abstract void threadFunction();


        /// <summary>
        /// <para>This function starts the thread if it is not allready running.</para>
        /// </summary>
        /// <param name="hardwareThreadSlot"><para>The hardware thread slot to be used.</para><para>Valid values are 1,3,4 and 5.</para></param>
        public void start(int hardwareThreadSlot)
        {
            //check for a valid slot
            Debug.Assert(   hardwareThreadSlot == 1 || 
                            hardwareThreadSlot == 3 || 
                            hardwareThreadSlot == 4 || 
                            hardwareThreadSlot == 5);

            //return if allready running
            if (_isRunning)
            {
                return;
            }

            //set the hardware slot
            _processorAffinity = hardwareThreadSlot;

            //the thread is running...
            _isRunning = true;

            //...and has not finished yet
            _isFinished = false;

            //create the thread
            _thread = new System.Threading.Thread(new System.Threading.ThreadStart(threadLoop));

            //start the thread
            _thread.Start();
        }


        /// <summary>
        /// <para>This function stops the thread if it is running.
        /// </summary>
        public void stop()
        {
            //test if there is a thread running
            if (_thread == null)
            {
                return;
            }

            //set _isRunning to false
            _isRunning = false;

            //wait until the thread has finished
            while (!_isFinished) { }

            //abort the thread
            _thread.Abort();

            //set it to null
            _thread = null;
        }


        /// <summary>
        /// <para>The class constructor.</para>
        /// </summary>
        public Thread()
        {
            //set the thread flags
            _isRunning = false;
            _isFinished = true;

            //set the processor affinity
            _processorAffinity = 1;
        }


        /// <summary>
        /// <para>The class destructor.</para>
        /// </summary>
        ~Thread()
        {
            //stop the thread
            stop();
        }
    }
}
