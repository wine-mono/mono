//
// Custom ChunkedMemoryStream.cs for System.Runtime.Remoting.Channels
//
// Authors:
//         Bernhard Kölbl <bkoelbl@codeweavers.com>
//
// Copyright (C) 2024 Bernhard Kölbl for CodeWeavers (https://www.codeweavers.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace System.Runtime.Remoting.Channels
{
	/// <summary>Provides an in-memory stream composed of non-contiguous chunks.</summary>
	internal sealed class ChunkedMemoryStream : Stream
	{
		private MemoryChunk _headChunk;
		private MemoryChunk _currentChunk;
		private MemoryChunk _currentReadChunk;

		private const int InitialChunkDefaultSize = 1024;
		private const int MaxChunkSize = 1024 * InitialChunkDefaultSize;
		private int _totalLength;
		private long _position;
		private int _inChunkOffset;

		internal ChunkedMemoryStream () { _position = 0; }

		public byte[] ToArray ()
		{
			byte[] result = new byte[_totalLength];
			int offset = 0;
			for (MemoryChunk chunk = _headChunk; chunk != null; chunk = chunk._next)
			{
				Debug.Assert (chunk._next == null || chunk._freeOffset == chunk._buffer.Length);
				Buffer.BlockCopy (chunk._buffer, 0, result, offset, chunk._freeOffset);
				offset += chunk._freeOffset;
			}
			return result;
		}

		public override void Write (byte[] buffer, int offset, int count)
		{
			while (count > 0)
			{
				if (_currentChunk != null)
				{
					int remaining = _currentChunk._buffer.Length - _currentChunk._freeOffset;
					if (remaining > 0)
					{
						int toCopy = Math.Min (remaining, count);
						Buffer.BlockCopy (buffer, offset, _currentChunk._buffer, _currentChunk._freeOffset, toCopy);
						count -= toCopy;
						offset += toCopy;
						_totalLength += toCopy;
						_currentChunk._freeOffset += toCopy;
						continue;
					}
				}

				AppendChunk (count);
			}
		}

		public override int Read (byte[] buffer, int offset, int count)
		{
			if (count + offset > buffer.Length)
				throw new ArgumentException ("Requested more bytes than the supplied buffer can hold.");

			if (_position == 0)
			{
				if ((_currentReadChunk = _headChunk) == null)
					throw new InvalidOperationException ("Internal state error.");
			}

			int readBytes = 0;

			while (count > 0 && _position < _totalLength)
			{
				int spaceLeftInChunk = _currentReadChunk._buffer.Length - _inChunkOffset;
				int toCopy = (int) Math.Min (_totalLength - _position, count);
				bool chunkTooSmall = spaceLeftInChunk < toCopy;

				toCopy = chunkTooSmall ? spaceLeftInChunk : toCopy;

				Buffer.BlockCopy (_currentReadChunk._buffer, _inChunkOffset, buffer, offset + readBytes, toCopy);

				if (chunkTooSmall)
				{
					_inChunkOffset = 0;
					_currentReadChunk = _currentReadChunk._next;
				}
				else _inChunkOffset += toCopy;

				readBytes += toCopy;
				_position += toCopy;
				count -= toCopy;
			}

			return readBytes;
		}

		private void AdvanceReadPosition (long amount)
		{
			if (_currentReadChunk == null)
				throw new InvalidOperationException ();

			while (amount > 0)
			{
				int spaceLeft = _currentReadChunk._buffer.Length - _inChunkOffset;

				// If we advance beyond the current chunk, go to the next.
				if (spaceLeft < amount)
				{
					amount -= spaceLeft;
					_position += spaceLeft;
					_currentReadChunk = _currentReadChunk._next;
					_inChunkOffset = 0;

					continue;
				}

				// Otherwise advance inside the current chunk
				_inChunkOffset += (int)amount;
				_position += amount;
				amount -= amount;
			}
		}

		public override Task WriteAsync (byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled (cancellationToken);
			}

			Write (buffer, offset, count);
			return Task.CompletedTask;
		}

		private void AppendChunk (long count)
		{
			int nextChunkLength = _currentChunk != null ? (int)Math.Min (_currentChunk._buffer.Length * 2, MaxChunkSize) : InitialChunkDefaultSize;
			if (count > nextChunkLength)
			{
				nextChunkLength = (int)Math.Min (count, MaxChunkSize);
			}

			MemoryChunk newChunk = new MemoryChunk (nextChunkLength);

			if (_currentChunk == null)
			{
				Debug.Assert (_headChunk == null);
				_headChunk = _currentChunk = newChunk;
			}
			else
			{
				Debug.Assert (_headChunk != null);
				_currentChunk._next = newChunk;
				_currentChunk = newChunk;
			}
		}

		private void SetCurrentPosition (long position)
		{
			if (position >= _totalLength)
			{
				throw new ArgumentOutOfRangeException (nameof(position));
			}

			if ((_currentReadChunk = _headChunk) == null)
				return;

			_position = _inChunkOffset = 0;

			AdvanceReadPosition (position);
		}

		public override bool CanRead => true;
		public override bool CanSeek => true;
		public override bool CanWrite => true;
		public override long Length => _totalLength;
		public override void Close () { }
		public override void Flush () { }
		public override Task FlushAsync (CancellationToken cancellationToken) => Task.CompletedTask;
		public override long Position { get { return _position; } set { SetCurrentPosition (value); } }
		public override void SetLength (long value) { throw new NotSupportedException (); }

		[MonoTODO ("Missing seeking")]
		public override long Seek (long offset, SeekOrigin origin) { throw new NotSupportedException (); }

		private sealed class MemoryChunk
		{
			internal readonly byte[] _buffer;
			internal int _freeOffset;
			internal MemoryChunk _next;

			internal MemoryChunk(int bufferSize) { _buffer = new byte[bufferSize]; }
		}
	}

}
