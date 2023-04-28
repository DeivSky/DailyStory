using System.Text;

namespace DailyStory.Maui.Internal
{
	public static class DailyStoriesInterface
	{
		private static readonly string path = Path.Combine(AppContext.BaseDirectory, "stories.dat");
		private static readonly object @lock = new();

		public static async ValueTask<(Story[] stories, DateTime lastSaved)> LoadStoriesAsync()
		{
			return !File.Exists(path)
				? ((Story[] stories, DateTime lastSaved))(Array.Empty<Story>(), DateTime.MinValue)
				: await Task.Run(() => LoadStories());
		}

		public static async ValueTask SaveStoriesAsync(IEnumerable<Story> stories)
		{
			await Task.Run(() => SaveStories(stories));
		}

		public static (Story[] stories, DateTime lastSaved) LoadStories()
		{
			if (!File.Exists(path))
				return (Array.Empty<Story>(), DateTime.MinValue);

			var list = new List<Story>();
			DateTime lastSaved = DateTime.MinValue;

			lock (@lock)
			{
				using var stream = File.OpenRead(path);
				using var reader = new BinaryReader(stream, Encoding.UTF8);

				long dateBinary = reader.ReadInt64();
				try
				{
					lastSaved = DateTime.FromBinary(dateBinary);
				}
				catch(ArgumentException)
				{
					lastSaved = DateTime.MinValue;
					stream.Position -= sizeof(long);
				}

				dateBinary = reader.ReadInt64();
				while (dateBinary != long.MaxValue)
				{
					var date = lastSaved;
					try
					{
						date = DateTime.FromBinary(dateBinary);
					}
					catch (ArgumentException)
					{
						stream.Position -= sizeof(long);
					}
					list.Add(new Story(date, reader.ReadString()));
					dateBinary = reader.ReadInt64();
				}

				reader.Close();
			}

			return (list.ToArray(), lastSaved);
		}

		public static void SaveStories(IEnumerable<Story> stories)
		{
			var list = stories.ToList();
			list.Sort(new StoryComparerByDateDescending());

			lock (@lock)
			{
				using var stream = File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
				using var writer = new BinaryWriter(stream, Encoding.UTF8);

				writer.Write(DateTime.Now.ToBinary());

				foreach (var s in list)
				{
					writer.Write(s.dateTime.ToBinary());
					writer.Write(s.Text);
				}

				writer.Write(long.MaxValue);

				writer.Close();
			}
		}

		public class StoryComparerByDateAscending : IComparer<Story>
		{
			public int Compare(Story x, Story y)
			{
				return x.Date.CompareTo(y.Date);
			}
		}

		public class StoryComparerByDateDescending : IComparer<Story>
		{
			public int Compare(Story x, Story y)
			{
				return - x.Date.CompareTo(y.Date);
			}
		}
	}
}
