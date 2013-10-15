using System;

namespace PlayScript.Tooling
{
	public interface ITimeline
	{
		double Time { get; set; }

		int NumTracks { get; }

		ITimelineTrack GetTrackAt (int index);

		ITimelineTrack CreateTrack (string target);

		void DeleteTrack(int index);

	}
}

