
public static class StreamHelper  
{
	public static async Task ResetStreamPosition(Stream stream)
	{
		stream.Position = 0;
	}
}