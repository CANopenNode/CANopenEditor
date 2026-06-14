using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using EDSEditorGUI2.ViewModels;
using EDSEditorGUI2.Views;

namespace GUITests;

public class ImportTests : IDisposable
{
    readonly MainWindow window;
    readonly MainWindowViewModel dc;
    readonly MenuItem? profileMenu;
    readonly TextBox? offsetsTextBox;

    readonly ComboBox? target;
    readonly Button? insert;
    readonly Button? cancel;

    public ImportTests()
    {
        dc = new MainWindowViewModel();
        window = new MainWindow
        {
            DataContext = dc
        };
        window.Show();

        // add device and select it
        dc.AddNewDevice(window);
        var deviceList = window.GetVisualDescendants().OfType<ListBox>().First();

        Dispatcher.UIThread.RunJobs();
        deviceList.SelectedItem = dc.Network[0];

        // import profile
        profileMenu = window.Find<MenuItem>("profileMenu");
        Assert.NotNull(profileMenu);
        profileMenu.Open();
        var DS301Menu = profileMenu.Items.OfType<MenuItem>().Where(x => x.Header!.ToString() == "DS301_profile.xpd").First();
        DS301Menu.Focus();
        window.KeyPressQwerty(PhysicalKey.Enter, RawInputModifiers.None);

        // fetch common ctrls
        offsetsTextBox = window.Find<TextBox>("InsertObjects_Offsets");
        Assert.NotNull(offsetsTextBox);

        target = window.Find<ComboBox>("InsertObjects_target");
        Assert.NotNull(target);
        insert = window.Find<Button>("InsertObjects_Insert");
        Assert.NotNull(insert);
        cancel = window.Find<Button>("InsertObjects_Cancel");
        Assert.NotNull(cancel);
    }

