using Digits;

namespace DigitDesktop;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Offset.Text = 6000.ToString();
        RecordCount.Text = 390.ToString();
        OutputSize.Text = "1.0";

        Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        LeftControl.Items.Add(new ComboBoxItem
        {
            Content = "Await (non-parallel)",
            Tag = "DigitDesktop.AwaitNonParallel, DigitDesktop",
            IsSelected = true
        });

        LeftControl.Items.Add(new ComboBoxItem
        {
            Content = "Basic Task (parallel)",
            Tag = "DigitDesktop.BasicTask, DigitDesktop"
        });

        LeftControl.Items.Add(new ComboBoxItem
        {
            Content = "Task with max parallel (parallel)",
            Tag = "DigitDesktop.TaskWithMaxParallel, DigitDesktop"
        });

        LeftControl.Items.Add(new ComboBoxItem
        {
            Content = "Parallel.ForEachAsync (parallel)",
            Tag = "DigitDesktop.ParallelForeachAsync, DigitDesktop"
        });

        LeftControl.Items.Add(new ComboBoxItem
        {
            Content = "Basic Channel (parallel)",
            Tag = "DigitDesktop.BasicChannel, DigitDesktop"
        });

        LeftControl.Items.Add(new ComboBoxItem
        {
            Content = "Channel with max parallel (parallel)",
            Tag = "DigitDesktop.ChannelWithMaxParallel, DigitDesktop"
        });

        RightControl.Items.Add(new ComboBoxItem
        {
            Content = "Await (non-parallel)",
            Tag = "DigitDesktop.AwaitNonParallel, DigitDesktop",
            IsSelected = true
        });

        RightControl.Items.Add(new ComboBoxItem
        {
            Content = "Basic Task (parallel)",
            Tag = "DigitDesktop.BasicTask, DigitDesktop"
        });

        RightControl.Items.Add(new ComboBoxItem
        {
            Content = "Task with max parallel (parallel)",
            Tag = "DigitDesktop.TaskWithMaxParallel, DigitDesktop"
        });

        RightControl.Items.Add(new ComboBoxItem
        {
            Content = "Parallel.ForEachAsync (parallel)",
            Tag = "DigitDesktop.ParallelForeachAsync, DigitDesktop"
        });

        RightControl.Items.Add(new ComboBoxItem
        {
            Content = "Basic Channel (parallel)",
            Tag = "DigitDesktop.BasicChannel, DigitDesktop"
        });

        RightControl.Items.Add(new ComboBoxItem
        {
            Content = "Channel with max parallel (parallel)",
            Tag = "DigitDesktop.ChannelWithMaxParallel, DigitDesktop"
        });
    }

    private async void GoButton_Click(object sender, RoutedEventArgs e)
    {
        LeftPanel.Children.Clear();
        RightPanel.Children.Clear();

        string fileName = AppDomain.CurrentDomain.BaseDirectory + "train.csv";

        int offset = int.Parse(Offset.Text);
        int recordCount = int.Parse(RecordCount.Text);
        double displayMultipler = double.Parse(OutputSize.Text);

        (DigitImage[] rawTrain, DigitImage[] rawValidation) = await Task.Run(() => FileLoader.GetData(fileName, offset, recordCount));

        var manhattanClassifier = new ManhattanClassifier(rawTrain);
        var euclideanClassifier = new EuclideanClassifier(rawTrain);
        var k5EuclideanClassifier = new K5EuclideanClassifier(rawTrain);

        Classifier GetClassifier(string? classifierType)
        {
            return classifierType switch
            {
                "Euclidean Classifier" => euclideanClassifier,
                "Manhattan Classifier" => manhattanClassifier,
                _ => manhattanClassifier,
            };
        }

        // Left Panel (Panel 1)
        var panel1Recognizer = GetLeftRecognizerControl();
        if (panel1Recognizer is not null)
        {
            LeftPanel.Children.Add(panel1Recognizer);

            Classifier leftClassifier = GetClassifier(
                GetComboBoxSelectedContent(LeftClassifier));

            MessageBox.Show("Ready to start panel #1");
            await panel1Recognizer.Start(rawValidation, leftClassifier);
        }

        // Right Panel (Panel 2)
        var panel2Recognizer = GetRightRecognizerControl();
        if (panel2Recognizer is not null)
        {
            RightPanel.Children.Add(panel2Recognizer);

            Classifier rightClassifier = GetClassifier(
                GetComboBoxSelectedContent(RightClassifier));

            MessageBox.Show("Ready to start panel #2");
            await panel2Recognizer.Start(rawValidation, rightClassifier);
        }
        MessageBox.Show("Done");
    }

    private RecognizerControl? GetLeftRecognizerControl()
    {
        string classtype = GetComboBoxSelectedContent(LeftClassifier);
        string rectype = GetComboBoxSelectedTag(LeftControl);
        Type? recognizerType = Type.GetType(rectype);
        if (recognizerType is null) return null;
        object? control = Activator.CreateInstance(recognizerType, new object[] { classtype, 1.0 });
        return control as RecognizerControl;
    }

    private RecognizerControl? GetRightRecognizerControl()
    {
        string classtype = GetComboBoxSelectedContent(RightClassifier);
        string rectype = GetComboBoxSelectedTag(RightControl);
        Type? recognizerType = Type.GetType(rectype);
        if (recognizerType is null) return null;
        object? control = Activator.CreateInstance(recognizerType, new object[] { classtype, 1.0 });
        return control as RecognizerControl;
    }

    private string GetComboBoxSelectedContent(ComboBox box)
    {
        return ((ComboBoxItem)box.SelectedItem).Content.ToString() ?? "";
    }

    private string GetComboBoxSelectedTag(ComboBox box)
    {
        return ((ComboBoxItem)box.SelectedItem).Tag.ToString() ?? "";
    }
}
