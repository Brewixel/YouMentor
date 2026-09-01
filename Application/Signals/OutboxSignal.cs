using System.Threading.Channels;

namespace Application.Signals;

public class OutboxSignal
{
	private readonly Channel<bool> _channel =
		Channel.CreateBounded<bool>(
			new BoundedChannelOptions(1)
			{
				SingleReader = true,
				SingleWriter = false,
				FullMode = BoundedChannelFullMode.DropOldest
			});

	public ChannelReader<bool> Reader => _channel.Reader;

	public void Notify()
	{
		_channel.Writer.TryWrite(true);
	}
}
