namespace System.Diagnostics.Eventing
{
	public struct EventDescriptor
	{
		public EventDescriptor (int id, byte version, byte channel, byte level, byte opcode, int task, long keywords)
		{
			EventId = id;
			Version = version;
			Channel = channel;
			Level = level;
			Opcode = opcode;
			Task = task;
			Keywords = keywords;
		}

		public int EventId { get; }
		public byte Version { get; }
		public byte Channel { get; }
		public byte Level { get; }
		public byte Opcode { get; }
		public int Task { get; }
		public long Keywords { get; }
	}
}
