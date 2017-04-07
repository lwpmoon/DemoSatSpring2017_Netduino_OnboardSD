using System.Collections;
using System.Threading;
using DemoSatSpring2017Netduino_OnboardSD.Flight_Computer;
using Microsoft.SPOT;

namespace DemoSatSpring2017Netduino_OnboardSD.Work_Items
{
    
    public class MemoryMonitor 
    {
        private static MemoryMonitor _instance;
        public static MemoryMonitor Instance => _instance ?? (_instance = new MemoryMonitor());

        private readonly WorkItem _workItem;
        private readonly WorkItem _workItem2;
        private readonly ArrayList _pauseableWorkItems = new ArrayList();
        private Logger _logger;
        private int _preLaunchCount;
        private uint _freeMem;

        private MemoryMonitor(int preLaunchPauseCount = 25)
        {
            var unused = new byte[] {};
            _workItem = new WorkItem(MonitorMemory, loggable:false, persistent:true, pauseable:false );
            _workItem2 = new WorkItem(MemoryStatus, loggable:false, persistent:true, pauseable:false );
            _preLaunchCount = preLaunchPauseCount;
        }

        private void MemoryStatus()
        {

            Thread.Sleep(10000);
            Rebug.Print("[MEMORY] Freemem: " + Debug.GC(true));
        }

        private void MonitorMemory() {

            if (Debug.GC(true) > 60000) return;

            Rebug.Print("[WARNING] RAM critically low... pausing actions.  Freemem: " + Debug.GC(true) + "  TimeStamp: " + Clock.Instance.ElapsedMilliseconds);

            foreach (WorkItem action in _pauseableWorkItems) action.Stop();
            //var currentCount = _logger.PendingItems;
            while (_logger.PendingItems > 0)
            {
                
            }

            Rebug.Print("[WARNING] Resuming paused actions... Current FreeMem: " + Debug.GC(false) + "  TimeStamp: " + Clock.Instance.ElapsedMilliseconds);

            foreach (WorkItem action in _pauseableWorkItems) action.Start();

        }

        public void RegisterPauseableAction(WorkItem actionToRegister) {
            _pauseableWorkItems.Add(actionToRegister);
        }

        public void Start(ref Logger logger)
        {
            _logger = logger;
            _workItem.Start();
            Rebug.Print("[SUCCESS] Memory monitor started.");
            _workItem2.Start();
            Rebug.Print("[SUCCESS] Memory status update started.");
        }
    }
}