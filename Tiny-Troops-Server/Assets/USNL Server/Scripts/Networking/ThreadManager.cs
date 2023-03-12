using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace USNL.Package {
    public class ThreadManager : MonoBehaviour {
        #region Main Thread

        private static readonly List<Action> executeOnMainThread = new List<Action>();
        private static readonly List<Action> executeCopiedOnMainThread = new List<Action>();
        private static bool actionToExecuteOnMainThread = false;

        private void Update() {
            UpdateMain();
        }

        /// <summary>Sets an action to be executed on the main thread.</summary>
        /// <param name="_action">The action to be executed on the main thread.</param>
        public static void ExecuteOnMainThread(Action _action) {
            if (_action == null) {
                Debug.Log("No action to execute on main thread!");
                return;
            }

            lock (executeOnMainThread) {
                executeOnMainThread.Add(_action);
                actionToExecuteOnMainThread = true;
            }
        }

        /// <summary>Executes all code meant to run on the main thread. NOTE: Call this ONLY from the main thread.</summary>
        public static void UpdateMain() {
            if (actionToExecuteOnMainThread) {
                executeCopiedOnMainThread.Clear();
                lock (executeOnMainThread) {
                    executeCopiedOnMainThread.AddRange(executeOnMainThread);
                    executeOnMainThread.Clear();
                    actionToExecuteOnMainThread = false;
                }

                for (int i = 0; i < executeCopiedOnMainThread.Count; i++) {
                    executeCopiedOnMainThread[i]();
                }
            }
        }

        #endregion

        #region Packet Handle Thread

        private static Thread packetHandleThread;
        private static AutoResetEvent packetHandleAutoResetEvent;
        private static readonly List<Action> executeOnPacketHandleThread = new List<Action>();
        private static readonly List<Action> executeOnPacketHandleThreadCopied = new List<Action>();
        private static bool actionToExecuteOnPacketHandleThread = false;

        public static void StartPacketHandleThread() {
            // Create new AutoResetEvent that is not updating by default
            packetHandleAutoResetEvent = new AutoResetEvent(false);

            // Create and Start new Thread
            packetHandleThread = new Thread(PacketHandleThreadFunction);
            packetHandleThread.IsBackground = true;
            packetHandleThread.Priority = System.Threading.ThreadPriority.AboveNormal;

            packetHandleThread.Start();
        }

        public static void StopPacketHandleThread() {
            packetHandleAutoResetEvent.Close();
            packetHandleThread.Abort();
        }

        private static void PacketHandleThreadFunction() {
            while (true) {
                // Pause thread
                packetHandleAutoResetEvent.WaitOne();

                // If there is an action waiting to be executed on this thread
                if (actionToExecuteOnPacketHandleThread) {
                    // Clear copied actions list
                    executeOnPacketHandleThreadCopied.Clear();

                    // Copy actions into copied actions list
                    lock (executeOnPacketHandleThread) {
                        executeOnPacketHandleThreadCopied.AddRange(executeOnPacketHandleThread);
                        executeOnPacketHandleThread.Clear();
                        actionToExecuteOnPacketHandleThread = false;
                    }

                    // Loop through copied actions and execute
                    for (int i = 0; i < executeOnPacketHandleThreadCopied.Count; i++) {
                        executeOnPacketHandleThreadCopied[i]();
                    }
                }
            }
        }

        public static void ExecuteOnPacketHandleThread(Action _action) {
            // Add action to list of actions to execute on Packet Handle Thread

            lock (executeOnPacketHandleThread) {
                executeOnPacketHandleThread.Add(_action);
                actionToExecuteOnPacketHandleThread = true;

                // Unpause Packet Handle Thread
                packetHandleAutoResetEvent.Set();
            }
        }

        #endregion
    }
}