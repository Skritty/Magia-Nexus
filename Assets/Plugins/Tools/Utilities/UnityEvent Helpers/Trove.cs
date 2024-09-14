//Created By: Julian Noel
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

namespace Skritty.Tools.Utilities
{
    /// <summary>
    /// Solving event cleanup procedures, once and for all.
    /// Adding listeners via a Trove enables you to automatically clean them up by running Clean().
    /// This differs from RemoveAllListners() in that a Trove can store any event-callback pair (up to 4 arguments) from any mix of events
    /// </summary>
    public class Trove
    {
        /// <summary>
        /// These are cleanup modes specifically used by Trove
        /// </summary>
        public enum CleanupMode { Destroy, Disable, Enable, None }

        private Action cleaningActions;

        public void Clean()
        {
            cleaningActions?.Invoke();
            cleaningActions = null;
        }

        /// <summary>
        /// Automatically cleans the passed-in subtrove when this trove is cleaned up
        /// </summary>
        /// <param name="subTrove"></param>
        /// <returns></returns>
        public Trove Extend(Trove subTrove)
        {
            cleaningActions += subTrove.Clean;
            return subTrove;
        }

        #region Add

        public UnityAction Add(UnityAction callback)
        {
            cleaningActions += () => callback();
            return callback;
        }

        public Action Add(Action callback)
        {
            cleaningActions += callback;
            return callback;
        }


        /// <summary>
        /// Adds the GameObject to the trove. Will either disable, enable, or destroy the given UnityEngine.Object on trove cleanup. Default is destroy.
        /// </summary>
        /// <returns>The value given, for convenience</returns>
        public GameObject Add(GameObject gobj, CleanupMode mode = CleanupMode.Destroy)
        {
            Action callback = null;
            switch(mode)
            {
                case CleanupMode.Destroy:
                    callback = () => UnityEngine.Object.Destroy(gobj);
                    break;
                case CleanupMode.Disable:
                    callback = () => gobj.SetActive(false);
                    break;
                case CleanupMode.Enable:
                    callback = () => gobj.SetActive(true);
                    break;
                default:
                    callback = () => UnityEngine.Object.Destroy(gobj);
                    break;
            }

            cleaningActions += callback;
            return gobj;
        }

        /// <summary>
        /// Adds the Behavior to the trove. Will either disable, enable, or destroy the given UnityEngine.Object on trove cleanup. Default is disable.
        /// </summary>
        /// <returns>The value given, for convenience</returns>
        public Behaviour Add(Behaviour comp, CleanupMode mode = CleanupMode.Disable)
        {
            Action callback = null;
            switch(mode)
            {
                case CleanupMode.Destroy:
                    callback = () => UnityEngine.Object.Destroy(comp);
                    break;
                case CleanupMode.Disable:
                    callback = () => comp.enabled = false;
                    break;
                case CleanupMode.Enable:
                    callback = () => comp.enabled = true;
                    break;
                default:
                    callback = () => UnityEngine.Object.Destroy(comp);
                    break;
            }

            cleaningActions += callback;
            return comp;
        }

        /// <summary>
        /// Fallback for Components. Adds the Component to the trove. Will destroy the given UnityEngine.Object on trove cleanup. For disable/enable options, use the Behavior overload of Add
        /// </summary>
        /// <returns>The value given, for convenience</returns>
        public Component Add(Component comp)
        {
            cleaningActions += () => UnityEngine.Object.Destroy(comp);
            return comp;
        }

        /// <summary>
        /// Final fallback for UnityEngine.Object. Adds the UnityEngine.Object to the trove. Will destroy the given UnityEngine.Object on trove cleanup.
        /// </summary>
        /// <returns>The value given, for convenience</returns>
        public UnityEngine.Object Add(UnityEngine.Object uObj)
        {
            cleaningActions += () => UnityEngine.Object.Destroy(uObj);
            return uObj;
        }

        #endregion Connect "Solo" cleanup Function 

        //I really hope there's a more compact way to do this, but the different forms of generic actions
        //do not appear to have a common ancestor or interface I can use, and UnityEventBase does not
        //expose AddListener or RemoveListener.
        //If anyone has a more elegant solution for this, you are welcome to implement it here!

        #region UnityEvent Overloads

        //These look identical, but since these various forms of UnityEvents/UnityActions don't actually have accessible
        //AddListener or RemoveListener functions, I haven't found a way to consolidate them into a single function.

