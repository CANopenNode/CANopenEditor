using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

internal static class EditorRegressionTests
{
    private const BindingFlags Fields = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    private static Assembly editor;
    private static Assembly library;

    private static object Field(object instance, string name)
    {
        return instance.GetType().GetField(name, Fields).GetValue(instance);
    }

    private static object Call(object instance, string name, params object[] args)
    {
        MethodInfo method = instance.GetType().GetMethod(name, Fields);
        int count = method.GetParameters().Length;
        if (args.Length < count)
        {
            int supplied = args.Length;
            Array.Resize(ref args, count);
            for (int index = supplied; index < count; index++)
            {
                args[index] = Type.Missing;
            }
        }
        return method.Invoke(instance, args);
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new Exception(message);
        }
    }

    private static object Model()
    {
        return Activator.CreateInstance(library.GetType("libEDSsharp.EDSsharp", true));
    }

    private static void PendingDeviceInfoSave()
    {
        object model = Model();
        Type listType = typeof(List<>).MakeGenericType(model.GetType());
        object network = Activator.CreateInstance(listType);
        ((IList)network).Add(model);
        using (var view = (Control)Activator.CreateInstance(editor.GetType("ODEditor.DeviceView", true), new[] { model, network }))
        using (var form = (Form)Activator.CreateInstance(editor.GetType("ODEditor.ODEditor_MainForm", true)))
        {
            Call(view, "dispatch_updatedevice");
            object info = Field(view, "deviceInfoView");
            var product = (TextBox)Field(info, "textBox_productname");
            var baud = (CheckBox)Field(info, "checkBox_baud_500");
            var lss = (CheckBox)Field(info, "checkBox_lss");
            product.Text = "Generic regression device";
            baud.Checked = !baud.Checked;
            lss.Checked = !lss.Checked;
            Call(view, "dispatch_updateOD");
            Require(product.Text == "Generic regression device", "Unrelated OD refresh discarded pending DeviceInfo edits");
            Require((bool)model.GetType().GetProperty("Dirty").GetValue(model), "Pending text/checkbox edits do not mark the project dirty");
            string filename = Path.Combine(Path.GetTempPath(), "editor-save-" + Guid.NewGuid().ToString("N") + ".eds");
            Call(form, "dosave", view, filename, true, false);
            object loaded = Model();
            Call(loaded, "Loadfile", filename);
            object device = Field(loaded, "di");
            Require((string)Field(device, "ProductName") == product.Text, "Save lost a pending ProductName edit without Update");
            Require((bool)Field(device, "BaudRate_500") == baud.Checked, "Save lost a pending baudrate edit without Update");
            Require((bool)Field(device, "LSS_Supported") == lss.Checked, "Save lost a pending LSS edit without Update");
        }
        Console.WriteLine("PASS #196/#254/#192: real editor edit/save/reopen without Update preserves device fields and dirty state");
    }

    private static object Cell(object grid, int row, int column)
    {
        return grid.GetType().GetProperty("Item", new[] { typeof(int), typeof(int) }).GetValue(grid, new object[] { row, column });
    }

    private static void SparsePdoNumbers()
    {
        object model = Model();
        object helper = Activator.CreateInstance(library.GetType("libEDSsharp.PDOHelper", true), new[] { model });
        Call(helper, "addPDOslot", (ushort)0x1800);
        Call(helper, "addPDOslot", (ushort)0x1802);
        Call(helper, "buildmappingsfromlists", true);
        using (var view = (Control)Activator.CreateInstance(editor.GetType("ODEditor.DevicePDOView2", true)))
        {
            view.GetType().GetField("eds", Fields).SetValue(view, model);
            Call(view, "Init", true);
            Call(view, "UpdatePDOinfo");
            object grid = Field(view, "grid1");
            for (int number = 0; number < 8; number++)
            {
                object header = Cell(grid, 1, 3 + number * 8);
                Require(Convert.ToString(header.GetType().GetProperty("Value").GetValue(header)) == "Byte " + number,
                    "PDO byte header is missing");
                Require(Convert.ToInt32(header.GetType().GetProperty("ColumnSpan").GetValue(header)) == 8,
                    "PDO byte header does not span eight bits");
            }
            object first = Cell(grid, 2, 0);
            object third = Cell(grid, 3, 0);
            Require(Convert.ToString(first.GetType().GetProperty("Value").GetValue(first)) == "1", "First PDO number is incorrect");
            Require(Convert.ToString(third.GetType().GetProperty("Value").GetValue(third)) == "3", "Sparse PDO slots display row ordinal instead of actual PDO number");
        }
        Console.WriteLine("PASS #120: real PDO grid derives sparse PDO numbers from communication indexes");
        Console.WriteLine("PASS #269: all eight real grid byte headers and spans exist on Windows");
    }

    private static void AdditionalMetadata()
    {
        object model = Model();
        using (var info = (Control)Activator.CreateInstance(editor.GetType("ODEditor.DeviceInfoView", true)))
        {
            info.GetType().GetField("eds", Fields).SetValue(info, model);
            Call(info, "populatedeviceinfo");
            foreach (string name in new[] { "textBox_revisionNumber", "textBox_orderCode", "textBox_networkName" })
            {
                Require(info.GetType().GetField(name, Fields) != null, "Missing DeviceInfo control: " + name);
            }
            ((TextBox)Field(info, "textBox_revisionNumber")).Text = "1234";
            ((TextBox)Field(info, "textBox_orderCode")).Text = "STANDARD-DEVICE";
            ((TextBox)Field(info, "textBox_networkName")).Text = "Regression network";
            MethodInfo commit = info.GetType().GetMethod("CommitPendingChanges");
            if (commit != null)
            {
                commit.Invoke(info, null);
            }
            object filetype = Enum.Parse(library.GetType("libEDSsharp.InfoSection+Filetype", true), "File_DCF");
            string path = Path.Combine(Path.GetTempPath(), "editor-metadata-" + Guid.NewGuid().ToString("N") + ".dcf");
            Call(model, "Savefile", path, filetype);
            object loaded = Model();
            Call(loaded, "Loadfile", path);
            Require(Convert.ToUInt32(Field(Field(loaded, "di"), "RevisionNumber")) == 1234, "RevisionNumber did not roundtrip");
            Require((string)Field(Field(loaded, "di"), "OrderCode") == "STANDARD-DEVICE", "OrderCode did not roundtrip");
            Require((string)Field(Field(loaded, "dc"), "NetworkName") == "Regression network", "NetworkName did not roundtrip");
        }
        Console.WriteLine("PASS #193/#194: revision, order code and commissioning network name edit/save/reopen");
    }

    private static object AddScalar(object model)
    {
        Type objectType = library.GetType("libEDSsharp.ODentry", true);
        object entry = Activator.CreateInstance(objectType);
        objectType.GetProperty("Index").SetValue(entry, (ushort)0x2000);
        foreach (var pair in new[] { new[] { "parameter_name", "Generic value" }, new[] { "defaultvalue", "0" } })
        {
            objectType.GetField(pair[0]).SetValue(entry, pair[1]);
        }
        foreach (var pair in new[] { new[] { "objecttype", "VAR" }, new[] { "datatype", "UNSIGNED32" }, new[] { "accesstype", "rw" } })
        {
            FieldInfo field = objectType.GetField(pair[0]);
            field.SetValue(entry, Enum.Parse(field.FieldType, pair[1]));
        }
        ((IDictionary)Field(model, "ods")).Add((ushort)0x2000, entry);
        return entry;
    }

    private static void StorageGroupSelection()
    {
        object model = Model();
        object entry = AddScalar(model);
        Field(entry, "prop").GetType().GetField("CO_storageGroup").SetValue(Field(entry, "prop"), "CUSTOM_GROUP");
        Call(Field(model, "CO_storageGroups"), "Add", "CUSTOM_GROUP");
        using (var view = (Control)Activator.CreateInstance(editor.GetType("ODEditor.DeviceODView", true)))
        {
            Call(view, "PopulateObjectLists", model);
            view.GetType().GetField("selectedObject", Fields).SetValue(view, entry);
            Call(view, "PopulateObject");
            var combo = (ComboBox)Field(view, "comboBox_storageGroup");
            Require(Convert.ToString(combo.SelectedItem) == "CUSTOM_GROUP", "Custom storage group was not selected on load");
            combo.SelectedIndex = -1;
            Call(view, "Button_saveChanges_Click", null, EventArgs.Empty);
            Require((string)Field(Field(entry, "prop"), "CO_storageGroup") == "CUSTOM_GROUP", "Empty storage selection corrupted the existing group");
            Require(ReferenceEquals(Field(view, "selectedObject"), entry), "Object save lost the selected object");
            using (var cancel = new System.Windows.Forms.Timer { Interval = 50 })
            {
                cancel.Tick += (sender, args) =>
                {
                    foreach (Form form in Application.OpenForms)
                    {
                        if (form.GetType().Name == "NewItem")
                        {
                            cancel.Stop();
                            form.DialogResult = DialogResult.Cancel;
                            break;
                        }
                    }
                };
                cancel.Start();
                combo.SelectedItem = "Add...";
                cancel.Stop();
                Require(Convert.ToString(combo.SelectedItem) == "CUSTOM_GROUP", "Cancelling Add left the sentinel selected");
            }
        }
        Console.WriteLine("PASS #184/#116: custom storage survives cleared selection and save retains selected object");
    }

    private static void MappingInvalidationWarning()
    {
        object model = Model();
        object entry = AddScalar(model);
        FieldInfo mappingType = entry.GetType().GetField("PDOtype");
        mappingType.SetValue(entry, Enum.Parse(mappingType.FieldType, "optional"));
        object helper = Activator.CreateInstance(library.GetType("libEDSsharp.PDOHelper", true), new[] { model });
        Call(helper, "addPDOslot", (ushort)0x1800);
        object slot = ((IList)Field(helper, "pdoslots"))[0];
        var mapping = (IList)Field(slot, "Mapping");
        Type itemType = mapping.GetType().GetGenericArguments()[0];
        mapping.Add(itemType == entry.GetType() ? entry : Activator.CreateInstance(itemType, new object[] { entry, 32 }));
        Call(helper, "buildmappingsfromlists", true);
        using (var view = (Control)Activator.CreateInstance(editor.GetType("ODEditor.DeviceODView", true)))
        {
            Call(view, "PopulateObjectLists", model);
            view.GetType().GetField("selectedObject", Fields).SetValue(view, entry);
            Call(view, "PopulateObject");
            ((ComboBox)Field(view, "comboBox_accessPDO")).SelectedItem = "no";
            Call(view, "Button_saveChanges_Click", null, EventArgs.Empty);
            Form[] warnings = Application.OpenForms.Cast<Form>().Where(form => form.GetType().Name == "WarningsFrm").ToArray();
            Require(warnings.Length > 0, "Removing mapping eligibility while mapped did not show a warning");
            foreach (Form warning in warnings)
            {
                warning.Close();
            }
        }
        Console.WriteLine("PASS #182: actual object edit reports newly invalidated PDO mappings");
    }

    private static void RecordCreationAndDocumentClose()
    {
        object model = Model();
        using (var dialog = (Form)Activator.CreateInstance(editor.GetType("ODEditor.NewIndex", true), new object[] { model, (ushort)0x2200 }))
        {
            ((TextBox)Field(dialog, "textBox_name")).Text = "Generic record";
            ((RadioButton)Field(dialog, "radioButton_record")).Checked = true;
            Call(dialog, "Button_create_Click", null, EventArgs.Empty);
            object record = Field(dialog, "od");
            Require(record != null && Convert.ToString(Field(record, "objecttype")) == "RECORD", "Record dialog did not create a record");
            Require(((IDictionary)Field(record, "subobjects")).Count == 2, "New record is missing subentries");
        }
        Console.WriteLine("PASS #177: real creation dialog creates RECORD and subentries");
        using (var form = (Form)Activator.CreateInstance(editor.GetType("ODEditor.ODEditor_MainForm", true)))
        {
            var tabs = (TabControl)Field(form, "tabControl1");
            var network = (IList)Field(form, "network");
            tabs.TabPages.Clear();
            network.Clear();
            for (int number = 0; number < 2; number++)
            {
                object document = Model();
                network.Add(document);
                var page = new TabPage("Document " + number);
                page.Controls.Add((Control)Activator.CreateInstance(editor.GetType("ODEditor.DeviceView", true), new[] { document, network }));
                tabs.TabPages.Add(page);
            }
            tabs.SelectedIndex = 0;
            Call(form, "closeFileToolStripMenuItem_Click", null, EventArgs.Empty);
            Require(tabs.TabPages.Count == 1 && network.Count == 1, "Closing the first document removed the wrong number of documents");
            tabs.SelectedIndex = 0;
            Require(tabs.SelectedTab != null && ReferenceEquals(Field(tabs.SelectedTab.Controls[0], "eds"), network[0]),
                "Remaining document cannot be selected after closing the first");
        }
        Console.WriteLine("PASS #180: actual first-document close leaves the remaining document selectable");
    }

    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.Error.WriteLine("Usage: EditorRegressionTests.exe <editor deployment directory>");
            return 2;
        }
        using (var deadline = new System.Threading.Timer(state => Environment.Exit(2), null, 60000, Timeout.Infinite))
        {
            string directory = Path.GetFullPath(args[0]);
            Environment.CurrentDirectory = directory;
            AppDomain.CurrentDomain.AssemblyResolve += (sender, request) =>
            {
                string filename = Path.Combine(directory, new AssemblyName(request.Name).Name + ".dll");
                return File.Exists(filename) ? Assembly.LoadFrom(filename) : null;
            };
            library = Assembly.LoadFrom(Path.Combine(directory, "libEDSsharp.dll"));
            editor = Assembly.LoadFrom(Path.Combine(directory, "EDSEditor.exe"));
            Application.EnableVisualStyles();
            int failed = 0;
            foreach (Action test in new Action[] { PendingDeviceInfoSave, SparsePdoNumbers, AdditionalMetadata, StorageGroupSelection, MappingInvalidationWarning, RecordCreationAndDocumentClose })
            {
                try
                {
                    test();
                }
                catch (Exception error)
                {
                    failed++;
                    Console.Error.WriteLine("FAIL: " + error);
                }
            }
            return failed == 0 ? 0 : 1;
        }
    }
}
