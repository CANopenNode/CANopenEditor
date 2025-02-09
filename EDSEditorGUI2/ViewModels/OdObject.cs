using CommunityToolkit.Mvvm.ComponentModel;
using libEDSsharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace EDSEditorGUI2.ViewModels
{
    public partial class OdObject : ObservableObject
    {
        [ObservableProperty]
        private bool _disabled;

        [ObservableProperty]
        string _name = string.Empty;

        [ObservableProperty]
        string _alias = string.Empty;

        [ObservableProperty]
        string _description = string.Empty;

        [ObservableProperty]
        LibCanOpen.OdObject.Types.ObjectType _objectType;

        [ObservableProperty]
        string _countLabel = string.Empty;

        [ObservableProperty]
        string _storageGroup = string.Empty;

        [ObservableProperty]
        bool flagsPDO;

        [ObservableProperty]
        ObservableCollection<KeyValuePair<string, OdSubObject>> _subObjects = [];

        /// <summary>
        /// very based on ODentry.AddSubEntry
        /// </summary>
        public OdSubObject? AddSubEntry(KeyValuePair<string, OdSubObject> selected)
        {
            if (ObjectType == LibCanOpen.OdObject.Types.ObjectType.Var)
                return null;

            OdSubObject newOd;

            //Do we need the type check??
            if ((SubObjects.Count == 0) && ((ObjectType == LibCanOpen.OdObject.Types.ObjectType.Array) || (ObjectType == LibCanOpen.OdObject.Types.ObjectType.Record)))
            {
                SubObjects.Add(new KeyValuePair<string, OdSubObject>("0", new OdSubObject
                {
                    Name = "Highest sub-index supported",
                    Type = LibCanOpen.OdSubObject.Types.DataType.Unsigned8,
                    Sdo = LibCanOpen.OdSubObject.Types.AccessSDO.Ro,
                    Pdo = LibCanOpen.OdSubObject.Types.AccessPDO.No,
                    Srdo = LibCanOpen.OdSubObject.Types.AccessSRDO.No,
                    DefaultValue = "0x01"
                }));
            }

            var lastSubOd = SubObjects.Last();
            UInt16 maxSubIndex = 1;
            UInt16 lastSubIndex = 1;

            // create new or clone existing sub od
            if (lastSubOd.Value == null || lastSubOd.Key.ToInteger() < 1)
            {
                newOd = new OdSubObject
                {
                    Name = "item",
                    Type = LibCanOpen.OdSubObject.Types.DataType.Unsigned32
                };
            }
            else
            {
                newOd = new OdSubObject
                {
                    //TODO: make a clone function with reflection to keep it up-to-date
                    Name = selected.Value.Name,
                    Alias = selected.Value.Alias,
                    Type = selected.Value.Type,
                    Sdo = selected.Value.Sdo,
                    Pdo = selected.Value.Pdo,
                    Srdo = selected.Value.Srdo,
                    DefaultValue = selected.Value.DefaultValue,
                    ActualValue = selected.Value.ActualValue,
                    LowLimit = selected.Value.LowLimit,
                    HighLimit = selected.Value.HighLimit,
                    StringLengthMin = selected.Value.StringLengthMin,
                };
            }

            // insert new sub od
            ObservableCollection<KeyValuePair<string, OdSubObject>> newSubObjects = [];
            UInt16 newSubIndex = 0;
            foreach (var sub in SubObjects)
            {
                var subOd = sub.Value;
                if (sub.Key.ToInteger() > newSubIndex)
                    newSubIndex = sub.Key.ToInteger();

                newSubObjects.Add(new KeyValuePair<string, OdSubObject>((newSubIndex++).ToHexString(), subOd));

                if (selected.Value == subOd)
                    newSubObjects.Add(new KeyValuePair<string, OdSubObject>((newSubIndex++).ToHexString(), newOd));
            }

            SubObjects = newSubObjects;

            // Write maxSubIndex to first sub index
            if (maxSubIndex > 0 && maxSubIndex == lastSubIndex && SubObjects.Count > 0)
            {
                SubObjects[0].Value.DefaultValue = string.Format("0x{0:X2}", newSubIndex - 1);
            }

            return newOd;
        }
        /// <summary>
        /// Remove current sub entry
        /// </summary>
        /// <param name="subobjectToRemove">Keyvalue pair of the subindex to be removed</param>
        /// <param name="renumber">Renumber subentries</param>
        /// <returns>true on successfull removal</returns>
        public bool RemoveSubEntry(KeyValuePair<string, OdSubObject> subObjectToRemove, bool renumber)
        {
            if (ObjectType == LibCanOpen.OdObject.Types.ObjectType.Array || ObjectType == LibCanOpen.OdObject.Types.ObjectType.Record)
            {
                UInt16 maxSubIndex = SubObjects[0].Value.DefaultValue.ToInteger();
                UInt16 lastSubIndex = SubObjects.Last().Key.ToInteger();

                SubObjects.Remove(subObjectToRemove);

                if (renumber)
                {
                    ObservableCollection<KeyValuePair<string, OdSubObject>> newSubObjects = [];
                    UInt16 subIndex = 0;
                    foreach (var subOd in SubObjects)
                        newSubObjects.Add(new KeyValuePair<string, OdSubObject>((subIndex++).ToHexString(), subOd.Value));

                    SubObjects = newSubObjects;
                }

                // Write maxSubIndex to first sub index
                if (maxSubIndex > 0 && maxSubIndex == lastSubIndex && SubObjects.Count > 0)
                {
                    SubObjects[0].Value.DefaultValue = string.Format("0x{0:X2}", SubObjects.Last().Key.ToInteger());
                }
                return true;
            }
            return false;
        }
    }
}