        public UnityEvent Connect(UnityEvent eventRef, UnityAction callback)
        {
            eventRef.AddListener(callback);
            //ref keyword needed to ensure it correctly adds to null Actions, but ref parameters blow up lambdas and local funcs.
            UnityEvent eventProxy = eventRef;
            ConnectCallbackToTrove(callback, () => eventProxy.RemoveListener(callback));
            return eventRef;
        }

        public UnityEvent<T> Connect<T>(UnityEvent<T> eventRef, UnityAction<T> callback)
        {
            eventRef.AddListener(callback);
            //ref keyword needed to ensure it correctly adds to null Actions, but ref parameters blow up lambdas and local funcs.
            var eventProxy = eventRef;
            ConnectCallbackToTrove(callback, () => eventProxy.RemoveListener(callback));
            return eventRef;
        }

        public UnityEvent<T, U> Connect<T, U>(UnityEvent<T, U> eventRef, UnityAction<T, U> callback)
        {
            eventRef.AddListener(callback);
            //ref keyword needed to ensure it correctly adds to null Actions, but ref parameters blow up lambdas and local funcs.
            var eventProxy = eventRef;
            ConnectCallbackToTrove(callback, () => eventProxy.RemoveListener(callback));
            return eventRef;
        }

        public UnityEvent<T, U, V> Connect<T, U, V>(UnityEvent<T, U, V> eventRef, UnityAction<T, U, V> callback)
        {
            eventRef.AddListener(callback);
            //ref keyword needed to ensure it correctly adds to null Actions, but ref parameters blow up lambdas and local funcs.
            var eventProxy = eventRef;
            ConnectCallbackToTrove(callback, () => eventProxy.RemoveListener(callback));
            return eventRef;
        }

        public UnityEvent<T, U, V, W> Connect<T, U, V, W>(UnityEvent<T, U, V, W> eventRef, UnityAction<T, U, V, W> callback)
        {
            eventRef.AddListener(callback);
            //ref keyword needed to ensure it correctly adds to null Actions, but ref parameters blow up lambdas and local funcs.
            var eventProxy = eventRef;
            ConnectCallbackToTrove(callback, () => eventProxy.RemoveListener(callback));
            return eventRef;
        }

        public UnityEvent Connect(UnityEvent eventRef, params UnityAction[] callbacks)
        {
            foreach(var callback in callbacks)
            {
                Connect(eventRef, callback);
            }
            return eventRef;
        }

        public UnityEvent<T> Connect<T>(UnityEvent<T> eventRef, params UnityAction<T>[] callbacks)
        {
            foreach(var callback in callbacks)
            {
                Connect(eventRef, callback);
            }
            return eventRef;
        }

        public UnityEvent<T, U> Connect<T, U>(UnityEvent<T, U> eventRef, params UnityAction<T, U>[] callbacks)
        {
            foreach(var callback in callbacks)
            {
                Connect(eventRef, callback);
            }
            return eventRef;
        }

        public UnityEvent<T, U, V> Connect<T, U, V>(UnityEvent<T, U, V> eventRef, params UnityAction<T, U, V>[] callbacks)
        {
            foreach(var callback in callbacks)
            {
                Connect(eventRef, callback);
            }
            return eventRef;
        }

        public UnityEvent<T, U, V, W> Connect<T, U, V, W>(UnityEvent<T, U, V, W> eventRef, params UnityAction<T, U, V, W>[] callbacks)
        {
            foreach(var callback in callbacks)
            {
                Connect(eventRef, callback);
            }
            return eventRef;
        }
        #endregion UnityEvent Overloads

        #region C# Event Overloads

        //These look identical, but since these various forms of actions don't actually hasve a common ancestor of any kind,
        //I haven't found a way to consolidate them into a single function. Possibly the one thing that was actually easier to do in Lua...

        /// <summary>
        /// Connects (subscribes) the callbacks to the event
        /// </summary>
        /// <returns>The updated event</returns>
        public void Connect(ref Action eventRef, Action callback)
        {
            eventRef += callback;
            //ref keyword needed to ensure it correctly adds to null Actions, but ref parameters blow up lambdas and local funcs.
            var eventProxy = eventRef;
            ConnectCallbackToTrove(callback,
                () => _ = (eventProxy?.GetInvocationList().Contains(callback) == true) ? (eventProxy -= callback) : null);
        }

