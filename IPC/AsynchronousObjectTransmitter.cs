using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Toolbox.IPC
{
	/**
		Stacks up writing request into the ThreadPool and waits 
		some time for their completion if the the end point is Disposed.
	**/

	sealed class AsynchronousObjectTransmitter : IAsynchronousObjectTransmitter
	{
		readonly IObjectTransmitter _transmitter;
		readonly Queue<Action> _queue = new Queue<Action>();
		readonly object _ = new object();
		Exception _error;

		public AsynchronousObjectTransmitter(IObjectTransmitter transmitter)
		{
			_transmitter = transmitter;
		}

		public void Dispose()
		{
			waitToEmptyQueue(2500);
		}

		public void send(Type type, object obj)
		{
			lock (_)
			{
				if (_error != null)
					throw _error;

				_queue.Enqueue(() => _transmitter.send(type, obj));
				if (_queue.Count == 1)
					triggerSending();
			}

		}

		void triggerSending()
		{
			ThreadPool.QueueUserWorkItem(asyncSendAll);
		}

		void waitToEmptyQueue(uint timeoutMS)
		{
			lock (_)
			{
				if (_error != null)
				{
					Debug.Assert(_queue.Count == 0);
					logIgnoredErrorOnDestruction(_error);
					return;
				}

				if (_queue.Count == 0)
					return;

				if (!Monitor.Wait(_, timeoutMS.signed()))
				{
					this.W("Write blocked on destruction, closing handle anyway.");
					return;
				}

				Debug.Assert(_queue.Count == 0);

				if (_error != null)
					logIgnoredErrorOnDestruction(_error);
			}
		}

		void logIgnoredErrorOnDestruction(Exception error)
		{
			this.W("Ignored error on ObjectEndPoint Dispose:");
			this.E(error);
		}

		// this function only ends when there are no more items in the queue.
	
		void asyncSendAll(object n)
		{
			Action currentItem = null;

			while (true)
			{
				lock (_)
				{
					if (currentItem != null)
						_queue.Dequeue();

					if (_queue.Count == 0)
					{
						Monitor.Pulse(_);
						return;
					}

					currentItem = _queue.Peek();
				}
				try
				{
					currentItem();
				}
				catch (Exception e)
				{
					lock (_)
					{
						_queue.Clear();
						_error = e;
						Monitor.Pulse(_);
					}
					return;
				}
			}
		}
	}
}
