using System;

namespace PlayScript.Tooling
{
	public interface ITimelineTrackKey
	{
		double Time { get; set; }

		void GetValue (ref Value value);

		void SetValue (ref Value value);
	}
}