        /// <summary>
        /// Doesn't pass the action by ref, so you must assign the returned value back to it to ensure it updates properly from null.
        /// </summary>
        /// <returns>The updated event Action reference.</returns>
        public Action Connect(Action eventRef, Action callback)
        {
            eventRef += callback;
            //ref keyword needed to ensure it correctly adds to null Actions, but ref parameters blow up lambdas and local funcs.
            var eventProxy = eventRef;
            ConnectCallbackToTrove(callback,
                () => _ = (eventProxy?.GetInvocationList().Contains(callback) == true) ? (eventProxy -= callback) : null);

            return eventProxy;
        }

        /// <summary>
        /// Connects (subscribes) the callbacks to the event
        /// </summary>
        public void Connect<T>(ref Action<T> eventRef, Action<T> callback)
        {
            eventRef += callback;
            //ref keyword needed to ensure it correctly adds to null Actions, but ref parameters blow up lambdas and local funcs.
            var eventProxy = eventRef;
            ConnectCallbackToTrove(callback,
                () => _ = (eventProxy?.GetInvocationList().Contains(callback) == true) ? (eventProxy -= callback) : null);
        }


        /// <summary>
        /// Doesn't pass the action by ref, so you must assign the returned value back to it to ensure it updates properly from null.
        /// </summary>
        /// <returns>The updated event Action reference.</returns>
        public Action<T> Connect<T>(Action<T> eventRef, Action<T> callback)
        {
            eventRef += callback;
            //ref keyword needed to ensure it correctly adds to null Actions, but ref parameters blow up lambdas and local funcs.
            var eventProxy = eventRef;
            ConnectCallbackToTrove(callback,
                () => _ = (eventProxy?.GetInvocationList().Contains(callback) == true) ? (eventProxy -= callback) : null);

            return eventProxy;
        }

        /// <summary>
        /// Connects (subscribes) the callbacks to the event
        /// </summary>
        /// <returns>The callbacks provided</returns>
        public void Connect<T, U>(ref Action<T, U> eventRef, Action<T, U> callback)
        {
            eventRef += callback;
            //ref keyword needed to ensure it correctly adds to null Actions, but ref parameters blow up lambdas and local funcs.
            var eventProxy = eventRef;
            ConnectCallbackToTrove(callback,
                () => _ = (eventProxy?.GetInvocationList().Contains(callback) == true) ? (eventProxy -= callback) : null);
        }

        //TODO: Write overloads for the 3 and 4 argument versions of reflexive return Connect() for Actions
        /// <summary>
        /// Doesn't pass the action by ref, so you must assign the returned value back to it to ensure it updates properly from null.
        /// </summary>
        /// <returns>The updated event Action reference.</returns>
        public Action<T, U> Connect<T, U>(Action<T, U> eventRef, Action<T, U> callback)
        {
            eventRef += callback;
            //ref keyword needed to ensure it correctly adds to null Actions, but ref parameters blow up lambdas and local funcs.
            var eventProxy = eventRef;
            ConnectCallbackToTrove(callback,
                () => _ = (eventProxy?.GetInvocationList().Contains(callback) == true) ? (eventProxy -= callback) : null);

            return eventProxy;
        }
        
        /// <summary>
        /// Connects (subscribes) the callbacks to the event
        /// </summary>
        /// <returns>The callbacks provided</returns>
        public void Connect<T, U, V>(ref Action<T, U, V> eventRef, Action<T, U, V> callback)
        {
            eventRef += callback;
            //ref keyword needed to ensure it correctly adds to null Actions, but ref parameters blow up lambdas and local funcs.
            var eventProxy = eventRef;
            ConnectCallbackToTrove(callback,
                () => _ = (eventProxy?.GetInvocationList().Contains(callback) == true) ? (eventProxy -= callback) : null);
        }

        /// <summary>
        /// Connects (subscribes) the callbacks to the event
        /// </summary>
        /// <returns>The callbacks provided</returns>
        public void Connect<T, U, V, W>(ref Action<T, U, V, W> eventRef, Action<T, U, V, W> callback)
        {
            eventRef += callback;
            //ref keyword needed to ensure it correctly adds to null Actions, but ref parameters blow up lambdas and local funcs.
            var eventProxy = eventRef;
            ConnectCallbackToTrove(callback,
                () => _ = (eventProxy?.GetInvocationList().Contains(callback) == true) ? (eventProxy -= callback) : null);
        }

