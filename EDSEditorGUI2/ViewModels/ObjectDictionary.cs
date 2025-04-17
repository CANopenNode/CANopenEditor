using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;


//based on https://raw.githubusercontent.com/Cysharp/ObservableCollections/refs/heads/master/src/ObservableCollections/ObservableDictionary.cs

namespace EDSEditorGUI2.ViewModels;
public class ObjectDictionary : IDictionary<string, OdObject>, INotifyCollectionChanged, INotifyPropertyChanged
{
    #region ObservableDictionary
    private IDictionary<string, OdObject> _Dictionary;
    protected IDictionary<string, OdObject> Dictionary
    {
        get
        {
            return _Dictionary;
        }
    }

    public ObjectDictionary()
    {
        _Dictionary = new Dictionary<string, OdObject>();
    }

    public void Add(string key, OdObject value)
    {
        Insert(key, value, true);
    }

    public bool ContainsKey(string key)
    {
        return Dictionary.ContainsKey(key);
    }

    public ICollection<string> Keys
    {
        get { return Dictionary.Keys; }
    }

    public bool Remove(string key)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));

        var removed = Dictionary.Remove(key);
        if (removed)
        { OnCollectionChanged(); }
        return removed;
    }

    public bool TryGetValue(string key, out OdObject value)
    {
        return Dictionary.TryGetValue(key, out value);
    }

    public ICollection<OdObject> Values
    {
        get { return Dictionary.Values; }
    }

    public OdObject this[string key]
    {
        get
        {
            return Dictionary[key];
        }
        set
        {
            Insert(key, value, false);
        }
    }

    public void Add(KeyValuePair<string, OdObject> item)
    {
        Insert(item.Key, item.Value, true);
    }

    public void Clear()
    {
        if (Dictionary.Count > 0)
        {
            Dictionary.Clear();
            OnCollectionChanged();
        }
    }

    public bool Contains(KeyValuePair<string, OdObject> item)
    {
        return Dictionary.Contains(item);
    }

    public void CopyTo(KeyValuePair<string, OdObject>[] array, int arrayIndex)
    {
        Dictionary.CopyTo(array, arrayIndex);
    }

    public int Count
    {
        get { return Dictionary.Count; }
    }

    public bool IsReadOnly
    {
        get { return Dictionary.IsReadOnly; }
    }

    public bool Remove(KeyValuePair<string, OdObject> item)
    {
        return Remove(item.Key);
    }

    public IEnumerator<KeyValuePair<string, OdObject>> GetEnumerator()
    {
        return Dictionary.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)Dictionary).GetEnumerator();
    }

    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    public event PropertyChangedEventHandler? PropertyChanged;

    public void AddRange(IDictionary<string, OdObject> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        if (items.Count > 0)
        {
            if (Dictionary.Count > 0)
            {
                if (items.Keys.Any((k) => Dictionary.ContainsKey(k)))
                {
                    throw new ArgumentException("An item with the same key has already been added.");
                }
                else
                {
                    foreach (var item in items)
                    {
                        Dictionary.Add(item);
                    }
                }
            }
            else
            {
                _Dictionary = new Dictionary<string, OdObject>(items);
            }
            OnCollectionChanged(NotifyCollectionChangedAction.Add, items.ToArray());
        }
    }

    private void Insert(string key, OdObject value, bool add)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (Dictionary.TryGetValue(key, out var item))
        {
            if (add)
            {
                throw new ArgumentException("An item with the same key has already been added.");
            }
            if (Equals(item, value))
            {
                return;
            }
            Dictionary[key] = value;

            OnCollectionChanged(NotifyCollectionChangedAction.Replace, new KeyValuePair<string, OdObject>(key, value), new KeyValuePair<string, OdObject>(key, item));
        }
        else
        {
            Dictionary[key] = value;
            OnCollectionChanged(NotifyCollectionChangedAction.Add, new KeyValuePair<string, OdObject>(key, value));
        }
    }

    private void OnPropertyChanged()
    {
        OnPropertyChanged(nameof(Count));
        OnPropertyChanged("Item[]");
        OnPropertyChanged(nameof(Keys));
        OnPropertyChanged(nameof(Values));
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void OnCollectionChanged()
    {
        OnPropertyChanged();
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    private void OnCollectionChanged(NotifyCollectionChangedAction action, KeyValuePair<string, OdObject> changedItem)
    {
        OnPropertyChanged();
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, changedItem));
    }

    private void OnCollectionChanged(NotifyCollectionChangedAction action, KeyValuePair<string, OdObject> newItem, KeyValuePair<string, OdObject> oldItem)
    {
        OnPropertyChanged();
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, newItem, oldItem));
    }

    private void OnCollectionChanged(NotifyCollectionChangedAction action, IList newItems)
    {
        OnPropertyChanged();
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, newItems));
    }
    #endregion ObservableDictionary

    KeyValuePair<string, OdObject> _selectedObject;
    KeyValuePair<string, OdSubObject> _selectedSubObject;
    ObservableCollection<KeyValuePair<string, OdSubObject>> _selectedSubObjects = [];

    public void AddIndex(int index, string name, LibCanOpen.OdObject.Types.ObjectType type)
    {
        var strIndex = index.ToString("X4");
        var newObj = new OdObject
        {
            Name = name,
            ObjectType = type
        };

        // create OD entry
        if (type == LibCanOpen.OdObject.Types.ObjectType.Var)
        {
            var newSub = new OdSubObject()
            {
                Name = name,
                DataType = LibCanOpen.OdSubObject.Types.DataType.Unsigned32,
                Sdo = LibCanOpen.OdSubObject.Types.AccessSDO.Rw,
                Pdo = LibCanOpen.OdSubObject.Types.AccessPDO.No,
                Srdo = LibCanOpen.OdSubObject.Types.AccessSRDO.No,
                DefaultValue = "0"
            };
            newObj.SubObjects.Add(new KeyValuePair<string, OdSubObject>("0x0", newSub));
        }
        else
        {
            var CountSub = new OdSubObject()
            {
                Name = "Highest sub-index supported",
                DataType = LibCanOpen.OdSubObject.Types.DataType.Unsigned8,
                Sdo = LibCanOpen.OdSubObject.Types.AccessSDO.Ro,
                Pdo = LibCanOpen.OdSubObject.Types.AccessPDO.No,
                Srdo = LibCanOpen.OdSubObject.Types.AccessSRDO.No,
                DefaultValue = "0x01"
            };
            var Sub1 = new OdSubObject()
            {
                Name = "Sub Object 1",
                DataType = LibCanOpen.OdSubObject.Types.DataType.Unsigned32,
                Sdo = LibCanOpen.OdSubObject.Types.AccessSDO.Rw,
                Pdo = LibCanOpen.OdSubObject.Types.AccessPDO.No,
                Srdo = LibCanOpen.OdSubObject.Types.AccessSRDO.No,
                DefaultValue = "0"
            };

            newObj.SubObjects.Add(new KeyValuePair<string, OdSubObject>("0x0", CountSub));
            newObj.SubObjects.Add(new KeyValuePair<string, OdSubObject>("0x1", Sub1));
        }
        Add(new KeyValuePair<string, OdObject>(strIndex, newObj));
    }

    // This method is called by the Set accessor of each property.  
    // The CallerMemberName attribute that is applied to the optional propertyName  
    // parameter causes the property name of the caller to be substituted as an argument.  
    private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public KeyValuePair<string, OdObject> SelectedObject
    {
        get
        {
            return _selectedObject;
        }

        set
        {
            if (value.Key != _selectedObject.Key || value.Value != _selectedObject.Value)
            {
                this._selectedObject = value;
                NotifyPropertyChanged();
            }
        }
    }

    public KeyValuePair<string, OdSubObject> SelectedSubObject
    {
        get
        {
            return _selectedSubObject;
        }

        set
        {
            if (value.Key != _selectedSubObject.Key || value.Value != _selectedSubObject.Value)
            {
                _selectedSubObject = value;
                NotifyPropertyChanged();
            }
        }
    }

    public ObservableCollection<KeyValuePair<string, OdSubObject>> SelectedSubObjects
    {
        get
        {
            return _selectedSubObjects;
        }

        set
        {
            if (value != _selectedSubObjects)
            {
                _selectedSubObjects = value;
                NotifyPropertyChanged();
            }
        }
    }
}
