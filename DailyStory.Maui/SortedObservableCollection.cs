using System.Collections;
using System.Collections.Specialized;

namespace DailyStory.Maui
{
	public class SortedObservableCollection<T> : IList<T>, INotifyCollectionChanged
	{
		private readonly List<T> collection;
		private IComparer<T> comparer;

		public bool Notify
		{
			get => notify;
			set
			{
				notify = value;
				if (value)
					CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Reset));
			}
		}
		private bool notify = true;

		public IComparer<T> Comparer
		{
			get => comparer;
			set
			{
				if (comparer == value)
					return;

				comparer = value ?? Comparer<T>.Default;
				collection.Sort(comparer);
				if (Notify)
					CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Reset));
			}
		}

		public SortedObservableCollection()
		{
			collection = new List<T>();
			Comparer = Comparer<T>.Default;
		}

		public SortedObservableCollection(IEnumerable<T> collection)
		{
			this.collection = new List<T>(collection);
			Comparer = Comparer<T>.Default;
		}

		public SortedObservableCollection(int capacity)
		{
			collection = new List<T>(capacity);
			Comparer = Comparer<T>.Default;
		}

		public SortedObservableCollection(IComparer<T> comparer)
		{
			collection = new List<T>();
			Comparer = comparer;
		}

		public SortedObservableCollection(IComparer<T> comparer, IEnumerable<T> collection)
		{
			this.collection = new List<T>(collection);
			Comparer = comparer;
		}

		public SortedObservableCollection(IComparer<T> comparer, int capacity)
		{
			collection = new List<T>(capacity);
			Comparer = comparer;
		}

		public void Add(T item) => Add(item, out _);

		public void Add(T item, out int index)
		{
			if (Contains(item, out index))
				index++;
			else
				index = ~index;

			collection.Insert(index, item);
			if (Notify)
				CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Add, item, index));
		}

		public void AddRange(IEnumerable<T> items)
		{
			collection.AddRange(items);
			collection.Sort(comparer);
			if (Notify)
				CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Reset));
		}

		public void Clear()
		{
			if (Count == 0)
				return;

			collection.Clear();
			if (Notify)
				CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Reset));
		}

		public bool Contains(T item)
		{
			return Contains(item, out _);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			collection.CopyTo(array, arrayIndex);
		}

		public bool Remove(T item)
		{
			if (!Contains(item, out int i))
				return false;

			collection.RemoveAt(i);
			if (Notify)
				CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Remove, item, i));
			return true;
		}

		public int Count => collection.Count;
		public bool IsReadOnly => false;

		public int IndexOf(T item)
		{
			return collection.BinarySearch(item, comparer);
		}

		public void Insert(int index, T item)
		{
			if (Contains(item, out int i))
			{
				if (index == i || comparer.Compare(item, collection[index]) == 0)
					collection.Insert(index, item);
				else
					Throw(i + 1, index);
			}
			else
			{
				if (index == ~i)
					collection.Insert(index, item);
				else
					Throw(~i, index);
			}

			if (Notify)
				CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Add, item, index));
		}

		public void RemoveAt(int index)
		{
			var item = collection[index];
			collection.RemoveAt(index);
			if (Notify)
				CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Remove, item));
		}

		public T this[int index]
		{
			get => collection[index];
			set
			{
				var current = collection[index];
				if (comparer.Compare(current, value) != 0)
					Throw(Contains(value, out int i) ? i : ~i, index);

				if (Equals(current, value))
					return;

				collection[index] = value;
				if (Notify)
					CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Replace, value, current, index));
			}
		}

		public T this[T item]
		{
			get => this[IndexOf(item)];
			set
			{
				if (Contains(item, out int index))
				{
					var current = collection[index];
					if (Equals(current, value))
						return;

					collection[index] = value;
					if (Notify)
						CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Replace, value, current, index));
				}
				else
				{
					collection.Insert(~index, value);
					if (Notify)
						CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Add, item, ~index));
				}
			}
		}

		public bool Contains(T item, out int index)
		{
			return (index = IndexOf(item)) >= 0;
		}

		public IEnumerator<T> GetEnumerator()
		{
			return collection.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public event NotifyCollectionChangedEventHandler CollectionChanged;

		private void Throw(int expected, int received)
		{
			throw new NotSupportedException($"Invalid insertion index. Expected: {expected}, received: {received}.");
		}
	}
}
