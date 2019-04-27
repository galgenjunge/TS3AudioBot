// TS3AudioBot - An advanced Musicbot for Teamspeak 3
// Copyright (C) 2017  TS3AudioBot contributors
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the Open Software License v. 3.0
//
// You should have received a copy of the Open Software License along with this
// program. If not, see <https://opensource.org/licenses/OSL-3.0>.

namespace TS3AudioBot.ResourceFactories.AudioTags
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;

	public static class M3uReader
	{
		private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();
		private const int MaxLineLength = 4096;
		private const int MaxListLength = 1000;
		private static readonly byte[] ExtM3uLine = Encoding.UTF8.GetBytes("#EXTM3U");
		private static readonly byte[] ExtInfLine = Encoding.UTF8.GetBytes("#EXTINF");
		private static readonly byte[] ExtXStreamInfLine = Encoding.UTF8.GetBytes("#EXT-X-STREAM-INF");

		public static R<List<M3uEntry>, string> TryGetData(Stream stream)
		{
			int read = 1;
			int bufferLen = 0;
			var buffer = new byte[MaxLineLength];
			var data = new List<M3uEntry>();
			string trackTitle = null;
			string trackStreamMeta = null;
			//bool extm3u = false;

			try
			{
				for (int i = 0; i < MaxListLength; i++)
				{
					if (read > 0)
					{
						read = stream.Read(buffer, bufferLen, MaxLineLength - bufferLen);
						bufferLen += read;
					}

					// find linebreak index
					int index = Array.IndexOf(buffer, (byte)'\n', 0, bufferLen);
					int lb = 1;
					if (index == -1)
						index = Array.IndexOf(buffer, (byte)'\r', 0, bufferLen);
					else if (index > 0 && buffer[index - 1] == (byte)'\r')
					{
						index--;
						lb = 2;
					}

					ReadOnlySpan<byte> line;
					bool atEnd = index == -1;
					if (atEnd)
					{
						if (bufferLen == MaxLineLength)
							return "Max read buffer exceeded";
						line = buffer.AsSpan(0, bufferLen);
						bufferLen = 0;
					}
					else
					{
						line = buffer.AsSpan(0, index);
					}

					if (!line.IsEmpty)
					{
						if (line[0] == (byte)'#')
						{
							if (line.StartsWith(ExtInfLine))
							{
								var dataSlice = line.Slice(ExtInfLine.Length + 1);
								var trackInfo = dataSlice.IndexOf((byte)',');
								if (trackInfo >= 0)
									trackTitle = AsString(dataSlice.Slice(trackInfo + 1));
							}
							else if (line.StartsWith(ExtM3uLine))
							{
								//extm3u = true; ???
							}
							else if (line.StartsWith(ExtXStreamInfLine))
							{
								trackStreamMeta = AsString(line.Slice(ExtXStreamInfLine.Length + 1));
							}
							// else: unsupported m3u tag
						}
						else
						{
							var lineStr = AsString(line);
							if (Uri.TryCreate(lineStr, UriKind.RelativeOrAbsolute, out _))
							{
								data.Add(new M3uEntry()
								{
									TrackUrl = lineStr,
									Title = trackTitle,
									StreamMeta = trackStreamMeta,
								});
							}
							else
							{
								Log.Debug("Skipping invalid playlist entry ({0})", lineStr);
							}
							trackTitle = null;
							trackStreamMeta = null;
						}
					}

					if (!atEnd)
					{
						index += lb;
						Array.Copy(buffer, index, buffer, 0, MaxLineLength - index);
						bufferLen -= index;
					}

					if (atEnd || bufferLen <= 0)
					{
						if (bufferLen < 0)
							return "Unexpected buffer underfill";
						return data;
					}
				}
				return "List too long";
			}
			catch { return "Unexpected m3u parsing error"; }
		}

		private static string AsString(ReadOnlySpan<byte> data)
		{
			return Encoding.UTF8.GetString(data.ToArray()); // TODO with nc 2.1
		}
	}

	public class M3uEntry
	{
		public string TrackUrl { get; set; }
		public string Title { get; set; }
		public string StreamMeta { get; set; }
	}
}
