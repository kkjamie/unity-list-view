using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityListView
{
	public class ListView : MonoBehaviour
	{
		private readonly Dictionary<object, GameObject> items = new Dictionary<object, GameObject>();
		public Dictionary<object, GameObject> Items
		{
			get { return items; }
		}

		[SerializeField]
		private GameObject itemTemplate;

		private void Start()
		{
			itemTemplate.SetActive(false);
		}

		public List<GameObject> Init<TAny>(IEnumerable<TAny> source)
		{
			return Init<TAny, IListViewItem<TAny>>(source,
				(item, component) => component.Init(item));
		}

		public List<GameObject> Init<TAny, TInitArgs>(IEnumerable<TAny> source, TInitArgs args)
		{
			return Init<TAny, IListViewItem<TAny, TInitArgs>>(source,
				(item, component) => component.Init(item, args));
		}

		public List<GameObject> Init<TAny, TComponent>(IEnumerable<TAny> source, Action<TAny, TComponent> initFunction)
		{
			Clear();

			var result = new List<GameObject>();
			foreach (var item in source)
			{
				var itemObject = AddItem(item, initFunction);
				result.Add(itemObject);
			}

			return result;
		}

		public GameObject AddItem<TAny>(TAny item)
		{
			return AddItem<TAny, IListViewItem<TAny>>(item, (i, component) => component.Init(i));
		}

		public GameObject AddItem<TAny, TInitArgs>(TAny item, TInitArgs args)
		{
			return AddItem<TAny, IListViewItem<TAny, TInitArgs>>(item,
				(i, component) => component.Init(i, args));
		}

		public GameObject AddItem<TAny, TComponent>(TAny item, Action<TAny, TComponent> initFunction)
		{
			var itemObject = Instantiate(itemTemplate);
			itemObject.transform.SetParent(transform, false);
			itemObject.SetActive(true);

			var component = itemObject.GetComponent<TComponent>();
			if (component != null)
			{
				initFunction(item, component);
			}

			itemObject.AddComponent<ListViewElement>().OnDestroyed += () => items.Remove(item);

			items.Add(item, itemObject);

			return itemObject;
		}

		public void RemoveItem<TAny>(TAny item)
		{
			if (items.ContainsKey(item))
			{
				var itemObj = items[item];
				Destroy(itemObj.gameObject);
			}
			else
			{
				throw new Exception("No list item found for the given list element");
			}
		}

		public void Clear()
		{
			foreach (Transform child in transform)
			{
				if (child.gameObject != itemTemplate)
				{
					Destroy(child.gameObject);
				}
			}

			Items.Clear();
		}

		public T GetModelForChild<T>(GameObject child)
		{
			if (child.transform.parent != transform)
			{
				throw new Exception("The specified game object is not a child of the list view");
			}

			if (items.All(kvp => kvp.Value != child))
			{
				throw new Exception("The specified game object is a child but doesn't represent a list view item");
			}

			var key = items.First(kvp => kvp.Value == child).Key;
			if (!(key is T))
			{
				throw new Exception("The model for the given game object doesn't match the specified type");
			}

			return (T) key;
		}

		public interface IListViewItem<T>
		{
			void Init(T model);
		}

		public interface IListViewItem<T, TInitArgs>
		{
			void Init(T model, TInitArgs args);
		}

		private class ListViewElement : MonoBehaviour
		{
			public event Action OnDestroyed;

			private void OnDestroy()
			{
				if (OnDestroyed != null)
				{
					OnDestroyed();
					OnDestroyed = null;
				}
			}
		}
	}
}