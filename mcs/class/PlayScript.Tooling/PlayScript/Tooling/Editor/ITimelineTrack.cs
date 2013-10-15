using System;

namespace PlayScript.Tooling
{
	public interface ITimelineTrack
	{
		int NumKeys { get; }

		ITimelineTrackKey GetKeyAt(int index);

		void DeleteKey(int index);

		ITimelineTrackKey CreateKey(double time, object value);

	}
}

