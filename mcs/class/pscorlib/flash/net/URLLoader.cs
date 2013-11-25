// Copyright 2013 Zynga Inc.
//	
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//		
//      Unless required by applicable law or agreed to in writing, software
//      distributed under the License is distributed on an "AS IS" BASIS,
//      WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//      See the License for the specific language governing permissions and
//      limitations under the License.
using System;
using System.Diagnostics;
using System.Net;

namespace flash.net {

	partial class URLLoader {

		// We use a partial C# class to mix C# with PlayScript
		private void AsyncSendAndResponse(Action<HttpWebResponse> responseAction)
		{
			Action wrapperAction = () =>
			{
				// We send the request from another thread (as it can be blocking if there is no Wifi connection)
				WebRequest request = sendRequest();		// Note that we need to make sure sendRequest() and other PlayScript methods are thread safe
				if (request == null)
				{
					return;
				}
				request.BeginGetResponse(new AsyncCallback((iar) =>
				{
					try {
						var response = (HttpWebResponse)((WebRequest)iar.AsyncState).EndGetResponse(iar);
						responseAction(response);
					} catch(System.Net.WebException e)
					{
						Console.WriteLine(e.Message + " Method:"  + request.Method + " RequestUri:" + request.RequestUri);
						throw e;
					}
				}), request);
			};
			wrapperAction.BeginInvoke(new AsyncCallback((iar) =>
			                                            {
				var action = (Action)iar.AsyncState;
				try
				{
					action.EndInvoke(iar);
				}
				catch
				{
					// This can happen if we could not send the message (or get the response)
					// We have to figure out if we want to retry later or show a message box to the end user.
				}
			}), wrapperAction);
		}

		[Conditional("TRACE_ACCURATE_METRICS")]
		private void TraceAccurateMetricsStartLoading()
		{
			_root.trace_fn.trace("@@@ Start loading ", mRequest.url);
			startLoadTime = Stopwatch.GetTimestamp();
		}

		[Conditional("TRACE_ACCURATE_METRICS")]
		private void TraceAccurateMetricsEndLoading()
		{
			_root.trace_fn.trace("@@@ End loading ", mRequest.url);
			endLoadTime = Stopwatch.GetTimestamp();
		}

		[Conditional("TRACE_ACCURATE_METRICS")]
		private void TraceAccurateMetricsStartDispatching()
		{
			startDispatchTime = Stopwatch.GetTimestamp();
		}

		[Conditional("TRACE_ACCURATE_METRICS")]
		private void TraceAccurateMetricsEndDispatching()
		{
			endDispatchTime = Stopwatch.GetTimestamp();

			double ticksPerSecond = (double)Stopwatch.Frequency;

			double loadTimeInSeconds = (double)(endLoadTime - startLoadTime) / ticksPerSecond;
			double dispatchTimeInSeconds = (double)(endDispatchTime - startDispatchTime) / ticksPerSecond;
			_root.trace_fn.trace("@@@ End dispatching ", mRequest.url, " --- load: ", loadTimeInSeconds, " dispatch: ", dispatchTimeInSeconds );
		}
	}

}
