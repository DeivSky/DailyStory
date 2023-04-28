using DailyStory.Maui.Internal;
using static DailyStory.Maui.Internal.DailyStoriesInterface;

namespace DailyStory.Maui;

public partial class MainPage : ContentPage
{
	private readonly SortedObservableCollection<Story> stories = new(new StoryComparerByDateDescending());
	private Story todaysStory = new(DateTime.Now);
	private DateTime lastSaved = DateTime.MinValue;

	public MainPage()
	{
		InitializeComponent();
		Initialize();

#if DEBUG && ENABLE_DEBUG_BUTTONS
		var layout = new HorizontalStackLayout();

		var configureButton = (Layout parent, string text, EventHandler action) =>
		{
			var button = new Button { Text = text };
			button.Clicked += action;
			parent.Children.Add(button);
		};

		configureButton(layout, "Clear", (o, e) => stories.Clear());
		configureButton(layout, "Fill", (o, e) => {
			var latest = stories[0].Date.ToDateTime(TimeOnly.MinValue);
			for (int i = 0; i < 10; i++)
			{
				stories.Add(new Story(latest.AddDays(i + 1), stories.Count.ToString()));
			}
		});
		configureButton(layout, "Save", OnSaveLogClicked);

		StackLayout.Children.Add(layout);
#endif
	}

	private async void Initialize()
	{
		await Init();
	}

	private async Task Init()
	{
		stories.Clear();

		(var arr, lastSaved) = await LoadStoriesAsync();
		stories.AddRange(arr);

		if (stories.Contains(todaysStory, out int index))
			todaysStory = stories[index];

		List.ItemsSource = stories;

		Editor.Text = todaysStory.Text;
		Editor.TextChanged += Editor_TextChanged;

		SetModTagText();
	}

	private void Editor_TextChanged(object sender, TextChangedEventArgs e)
	{
		SetModTagText();
	}

	private async void OnSaveLogClicked(object sender, EventArgs e)
	{
		await Save();
	}

	private async ValueTask Save()
	{
		stories.Notify = false;

		todaysStory.Text = Editor.Text;
		stories[todaysStory] = todaysStory;

		var list = stories.Where(s => !string.IsNullOrEmpty(s.Text));

		await SaveStoriesAsync(list);

		await Init();

		stories.Notify = true;
	}

	private void SetModTagText()
	{
		ModTag.Text = Editor.Text.Equals(todaysStory.Text) ?
			lastSaved.Equals(DateTime.MinValue) ? string.Empty : $"Last saved: {lastSaved}"
			: "Modified";
	}
}