    [AvaloniaFact]
    public void CollisionCheckImportedOnly()
    {
        Assert.Single(dc.MergeStatus[0].Offsets);

        // set two offset numbers
        offsetsTextBox!.Text = "0 1";
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(2, dc.MergeStatus[0].Offsets.Count);

        var numberOfOffsets = dc.MergeStatus[0].Offsets.Count;
        for (int i = 0; i < numberOfOffsets; i++)
        {
            foreach (var leftRow in dc.MergeStatus)
            {
                int rightCollumIndex = leftRow.Offsets[i].Index;
                for (int j = i; j >= 0; j--)
                {
                    if (j != i)
                    {
                        foreach (var rightRow in dc.MergeStatus)
                        {
                            int leftCollumIndex = rightRow.Offsets[j].Index;
                            if (rightCollumIndex == leftCollumIndex)
                            {
                                Assert.True(leftRow.Offsets[i].Collision);
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Check that profilelist menu is correct
    /// </summary>
    [AvaloniaFact]
    public void ProfileMenuList()
    {
        Dictionary<string, bool> expectedEntries = [];

        List<string> profilelist = [.. Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "Profiles"))];
        string homepath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), ".edseditor");
        homepath = Path.Combine(homepath, "profiles");

        if (Directory.Exists(homepath))
        {
            profilelist.AddRange(Directory.GetFiles(homepath));
        }

        foreach (string file in profilelist)
        {
            string ext = Path.GetExtension(file).ToLower();
            if (ext == ".xpd" || ext == ".xdd")
            {
                var fileName = Path.GetFileName(file);
                expectedEntries[fileName] = false;
            }
        }

        foreach (var entry in expectedEntries)
        {
            var profileEntry = profileMenu!.Items.OfType<MenuItem>().Where(x => x.Header!.ToString() == entry.Key);
            Assert.Single(profileEntry);
        }
    }

    [AvaloniaFact]
    public void OffsettTextContainingNotValid()
    {
        Assert.Single(dc.MergeStatus[0].Offsets);

        // check that the system handles 
        // set two offset numbers
        offsetsTextBox!.Text = "something that is not a number";
        Dispatcher.UIThread.RunJobs();
        Assert.Empty(dc.MergeStatus[0].Offsets);

        offsetsTextBox!.Text = "10 and something else and then a number: 20";
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(2, dc.MergeStatus[0].Offsets.Count);
        Assert.Equal(10, dc.MergeStatus[0].Offsets[0].Index - dc.MergeStatus[0].OriginalIndex);
        Assert.Equal(20, dc.MergeStatus[0].Offsets[1].Index - dc.MergeStatus[0].OriginalIndex);

        offsetsTextBox!.Text = "10 and something else and then a number: 20";
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(2, dc.MergeStatus[0].Offsets.Count);
        Assert.Equal(10, dc.MergeStatus[0].Offsets[0].Index - dc.MergeStatus[0].OriginalIndex);
        Assert.Equal(20, dc.MergeStatus[0].Offsets[1].Index - dc.MergeStatus[0].OriginalIndex);

        //Testing that very big numbers will be interpreted as 0
        offsetsTextBox!.Text = "99999999999999999999";
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(0, dc.MergeStatus[0].Offsets[0].Index - dc.MergeStatus[0].OriginalIndex);
    }

    [AvaloniaFact]
    public void ImportWithoutConflict()
    {
        Assert.Single(dc.MergeStatus[0].Offsets);
        Dispatcher.UIThread.RunJobs();
        insert!.Focus();

        var copyOfMergeStatus = new List<ODIndexMergeStatus>(dc.MergeStatus);
        // press enter to import.
        window.KeyPressQwerty(PhysicalKey.Enter, RawInputModifiers.None);

        // check that its no longer using memory
        Assert.Empty(dc.MergeStatus);

        foreach (var index in copyOfMergeStatus)
        {
            foreach (var offset in index.Offsets)
            {
                int expectedIndex = offset.Index;
                var resultDec = dc.SelectedDevice!.Objects.ContainsKey(expectedIndex.ToString());
                var resultHex = dc.SelectedDevice!.Objects.ContainsKey(expectedIndex.ToString("X4"));
                Assert.True(resultDec || resultHex);
            }
        }
    }

    [AvaloniaFact]
    public void ImportWithConflict()
    {
        offsetsTextBox!.Text = "0 2 1";
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(3, dc.MergeStatus[0].Offsets.Count());
        insert!.Focus();

        // press enter to import.
        window.KeyPressQwerty(PhysicalKey.Enter, RawInputModifiers.None);
        //window.CaptureRenderedFrame()!.Save("file.png");

        // check that its no longer using memory
        Assert.Empty(dc.MergeStatus);

        //1002 Check that 1002 is merged from the first offsett without collision
        var index1002 = dc.SelectedDevice!.Objects["1002"];
        Assert.Equal("Manufacturer status register", index1002.Name);

        //1004 Check that 1004 is merged from the first offsett without collision
        var index1004 = dc.SelectedDevice!.Objects["1004"];
        Assert.Equal("Manufacturer status register", index1004.Name);
    }

    [AvaloniaFact]
    public void NotImportingUnselectedEntries()
    {
        Dispatcher.UIThread.RunJobs();
        dc.MergeStatus[0].Insert = false;
        Dispatcher.UIThread.RunJobs();
        insert!.Focus();

        // press enter to import.
        window.KeyPressQwerty(PhysicalKey.Enter, RawInputModifiers.None);

        // Check that 1000 is not merged as it is not selected for insertion
        Assert.False(dc.SelectedDevice!.Objects.TryGetValue("1000", out var index1001));
    }

    [AvaloniaFact]
    public void CancelImport()
    {
        Dispatcher.UIThread.RunJobs();
        dc.MergeStatus[0].Insert = false;
        Dispatcher.UIThread.RunJobs();
        cancel!.Focus();

        // press enter to cansel import.
        window.KeyPressQwerty(PhysicalKey.Enter, RawInputModifiers.None);

        // check that its no longer using memory
        Assert.Empty(dc.MergeStatus);
    }

    public void Dispose()
    {
        window.Close();
    }
}