        //-------------------------//

        /// <summary>
        /// Connects (subscribes) all the callbacks to the event
        /// </summary>
        /// <returns>the original event Action</returns>
        public Action Connect(ref Action eventRef, params Action[] callbacks)
        {
            foreach(var callback in callbacks)
            {
                Connect(ref eventRef, callback);
            }
            return eventRef;
        }

        /// <summary>
        /// Connects (subscribes) all the callbacks to the event
        /// </summary>
        /// <returns>the original event Action</returns>
        public Action Connect(Action eventRef, params Action[] callbacks)
        {
            foreach(var callback in callbacks)
            {
                Connect(ref eventRef, callback);
            }
            return eventRef;
        }

        /// <summary>
        /// Connects (subscribes) all the callbacks to the event
        /// </summary>
        /// <returns>the original event Action</returns>
        public Action<T> Connect<T>(ref Action<T> eventRef, params Action<T>[] callbacks)
        {
            foreach(var callback in callbacks)
            {
                Connect(ref eventRef, callback);
            }
            return eventRef;
        }

        /// <summary>
        /// Connects (subscribes) all the callbacks to the event
        /// </summary>
        /// <returns>the original event Action</returns>
        public Action<T> Connect<T>(Action<T> eventRef, params Action<T>[] callbacks)
        {
            foreach(var callback in callbacks)
            {
                Connect(ref eventRef, callback);
            }
            return eventRef;
        }

        /// <summary>
        /// Connects (subscribes) all the callbacks to the event
        /// </summary>
        /// <returns>the original event Action</returns>
        public Action<T, U> Connect<T, U>(ref Action<T, U> eventRef, params Action<T, U>[] callbacks)
        {
            foreach(var callback in callbacks)
            {
                Connect(ref eventRef, callback);
            }
            return eventRef;
        }

        /// <summary>
        /// Connects (subscribes) all the callbacks to the event
        /// </summary>
        /// <returns>the original event Action</returns>
        public Action<T, U> Connect<T, U>(Action<T, U> eventRef, params Action<T, U>[] callbacks)
        {
            foreach(var callback in callbacks)
            {
                Connect(ref eventRef, callback);
            }
            return eventRef;
        }

        /// <summary>
        /// Connects (subscribes) all the callbacks to the event
        /// </summary>
        /// <returns>the original event Action</returns>
        public Action<T, U, V> Connect<T, U, V>(ref Action<T, U, V> eventRef, params Action<T, U, V>[] callbacks)
        {
            foreach(var callback in callbacks)
            {
                Connect(ref eventRef, callback);
            }
            return eventRef;
        }

        /// <summary>
        /// Connects (subscribes) all the callbacks to the event
        /// </summary>
        /// <returns>the original event Action</returns>
        public Action<T, U, V> Connect<T, U, V>(Action<T, U, V> eventRef, params Action<T, U, V>[] callbacks)
        {
            foreach(var callback in callbacks)
            {
                Connect(ref eventRef, callback);
            }
            return eventRef;
        }

        /// <summary>
        /// Connects (subscribes) all the callbacks to the event
        /// </summary>
        /// <returns>the original event Action</returns>
        public Action<T, U, V, W> Connect<T, U, V, W>(ref Action<T, U, V, W> eventRef, params Action<T, U, V, W>[] callbacks)
        {
            foreach(var callback in callbacks)
            {
                Connect(ref eventRef, callback);
            }
            return eventRef;
        }

        /// <summary>
        /// Connects (subscribes) all the callbacks to the event
        /// </summary>
        /// <returns>the original event Action</returns>
        public Action<T, U, V, W> Connect<T, U, V, W>(Action<T, U, V, W> eventRef, params Action<T, U, V, W>[] callbacks)
        {
            foreach(var callback in callbacks)
            {
                Connect(ref eventRef, callback);
            }
            return eventRef;
        }

        #endregion Action Overloads

        private T ConnectCallbackToTrove<T>(T callback, Action cleanupFunction) where T : Delegate
        {
            if(callback != null)
            {
                cleaningActions += () => cleanupFunction?.Invoke();
                return callback;
            }

            return null;
        }
    }
}
