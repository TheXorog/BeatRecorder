namespace BeatRecorder;

internal static class Extensions
{
    /// <summary>
    /// Used for authentication with OBS Websocket if authentication is required ("Borrowed" from https://github.com/BarRaider/obs-websocket-dotnet/blob/268b7f6c52d8daf8e8d08cf517812009c6f9cc26/obs-websocket-dotnet/OBSWebsocket.cs#L797)
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    internal static string HashEncode(this string input)
    {
        using var sha256 = SHA256.Create();

        byte[] textBytes = Encoding.ASCII.GetBytes(input);
        byte[] hash = sha256.ComputeHash(textBytes);

        return System.Convert.ToBase64String(hash);
    }

    internal static string GetShortTimeFormat(this TimeSpan _timespan, TimeFormat timeFormat)
    {
        switch (timeFormat)
        {
            case TimeFormat.HOURS:
                if (_timespan.TotalDays >= 1)
                    return $"{(Math.Floor(_timespan.TotalHours).ToString().Length == 1 ? $"0{Math.Floor(_timespan.TotalHours)}" : Math.Floor(_timespan.TotalHours))}:" +
                        $"{(_timespan.Minutes.ToString().Length == 1 ? $"0{_timespan.Minutes}" : _timespan.Minutes)}:" +
                        $"{(_timespan.Seconds.ToString().Length == 1 ? $"0{_timespan.Seconds}" : _timespan.Seconds)}";

                if (_timespan.TotalHours >= 1)
                    return $"{(_timespan.Hours.ToString().Length == 1 ? $"0{_timespan.Hours}" : _timespan.Hours)}:" +
                        $"{(_timespan.Minutes.ToString().Length == 1 ? $"0{_timespan.Minutes}" : _timespan.Minutes)}:" +
                        $"{(_timespan.Seconds.ToString().Length == 1 ? $"0{_timespan.Seconds}" : _timespan.Seconds)}";

                return $"{(_timespan.Minutes.ToString().Length == 1 ? $"0{_timespan.Minutes}" : _timespan.Minutes)}:" +
                       $"{(_timespan.Seconds.ToString().Length == 1 ? $"0{_timespan.Seconds}" : _timespan.Seconds)}";
            case TimeFormat.DAYS:
                if (_timespan.TotalDays >= 1)
                    return $"{(Math.Floor(_timespan.TotalDays).ToString().Length == 1 ? $"0{Math.Floor(_timespan.TotalDays)}" : Math.Floor(_timespan.TotalDays))}" +
                            $"{(_timespan.Hours.ToString().Length == 1 ? $"0{_timespan.Hours}" : _timespan.Hours)}:" +
                            $"{(_timespan.Minutes.ToString().Length == 1 ? $"0{_timespan.Minutes}" : _timespan.Minutes)}:" +
                            $"{(_timespan.Seconds.ToString().Length == 1 ? $"0{_timespan.Seconds}" : _timespan.Seconds)}";

                if (_timespan.TotalHours >= 1)
                    return $"{(Math.Floor(_timespan.TotalHours).ToString().Length == 1 ? $"0{Math.Floor(_timespan.TotalHours)}" : Math.Floor(_timespan.TotalHours))}:" +
                            $"{(_timespan.Minutes.ToString().Length == 1 ? $"0{_timespan.Minutes}" : _timespan.Minutes)}:" +
                            $"{(_timespan.Seconds.ToString().Length == 1 ? $"0{_timespan.Seconds}" : _timespan.Seconds)}";

                return $"{(_timespan.Minutes.ToString().Length == 1 ? $"0{_timespan.Minutes}" : _timespan.Minutes)}:" +
                       $"{(_timespan.Seconds.ToString().Length == 1 ? $"0{_timespan.Seconds}" : _timespan.Seconds)}";

            case TimeFormat.MINUTES:
                if (_timespan.TotalHours >= 1)
                    return $"{(Math.Floor(_timespan.TotalMinutes).ToString().Length == 1 ? $"0{Math.Floor(_timespan.TotalMinutes)}" : Math.Floor(_timespan.TotalMinutes))}:" +
                            $"{(_timespan.Seconds.ToString().Length == 1 ? $"0{_timespan.Seconds}" : _timespan.Seconds)}";

                return $"{(_timespan.Minutes.ToString().Length == 1 ? $"0{_timespan.Minutes}" : _timespan.Minutes)}:" +
                       $"{(_timespan.Seconds.ToString().Length == 1 ? $"0{_timespan.Seconds}" : _timespan.Seconds)}";

            default:
                return _timespan.ToString();
        }
    }

    public enum TimeFormat
    {
        MINUTES,
        HOURS,
        DAYS
    }
}
