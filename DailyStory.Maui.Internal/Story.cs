using System;

namespace DailyStory.Maui.Internal
{
	public struct Story
	{
		public readonly DateOnly Date => DateOnly.FromDateTime(dateTime);
		public string Text { get; set; }

		internal DateTime dateTime;

		public Story(DateTime dateTime) : this(dateTime, string.Empty) { }

		public Story(string text) : this(DateTime.Now, text) { }

		public Story(DateTime dateTime, string text)
		{
			this.dateTime = dateTime;
			Text = text;
		}


		public static implicit operator Story(DateTime date) => new(date, string.Empty);
		public static implicit operator Story(string text) => new(text);
	}
}