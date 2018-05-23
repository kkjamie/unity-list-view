# unity-list-view

A simple & easy component that makes instantiating lists of objects easier.

## Usage

1. Add component to the parent of the list view. This is usually the "Content" of a ScrollRect

2. Setup the template object as a child. This will be used to instantiate the other items.
 (You can leave this active, it will be deactivated when the list is initialized)

3. Get hold of the ListView and call one of the Init() functions and pass your data in

## API

### Initialization

To intialize the list from a collection of data there are 3 options.

#### Option 1
**`public List<GameObject> Init<TAny>(IEnumerable<TAny> source)`**

This function takes any collection, and for each element will instantiate one copy of the template.

##### Example

```c#
LevelConfig[] levels = GetLevels();

// setup the levels list (Note: the type parameters can usually be omitted since the are inferred by the type of the variable passed in)
listView.Init<LevelConfig>(levels);
```

Quite often we want to pass each element from the source into a script on the instantiated object that represents that element. To do this you can just add a script that implements `ListView.IListViewItem<T>`. This interface just requires `void Init(T item);`
If you implement this then each element will be automatically passed into the script on the instantiated object that represents that element.

#### Option 2
**`public List<GameObject> Init<TAny, TInitArgs>(IEnumerable<TAny> source, TInitArgs args)`**

Identical to the function above, with one extra argument. Quite often not only do we want to pass each item from the source array into the instantiated objects, but also pass an object that is common to all. This is useful for things like callbacks. To do we use a similar interface to above. This time it's `ListView.IListViewItem<T1, T2>` which requires `void Init(T1 item, T2 initArgs)`

##### Examples
Here we setup the levels as before, but pass in a function for handling levels clicked. All instantiated element implementing `ListView.IListViewItem<T1, T2>` will receive the same `HandleLevelClicked` function
```c#
// setup array and pass in a handler function for handling levels clicked
listView.Init<LevelConfig, Action<levelConfig>>(levels, HandleLevelClicked);
```

In this example we wrap several args into one object to pass through so we can handle multiple events as well as the last played character, for each element if the last played character is equal to their own they can change their visual state to represent that.
Again there is only 1 `args` object and all list elements receive the same instance
```c#
// We can also encapsulate several args into an object
var args = new CharacterListItemArgs(lastPlayedCharacter, HandleCharacterClicked, HandleDeleteCharacterClicked, HandleRenamedCharacterClicked);
listView.Init<Character, CharacterListItemArgs>(characters, args);
```

If the object doesn't contain a script that implements `ListView.IListViewItem<T1, T2>`, the function will still create the list of objects, but obviously you will not get any of the data you passed in in the list element instances.

#### Option 3
If you need more control, you can perform your own initialization, by specifying your own `initFunction`, that `initFunction` will give you back the item from the source array as well as a component from the object 
**`public List<GameObject> Init<TAny, TComponent>(IEnumerable<TAny> source, Action<TAny, TComponent> initFunction)`**

The type `TComponent` will be used to attempt to find a script on the instantiated object of that type. If it is found the `initFunction` will not be called

##### Example

```c#
// setup the levels list
listView.Init<LevelConfig, LevelListItemView>(levels, (levelConfig, levelListItemView) => {
    // this happens for each level item 
    levelListItemView.Init(levelConfig);
    // Here we can do any custom thing we need to each item, bind to events, manipulates state or cache the instances
});
```

### Other useful functions

#### Clear
**`void Clear()`**

Clears the list and destroys all the instantiated items (doesn't remove the template)

#### AddItem
**`GameObject AddItem<TAny>(TAny item)`**

**`GameObject AddItem<TAny, TInitArgs>(TAny item, TInitArgs args)`**

**`GameObject AddItem<TAny, TComponent>(TAny item, Action<TAny, TComponent> initFunction)`**

The above functions can be used to add individual items to the list. This maybe useful when you need to manually add extra items not in the source list. These items maybe of a differnet type to the source item list, and can be caught by a different implementation of the `IListViewItem`.

The API mimics that of the `Init` functions providing the same 3 function options. one with the item, one with item and init args, and finally one with a custom init function.

They all return the game object that was instantiated.

This function is used internally by the `Init` functions.

#### RemoveItem
**`void RemoveItem<TAny>(TAny item)`**

This functions allows you to remove an element from the list view, by passing in the data item that was used to create it. If it doesn't correspond with an item that was used to create a list view element then it will throw.
